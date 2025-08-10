using System;
using System.Collections.Generic;
using UnityEngine;
using TJNK.Farwander.Core;
using TJNK.Farwander.Modules.Game;
using TJNK.Farwander.Modules.Game.Runtime.Entities;
using TJNK.Farwander.Modules.Game.Runtime.Movement;

namespace TJNK.Farwander.Modules.AI
{
    /// <summary>
    /// Deterministic random-walk AI: on AI_TurnStart(Action), each enemy attempts one legal 8-way move.
    /// Publishes Move_Request immediately (inside the same Action pipeline) so results resolve this tick.
    /// </summary>
    public sealed class EnemyWanderBrain
    {
        private readonly EventBus _bus;
        private readonly TimedScheduler _sch;
        private readonly IEnemyRegistry _reg;
        private readonly IEntityLocations _locs;
        private readonly ICollisionPolicy _coll;
        private readonly int _seed;

        public EnemyWanderBrain(EventBus bus, TimedScheduler sch, IEnemyRegistry reg, IEntityLocations locs, ICollisionPolicy coll, int seed)
        {
            _bus = bus; _sch = sch; _reg = reg; _locs = locs; _coll = coll; _seed = seed;
            _bus.Subscribe<AI_TurnStart>(OnTurn);
        }

        private static readonly Direction8[] AllDirs = new[]{
            Direction8.Left, Direction8.Right, Direction8.Up, Direction8.Down,
            Direction8.UpLeft, Direction8.UpRight, Direction8.DownLeft, Direction8.DownRight
        };

        private void OnTurn(AI_TurnStart t)
        {
            // Iterate in ascending id order for determinism
            var ids = _reg.EnemyIds;
            for (int i=0;i<ids.Count;i++)
            {
                int id = ids[i];
                if (!_locs.TryGet(id, out var pos)) continue;

                // Build deterministic shuffled directions
                var dirs = new List<Direction8>(AllDirs);
                var rng = new System.Random(_seed ^ (id * 73856093) ^ t.TurnIndex);
                // Fisher-Yates
                for (int k = dirs.Count - 1; k > 0; k--)
                {
                    int j = rng.Next(k + 1);
                    var tmp = dirs[k]; dirs[k] = dirs[j]; dirs[j] = tmp;
                }

                // Pick first legal
                Direction8? chosen = null;
                for (int d=0; d<dirs.Count; d++)
                {
                    var to = Step(pos, dirs[d]);
                    if (_coll.CanEnter(to)) { chosen = dirs[d]; break; }
                }

                if (chosen.HasValue)
                {
                    // Publish Move_Request immediately (Action context continues)
                    _bus.Publish(new Move_Request { EntityId = id, Dir = chosen.Value });
                    // Optional: _bus.Publish(new AI_MoveIntent { EntityId = id });
                }
            }
        }

        private static Vector2Int Step(Vector2Int p, Direction8 d)
        {
            switch (d)
            {
                case Direction8.Left: return new Vector2Int(p.x-1, p.y);
                case Direction8.Right: return new Vector2Int(p.x+1, p.y);
                case Direction8.Up: return new Vector2Int(p.x, p.y+1);
                case Direction8.Down: return new Vector2Int(p.x, p.y-1);
                case Direction8.UpLeft: return new Vector2Int(p.x-1, p.y+1);
                case Direction8.UpRight: return new Vector2Int(p.x+1, p.y+1);
                case Direction8.DownLeft: return new Vector2Int(p.x-1, p.y-1);
                case Direction8.DownRight: return new Vector2Int(p.x+1, p.y-1);
                default: return p;
            }
        }
    }
}
