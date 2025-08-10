using TJNK.Farwander.Core;
using TJNK.Farwander.Modules.Generation;
using TJNK.Farwander.Modules.Generation.Validators;
using UnityEngine;

namespace TJNK.Farwander.Modules.Game
{
    /// <summary>
    /// Single orchestrator Mono for gameplay. Holds ProjectConfigSO and boots systems.
    /// Implements Step 1 (Dungeon Generation) orchestration only.
    /// </summary>
    public sealed class GameControllerModuleProvider : ModuleProvider
    {
        [Header("Root Project Config")]
        public ProjectConfigSO Config;

        public override string ModuleId { get { return "Game"; } }

        private GameCore _core;
        private EventBus _bus; private TimedScheduler _sch; private ValidationPipeline _val; private QueryRegistry _q;
        private IDungeonGenerator _gen;
        private System.IDisposable _subGenComplete;

        public override void Bind(GameCore core)
        {
            base.Bind(core);
            _core = core; _bus = core.Bus; _sch = core.Scheduler; _val = core.Validation; _q = core.Queries;

            // Register validators (audit-only) for DungeonMap
            _val.Register<DungeonMap>(new RoomsWithinBoundsValidator());
            _val.Register<DungeonMap>(new NoRoomOverlapValidator());
            _val.Register<DungeonMap>(new ConnectivityValidator());

            // Register generator service
            _gen = new RectDungeonGenerator(_val);
            var genInstance = _gen; _q.Register<IDungeonGenerator>(() => genInstance);

            // Subscribe to generation request
            _bus.Subscribe<Gen_Request>(OnGenRequest);
            Debug.Log("[GameController] Scheduled Gen_Request @ tick 1");
            
            // Subscribe to complete to register IMapQuery for later steps
            _subGenComplete = _bus.Subscribe<Gen_Complete>(e =>
            {
                var map = e.DungeonMap as DungeonMap;
                if (map != null)
                {
                    var mq = new MapQuery(map);
                    _q.Register<IMapQuery>(() => mq);
                    Debug.Log($"[GameController] Gen_Complete: size={map.Width}x{map.Height}, rooms={map.Rooms.Count}, spawns={map.SpawnPoints.Count}");
                }
            });

            // Post Gen_Request at tick 1 via Action lane
            var dcfg = Config != null ? Config.Dungeon : null;
            var size = new Vector2Int(dcfg != null ? dcfg.Width : 48, dcfg != null ? dcfg.Height : 32);
            var roomMin = dcfg != null ? dcfg.RoomMin : new Vector2Int(4,3);
            var roomMax = dcfg != null ? dcfg.RoomMax : new Vector2Int(10,7);
            var roomCount = dcfg != null ? dcfg.RoomCount : 8;
            var seed = dcfg != null ? dcfg.Seed : 1337;

            _sch.Schedule(_sch.Now + 1, EventPriority.System, EventLane.Action, this, () =>
            {
                _bus.Publish(new Gen_Request { Seed = seed, Size = size, RoomMin = roomMin, RoomMax = roomMax, RoomCount = roomCount });
            });
        }

        private void OnGenRequest(Gen_Request req)
        {
            // Generate map (Action lane execution context)
            var map = _gen.Generate(req.Seed, req.Size, req.RoomMin, req.RoomMax, req.RoomCount);

            // Dispatch result at Now, respecting Dispatch lane rules
            _sch.Schedule(_sch.Now, EventPriority.System, EventLane.Dispatch, this, () =>
            {
                _bus.Publish(new Gen_Complete { DungeonMap = map });
            });
        }

        private void OnDestroy()
        {
            if (_subGenComplete != null) { _subGenComplete.Dispose(); _subGenComplete = null; }
        }
    }
}
