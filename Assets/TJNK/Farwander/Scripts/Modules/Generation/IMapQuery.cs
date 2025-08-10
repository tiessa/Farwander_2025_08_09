using UnityEngine;

namespace TJNK.Farwander.Modules.Generation
{
    /// <summary>Read-only map access for other systems.</summary>
    public interface IMapQuery
    {
        Vector2Int Size { get; }
        bool InBounds(Vector2Int p);
        MapTile GetTile(Vector2Int p);
        bool IsWalkable(Vector2Int p);
    }
}