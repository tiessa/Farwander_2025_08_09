using System.Collections.Generic;
using UnityEngine;
using TJNK.Farwander.Modules.Generation;
using TJNK.Farwander.Modules.Game.Runtime.Entities;
using TJNK.Farwander.Modules.Game;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Modules.AI
{
    /// <summary>
    /// Places enemies on walkable, unoccupied tiles and publishes Entity_Spawned for each.
    /// Deterministic given map + order. Aligned to TJNK.Farwander.Modules.Game.EnemyRosterConfigSO.
    /// </summary>
    public sealed class EnemySpawner
    {
        private readonly EventBus _bus; private readonly TimedScheduler _sch;
        private readonly IEntityLocations _locs; private readonly IEnemyRegistry _reg;

        public EnemySpawner(EventBus bus, TimedScheduler sch, IEntityLocations locs, IEnemyRegistry reg)
        { _bus = bus; _sch = sch; _locs = locs; _reg = reg; } 
        
        /// <summary>
        /// Spawn enemies based on roster.Types[].Count. If roster or Types is null/empty, spawns nothing.
        /// </summary>
        public void SpawnFromRoster(DungeonMap map, EnemyRosterConfigSO roster)
        {
            _reg.Clear(); if (roster == null || roster.Types == null || roster.Types.Length == 0) return;

            var used = new HashSet<int>(); // hashes of cells to avoid duplicates during this call
            System.Func<Vector2Int,int> H = p => (p.x & 0xFFFF) | (p.y << 16);

            int nextEntityId = 2; // 1 is the player by convention

            // For each enemy type, place Count instances
            for (int t = 0; t < roster.Types.Length; t++)
            {
                var type = roster.Types[t]; if (type == null || type.Count <= 0) continue;

                for (int i = 0; i < type.Count; i++)
                {
                    var pos = PickFreeNearRooms(map, used);
                    int id = nextEntityId++;
                    _locs.Set(id, pos); _reg.Register(id); used.Add(H(pos));

                    // Tell views (Dispatch @ Now)
                    int eid = id; var epos = pos; string name = string.IsNullOrEmpty(type.EnemyName) ? "enemy" : type.EnemyName;
                    _sch.Schedule(_sch.Now, EventPriority.World, EventLane.Dispatch, this,
                        () => _bus.Publish(new Entity_Spawned { EntityId = eid, Pos = epos, IsPlayer = false, SpriteName = name }));
                }
            }
        }

        /// <summary>
        /// Picks a free floor tile near room centers (skips room[0] which we reserve for player),
        /// with a small spiral search and linear fallback.
        /// </summary>
        private Vector2Int PickFreeNearRooms(DungeonMap map, HashSet<int> used)
        {
            System.Func<Vector2Int,int> H = p => (p.x & 0xFFFF) | (p.y << 16);
            bool IsFree(Vector2Int p)
            {
                if (p.x<0||p.y<0||p.x>=map.Width||p.y>=map.Height) return false;
                if (map.Tiles[p.x,p.y] != MapTile.Floor) return false;
                int h = H(p); if (used.Contains(h)) return false;
                if (_locs.IsOccupied(p)) return false; // avoid existing entities
                return true;
            }

            // Prefer room centers excluding index 0 (player)
            int start = map.Rooms.Count > 1 ? 1 : 0;
            for (int k = start; k < map.Rooms.Count; k++)
            {
                var r = map.Rooms[k];
                var pref = new Vector2Int(r.x + r.width/2, r.y + r.height/2);
                var got = SpiralPick(pref, 6, IsFree);
                if (got.HasValue) return got.Value;
            }

            // Fallback linear scan
            for (int y=0;y<map.Height;y++) for (int x=0;x<map.Width;x++)
            {
                var p = new Vector2Int(x,y); if (IsFree(p)) return p;
            }
            return new Vector2Int(map.Width/2, map.Height/2);
        }

        private static Vector2Int? SpiralPick(Vector2Int preferred, int maxRadius, System.Func<Vector2Int,bool> ok)
        {
            if (ok(preferred)) return preferred;
            var dirs = new[]{ new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(-1,0), new Vector2Int(0,-1) };
            for (int r=1;r<=maxRadius;r++)
            {
                var p = preferred + new Vector2Int(-r, -r);
                int side = r*2;
                for (int d=0; d<4; d++)
                {
                    for (int s=0; s<side; s++)
                    {
                        if (ok(p)) return p;
                        p += dirs[d];
                    }
                }
            }
            return null;
        }
    }
}
