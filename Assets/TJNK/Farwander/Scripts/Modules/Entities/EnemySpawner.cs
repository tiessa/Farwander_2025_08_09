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
    /// Deterministic given seed + order.
    /// </summary>
    public sealed class EnemySpawner
    {
        private readonly EventBus _bus; private readonly TimedScheduler _sch;
        private readonly IEntityLocations _locs; private readonly IEnemyRegistry _reg;

        public EnemySpawner(EventBus bus, TimedScheduler sch, IEntityLocations locs, IEnemyRegistry reg)
        { _bus = bus; _sch = sch; _locs = locs; _reg = reg; }

        public void SpawnAll(DungeonMap map, int count, System.Func<int, Color32> tintByIndex)
        {
            _reg.Clear();
            var used = new HashSet<int>(); // hashes of cells
            System.Func<Vector2Int,int> H = p => (p.x & 0xFFFF) | (p.y << 16);

            // Seed spawn locations around room centers (skip index0 reserved for player)
            var centers = map.Rooms;
            int nextEntityId = 2; // 1 is the player by convention
            int placed = 0;

            // Helper picks a free floor tile near preference or via scan
            Vector2Int PickFree(Vector2Int preferred)
            {
                if (IsFree(preferred)) return preferred;
                // small spiral around preferred
                var dirs = new[]{ new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(-1,0), new Vector2Int(0,-1) };
                for (int r=1;r<6;r++)
                {
                    var p = preferred + new Vector2Int(-r, -r);
                    int side = r*2;
                    for (int d=0; d<4; d++)
                    {
                        for (int s=0; s<side; s++)
                        {
                            if (IsFree(p)) return p;
                            p += dirs[d];
                        }
                    }
                }
                // fallback linear scan
                for (int y=0;y<map.Height;y++) for (int x=0;x<map.Width;x++)
                {
                    var p = new Vector2Int(x,y); if (IsFree(p)) return p;
                }
                return preferred; // last resort
            }

            bool IsFree(Vector2Int p)
            {
                if (p.x<0||p.y<0||p.x>=map.Width||p.y>=map.Height) return false;
                if (map.Tiles[p.x,p.y] != MapTile.Floor) return false;
                int h = H(p); if (used.Contains(h)) return false;
                if (_locs.IsOccupied(p)) return false;
                return true;
            }

            int centerIndex = 1; // skip room 0 center (player)
            while (placed < count)
            {
                Vector2Int pref;
                if (centerIndex < centers.Count)
                {
                    var r = centers[centerIndex++];
                    pref = new Vector2Int(r.x + r.width/2, r.y + r.height/2);
                }
                else
                {
                    pref = new Vector2Int(map.Width/2, map.Height/2);
                }
                var pos = PickFree(pref);
                int id = nextEntityId++;
                _locs.Set(id, pos); _reg.Register(id); used.Add(H(pos)); placed++;
                var color = tintByIndex != null ? tintByIndex(placed-1) : new Color32(200,60,60,255);

                // Tell views (Dispatch @ Now)
                int eid = id; var epos = pos;
                _sch.Schedule(_sch.Now, EventPriority.World, EventLane.Dispatch, this,
                    () => _bus.Publish(new Entity_Spawned { EntityId = eid, Pos = epos, IsPlayer = false, SpriteName = "enemy" }));
            }
        }
    }
}
