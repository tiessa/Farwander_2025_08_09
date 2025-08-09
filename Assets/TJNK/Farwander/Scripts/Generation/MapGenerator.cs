using UnityEngine;
using UnityEngine.Tilemaps;
using TJNK.Farwander.Content;
using TJNK.Farwander.Core;
using System;

namespace TJNK.Farwander.Generation
{
    public class MapGenerator
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private bool[,] walkable;
        private Tileset tileset;
        private Tilemap tilemap;
        private System.Random rng;

        public MapGenerator(Tilemap tilemap, Tileset tileset, int width, int height, int seed = 0)
        {
            this.tilemap = tilemap;
            this.tileset = tileset;
            Width = Mathf.Max(20, width);
            Height = Mathf.Max(20, height);
            rng = seed == 0 ? new System.Random() : new System.Random(seed);
            walkable = new bool[Width, Height];
        }

        public bool[,] Generate()
        {
            // Start: fill walls
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    walkable[x, y] = false;

            // Drunkard-walk carve
            int targetFloor = (int)(Width * Height * 0.42f);
            int carved = 0;
            int cx = Width / 2, cy = Height / 2;
            while (carved < targetFloor)
            {
                if (!walkable[cx, cy]) { walkable[cx, cy] = true; carved++; }
                int dir = rng.Next(4);
                if (dir == 0 && cx > 1) cx--;
                else if (dir == 1 && cx < Width - 2) cx++;
                else if (dir == 2 && cy > 1) cy--;
                else if (dir == 3 && cy < Height - 2) cy++;
            }

            // Draw to tilemap
            tilemap.ClearAllTiles();
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                var def = walkable[x, y] ? tileset.floor : tileset.wall;
                tilemap.SetTile(new Vector3Int(x, y, 0), def.tile);
            }

            return walkable;
        }

        public bool IsWalkable(GridPosition p)
        {
            if (p.x < 0 || p.y < 0 || p.x >= Width || p.y >= Height) return false;
            return walkable[p.x, p.y];
        }

        public GridPosition GetRandomFloor()
        {
            for (int tries = 0; tries < 5000; tries++)
            {
                int x = rng.Next(Width);
                int y = rng.Next(Height);
                if (walkable[x, y]) return new GridPosition(x, y);
            }
            return new GridPosition(Width / 2, Height / 2);
        }
    }
}
