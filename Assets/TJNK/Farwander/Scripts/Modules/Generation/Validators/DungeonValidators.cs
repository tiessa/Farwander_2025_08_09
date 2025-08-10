using System.Collections.Generic;
using UnityEngine;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Modules.Generation.Validators
{
    /// <summary>Validates that rooms sit fully within bounds.</summary>
    public sealed class RoomsWithinBoundsValidator : IValidator<DungeonMap>
    {
        public bool Validate(DungeonMap map, out string reason)
        {
            var size = new Vector2Int(map.Width, map.Height);
            for (int i=0;i<map.Rooms.Count;i++)
            {
                var r = map.Rooms[i];
                if (r.xMin < 0 || r.yMin < 0 || r.xMax > size.x || r.yMax > size.y)
                { reason = "Room out of bounds"; return false; }
            }
            reason = null; return true;
        }
    }

    /// <summary>Validates that no rooms overlap.</summary>
    public sealed class NoRoomOverlapValidator : IValidator<DungeonMap>
    {
        public bool Validate(DungeonMap map, out string reason)
        {
            for (int i=0;i<map.Rooms.Count;i++)
            {
                for (int j=i+1;j<map.Rooms.Count;j++)
                {
                    if (map.Rooms[i].Overlaps(map.Rooms[j])) { reason = "Rooms overlap"; return false; }
                }
            }
            reason = null; return true;
        }
    }

    /// <summary>Validates that all floors are mutually reachable.</summary>
    public sealed class ConnectivityValidator : IValidator<DungeonMap>
    {
        private static readonly Vector2Int[] Dir4 = new [] { new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1) };

        public bool Validate(DungeonMap map, out string reason)
        {
            // Find a starting floor cell
            Vector2Int start = new Vector2Int(-1, -1);
            int floorCount = 0;
            for (int x=0;x<map.Width;x++) for (int y=0;y<map.Height;y++)
            { if (map.Tiles[x,y] == MapTile.Floor) { floorCount++; if (start.x<0) start = new Vector2Int(x,y); } }
            if (floorCount == 0) { reason = "No floor tiles"; return false; }

            var visited = new bool[map.Width, map.Height];
            var q = new Queue<Vector2Int>(); q.Enqueue(start); visited[start.x,start.y] = true; int seen = 1;
            while (q.Count>0)
            {
                var p = q.Dequeue();
                for (int i=0;i<4;i++)
                {
                    var n = p + Dir4[i];
                    if (n.x<0||n.y<0||n.x>=map.Width||n.y>=map.Height) continue;
                    if (visited[n.x,n.y]) continue;
                    if (map.Tiles[n.x,n.y] != MapTile.Floor) continue;
                    visited[n.x,n.y] = true; seen++; q.Enqueue(n);
                }
            }
            if (seen != floorCount) { reason = "Map is not fully connected"; return false; }
            reason = null; return true;
        }
    }
}
