using System;
using System.Collections.Generic;
using UnityEngine;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Modules.Generation
{
    /// <summary>
    /// Deterministic rectangular dungeon generator:
    /// - Places non-overlapping rooms within given size/count limits
    /// - Connects them with deterministic L-corridors (Manhattan)
    /// </summary>
    public sealed class RectDungeonGenerator : IDungeonGenerator
    {
        private readonly ValidationPipeline _validation; // optional audit
        public RectDungeonGenerator(ValidationPipeline validation = null) { _validation = validation; }

        public DungeonMap Generate(int seed, Vector2Int size, Vector2Int roomMin, Vector2Int roomMax, int roomCount)
        {
            if (roomMin.x > roomMax.x) { var t = roomMin.x; roomMin.x = roomMax.x; roomMax.x = t; }
            if (roomMin.y > roomMax.y) { var t = roomMin.y; roomMin.y = roomMax.y; roomMax.y = t; }
            var rnd = new System.Random(seed);
            var tiles = new MapTile[size.x, size.y];
            for (int x=0;x<size.x;x++) for (int y=0;y<size.y;y++) tiles[x,y] = MapTile.Wall;

            var rooms = new List<RectInt>();
            var attempts = 0; var maxAttempts = roomCount * 20;
            while (rooms.Count < roomCount && attempts++ < maxAttempts)
            {
                var w = rnd.Next(roomMin.x, roomMax.x + 1);
                var h = rnd.Next(roomMin.y, roomMax.y + 1);
                var x = rnd.Next(1, Math.Max(1, size.x - w - 1));
                var y = rnd.Next(1, Math.Max(1, size.y - h - 1));
                var candidate = new RectInt(x, y, w, h);
                bool overlap = false;
                for (int i=0;i<rooms.Count;i++) { if (rooms[i].Overlaps(candidate)) { overlap = true; break; } }
                if (overlap) continue;
                rooms.Add(candidate);
            }

            // Carve rooms
            for (int i=0;i<rooms.Count;i++)
            {
                var r = rooms[i];
                for (int x=r.xMin; x<r.xMax; x++)
                    for (int y=r.yMin; y<r.yMax; y++) tiles[x,y] = MapTile.Floor;
            }

            // Connect rooms with L-corridors: sort by center.x for deterministic ordering
            rooms.Sort((ra, rb) => (ra.x + ra.width / 2).CompareTo(rb.x + rb.width / 2));
            for (int i=1;i<rooms.Count;i++)
            {
                var a = new Vector2Int(rooms[i - 1].x + rooms[i - 1].width / 2, rooms[i - 1].y + rooms[i - 1].height / 2);
                var b = new Vector2Int(rooms[i].x + rooms[i].width / 2, rooms[i].y + rooms[i].height / 2);
                var turn = new Vector2Int(b.x, a.y);
                CarveCorridor(tiles, a, turn);
                CarveCorridor(tiles, turn, b);
            }

            // Spawn points: room centers (rounded)
            var spawns = new List<Vector2Int>(rooms.Count);
            for (int i = 0; i < rooms.Count; i++)
                spawns.Add(new Vector2Int(rooms[i].x + rooms[i].width / 2, rooms[i].y + rooms[i].height / 2));

            var map = new DungeonMap(tiles, rooms, spawns);

            // Optional audit
            if (_validation != null)
            {
                _validation.Audit(map);
            }

            return map;
        }

        private static void CarveCorridor(MapTile[,] tiles, Vector2Int from, Vector2Int to)
        {
            int x = from.x, y = from.y; int tx = to.x, ty = to.y;
            int dx = Math.Sign(tx - x); int dy = Math.Sign(ty - y);
            // Horizontal leg
            while (x != tx) { tiles[x,y] = MapTile.Floor; x += dx; }
            // Vertical leg
            while (y != ty) { tiles[x,y] = MapTile.Floor; y += dy; }
            tiles[tx,ty] = MapTile.Floor;
        }
    }
}
