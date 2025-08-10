using UnityEngine;
using TJNK.Farwander.Modules.Generation;
using TJNK.Farwander.Modules.Game.Runtime.Entities;

namespace TJNK.Farwander.Modules.Game.Runtime.Movement
{
    /// <summary>Walkability: in bounds, floor tile, not occupied.</summary>
    public sealed class CollisionPolicy : ICollisionPolicy
    {
        private readonly System.Func<IMapQuery> _mapProvider;
        private readonly IEntityLocations _locs;
        public CollisionPolicy(System.Func<IMapQuery> mapProvider, IEntityLocations locs)
        { _mapProvider = mapProvider; _locs = locs; }

        public bool CanEnter(Vector2Int cell)
        {
            var map = _mapProvider();
            if (map == null) return false;
            if (!map.InBounds(cell)) return false;
            if (!map.IsWalkable(cell)) return false;
            if (_locs.IsOccupied(cell)) return false;
            return true;
        }
    }
}