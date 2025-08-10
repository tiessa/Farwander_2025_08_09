using UnityEngine;

namespace TJNK.Farwander.Modules.Game.Runtime.Entities
{
    public interface IEntityLocations
    {
        bool TryGet(int id, out Vector2Int pos);
        void Set(int id, Vector2Int pos);
        void Remove(int id);
        bool IsOccupied(Vector2Int pos);
    }
}