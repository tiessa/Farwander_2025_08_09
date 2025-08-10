using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using TJNK.Farwander.Core;
using TJNK.Farwander.Modules.Generation;
using TJNK.Farwander.Modules.Generation.Validators;
using TJNK.Farwander.Modules.Game.Runtime.Entities;
using TJNK.Farwander.Modules.Game.Runtime.Movement;
using TJNK.Farwander.Modules.Game.Runtime.State;
using TJNK.Farwander.Modules.AI;

namespace TJNK.Farwander.Modules.Game
{
    /// <summary>
    /// Step 1+2+3 controller: generation, player spawn/move, enemy spawn and wander cadence.
    /// This version fixes QueryRegistry property name and avoids compile-time dependency on a specific EnemyRoster shape.
    /// </summary>
    public sealed class GameControllerModuleProvider : ModuleProvider
    {
        [Header("Root Project Config")]
        public ProjectConfigSO Config;

        public override string ModuleId { get { return "Game"; } }

        private GameCore _core;
        private EventBus _bus; private TimedScheduler _sch; private ValidationPipeline _val; private QueryRegistry _q;
        private IDungeonGenerator _gen;

        private System.IDisposable _subGenRequest;
        private System.IDisposable _subGenComplete;
        private System.IDisposable _subInputMove;
        private System.IDisposable _subInputWait;
        private System.IDisposable _subMoveResolved;

        // Services
        private IEntityLocations _locs;
        private IPlayerState _player;
        private ICollisionPolicy _coll;
        private MovementService _moveSvc;
        private IEnemyRegistry _enemies;
        private EnemySpawner _spawner;
        private EnemyWanderBrain _brain;
        private int _aiTurnIndex = 0;

        public override void Bind(GameCore core)
        {
            base.Bind(core);
            _core = core; _bus = core.Bus; _sch = core.Scheduler; _val = core.Validation; _q = core.Queries; // fixed: Queries

            // Validators for generation (audit-only)
            _val.Register<DungeonMap>(new RoomsWithinBoundsValidator());
            _val.Register<DungeonMap>(new NoRoomOverlapValidator());
            _val.Register<DungeonMap>(new ConnectivityValidator());

            // Generator service
            _gen = new RectDungeonGenerator(_val);
            var genRef = _gen; _q.Register<IDungeonGenerator>(() => genRef);

            // Movement stack (Step 2)
            _locs = new EntityLocations();      _q.Register<IEntityLocations>(() => _locs);
            _player = new PlayerState();        _q.Register<IPlayerState>(() => _player);
            _coll   = new CollisionPolicy(() => _q.Get<IMapQuery>(), _locs);
                                               _q.Register<ICollisionPolicy>(() => _coll);
            _moveSvc = new MovementService(_bus, _sch, _locs, _coll); // subscribes to Move_Request

            // Enemies (Step 3)
            _enemies = new EnemyRegistry();     _q.Register<IEnemyRegistry>(() => _enemies);
            _spawner = new EnemySpawner(_bus, _sch, _locs, _enemies);

            // Wire generation pipeline
            _subGenRequest  = _bus.Subscribe<Gen_Request>(OnGenRequest);
            _subGenComplete = _bus.Subscribe<Gen_Complete>(OnGenComplete);

            // Input routing (Step 2)
            _subInputMove = _bus.Subscribe<Input_Move>(e =>
            {
                var id = _player.PlayerId; if (id == 0) return;
                _bus.Publish(new Move_Request { EntityId = id, Dir = e.Dir });
            });
            _subInputWait = _bus.Subscribe<Input_Wait>(_ =>
            {
                // Consume a tick and then trigger AI turn on next tick
                int turn = ++_aiTurnIndex;
                _sch.Schedule(_sch.Now + 1, EventPriority.Actor, EventLane.Action, this,
                    () => _bus.Publish(new AI_TurnStart { TurnIndex = turn }));
            });

            // After successful player move, schedule AI turn next tick
            _subMoveResolved = _bus.Subscribe<Move_Resolved>(e =>
            {
                if (!e.Succeeded || e.EntityId != _player.PlayerId) return;
                int turn = ++_aiTurnIndex;
                _sch.Schedule(_sch.Now + 1, EventPriority.Actor, EventLane.Action, this,
                    () => _bus.Publish(new AI_TurnStart { TurnIndex = turn }));
            });

            // Kick generation at tick 1 (Action)
            var dcfg0 = Config != null ? Config.Dungeon : null;
            var size      = new Vector2Int(dcfg0 != null ? dcfg0.Width    : 48, dcfg0 != null ? dcfg0.Height   : 32);
            var roomMin   = dcfg0 != null ? dcfg0.RoomMin : new Vector2Int(4, 3);
            var roomMax   = dcfg0 != null ? dcfg0.RoomMax : new Vector2Int(10, 7);
            var roomCount = dcfg0 != null ? dcfg0.RoomCount : 8;
            var seed      = dcfg0 != null ? dcfg0.Seed : 1337;

            _brain = new EnemyWanderBrain(_bus, _sch, _enemies, _locs, _coll, seed);

            _sch.Schedule(_sch.Now + 1, EventPriority.System, EventLane.Action, this, () =>
            {
                _bus.Publish(new Gen_Request { Seed = seed, Size = size, RoomMin = roomMin, RoomMax = roomMax, RoomCount = roomCount });
                Debug.Log("[GameController] Scheduled Gen_Request @ tick 1");
            });
        }

