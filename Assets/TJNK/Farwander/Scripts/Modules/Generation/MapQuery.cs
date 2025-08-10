using UnityEngine;

namespace TJNK.Farwander.Modules.Generation
{
    /// <summary>Default IMapQuery backed by a DungeonMap.</summary>
    public sealed class MapQuery : IMapQuery
    {
        private readonly DungeonMap _map;
        private readonly Vector2Int _size;
        public MapQuery(DungeonMap map)
        {
            _map = map; _size = new Vector2Int(map.Width, map.Height);
        }
        public Vector2Int Size { get { return _size; } }
        public bool InBounds(Vector2Int p) { return p.x >= 0 && p.y >= 0 && p.x < _size.x && p.y < _size.y; }
        public MapTile GetTile(Vector2Int p) { return _map.Tiles[p.x, p.y]; }
        public bool IsWalkable(Vector2Int p) { return InBounds(p) && _map.Tiles[p.x, p.y] == MapTile.Floor; }
    }
}