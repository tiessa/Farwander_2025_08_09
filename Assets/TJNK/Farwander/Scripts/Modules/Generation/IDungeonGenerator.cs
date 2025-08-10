using UnityEngine;

namespace TJNK.Farwander.Modules.Generation
{
    public interface IDungeonGenerator
    {
        DungeonMap Generate(int seed, Vector2Int size, Vector2Int roomMin, Vector2Int roomMax, int roomCount);
    }
}