        private void OnGenRequest(Gen_Request req)
        {
            var map = _gen.Generate(req.Seed, req.Size, req.RoomMin, req.RoomMax, req.RoomCount);
            _sch.Schedule(_sch.Now, EventPriority.System, EventLane.Dispatch, this,
                () => _bus.Publish(new Gen_Complete { DungeonMap = map }));
        }

        private void OnGenComplete(Gen_Complete e)
        {
            var map = e.DungeonMap as DungeonMap; if (map == null) return;

            // Make IMapQuery available
            var mq = new MapQuery(map);
            _q.Register<IMapQuery>(() => mq);

            // Spawn player at first spawn (or center fallback)
            var id  = 1;
            var pos = map.SpawnPoints != null && map.SpawnPoints.Count > 0
                    ? map.SpawnPoints[0]
                    : new Vector2Int(map.Width / 2, map.Height / 2);

            _locs.Set(id, pos); _player.PlayerId = id;

            _sch.Schedule(_sch.Now, EventPriority.System, EventLane.Dispatch, this,
                () => _bus.Publish(new Entity_Spawned { EntityId = id, Pos = pos, IsPlayer = true, SpriteName = "player" }));

            // Spawn enemies from config (fallback to 5 if roster unspecified)
            int count = ComputeEnemyCountFromRoster(Config != null ? (object)Config.Enemies : null);
            var tint = new System.Func<int, Color32>(i =>
            {
                byte r = (byte)(200 - (i*23)%80);
                byte g = (byte)(60 + (i*13)%60);
                byte b = (byte)(60 + (i*7)%60);
                return new Color32(r,g,b,255);
            });
            if (count <= 0) count = 5;
            _spawner.SpawnAll(map, count, tint);

            // Recompute FOV next tick
            var dcfg = Config != null ? Config.Dungeon : null; int r = dcfg != null ? dcfg.FovRadius : 8;
            _sch.Schedule(_sch.Now + 1, EventPriority.Actor, EventLane.Action, this,
                () => _bus.Publish(new Fov_Recompute { ViewerId = id, Radius = r }));

            Debug.Log($"[GameController] Gen_Complete: size={map.Width}x{map.Height}, rooms={map.Rooms.Count}, spawns={map.SpawnPoints.Count}");
        }

        private static int ComputeEnemyCountFromRoster(object roster)
        {
            try
            {
                if (roster == null) return 5;
                // Try property or field named "Spawns" that is IEnumerable; sum each entry's Count (>0)
                var t = roster.GetType();
                object listObj = null;
                var p = t.GetProperty("Spawns", BindingFlags.Public | BindingFlags.Instance);
                if (p != null) listObj = p.GetValue(roster);
                if (listObj == null)
                {
                    var f = t.GetField("Spawns", BindingFlags.Public | BindingFlags.Instance);
                    if (f != null) listObj = f.GetValue(roster);
                }
                if (listObj is IEnumerable en)
                {
                    int total = 0;
                    foreach (var entry in en)
                    {
                        var et = entry.GetType();
                        int c = 0;
                        var pc = et.GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
                        if (pc != null)
                        {
                            var v = pc.GetValue(entry);
                            if (v != null) c = Convert.ToInt32(v);
                        }
                        else
                        {
                            var fc = et.GetField("Count", BindingFlags.Public | BindingFlags.Instance);
                            if (fc != null)
                            {
                                var v = fc.GetValue(entry);
                                if (v != null) c = Convert.ToInt32(v);
                            }
                        }
                        if (c > 0) total += c; // avoid Mathf.Max ambiguity; simple int compare
                    }
                    return total > 0 ? total : 5;
                }
                return 5;
            }
            catch { return 5; }
        }

        private void OnDestroy()
        {
            _subGenRequest?.Dispose();
            _subGenComplete?.Dispose();
            _subInputMove?.Dispose();
            _subInputWait?.Dispose();
            _subMoveResolved?.Dispose();
        }
    }
}
