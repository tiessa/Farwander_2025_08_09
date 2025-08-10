using System.Collections.Generic;
using UnityEngine;

namespace TJNK.Farwander.Modules.Game.Runtime.Entities
{
    public sealed class EntityLocations : IEntityLocations
    {
        private readonly Dictionary<int, Vector2Int> _pos = new Dictionary<int, Vector2Int>();
        private readonly Dictionary<int, int> _occ = new Dictionary<int, int>(); // key = hash(x,y), val = count

        private static int Hash(Vector2Int p) => (p.x & 0xFFFF) | (p.y << 16);

        public bool TryGet(int id, out Vector2Int pos) => _pos.TryGetValue(id, out pos);

        public void Set(int id, Vector2Int pos)
        {
            if (_pos.TryGetValue(id, out var old))
            {
                var hOld = Hash(old); if (_occ.ContainsKey(hOld)) { _occ[hOld]--; if (_occ[hOld] <= 0) _occ.Remove(hOld); }
            }
            _pos[id] = pos;
            var hNew = Hash(pos); _occ[hNew] = _occ.TryGetValue(hNew, out var c) ? c + 1 : 1;
        }

        public void Remove(int id)
        {
            if (_pos.TryGetValue(id, out var old))
            {
                var hOld = Hash(old); if (_occ.ContainsKey(hOld)) { _occ[hOld]--; if (_occ[hOld] <= 0) _occ.Remove(hOld); }
                _pos.Remove(id);
            }
        }

        public bool IsOccupied(Vector2Int pos)
        {
            return _occ.ContainsKey(Hash(pos));
        }
    }
}