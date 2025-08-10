using System.Collections.Generic;
using UnityEngine;

namespace TJNK.Farwander.Modules.Generation
{
    /// <summary>DTO produced by the generator and consumed by systems.</summary>
    public sealed class DungeonMap
    {
        public readonly MapTile[,] Tiles;
        public readonly List<RectInt> Rooms;
        public readonly List<Vector2Int> SpawnPoints;

        public int Width { get { return Tiles.GetLength(0); } }
        public int Height { get { return Tiles.GetLength(1); } }

        public DungeonMap(MapTile[,] tiles, List<RectInt> rooms, List<Vector2Int> spawns)
        {
            Tiles = tiles; Rooms = rooms; SpawnPoints = spawns;
        }
    }
}