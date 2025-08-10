using UnityEngine;

namespace TJNK.Farwander.Modules.Game
{
    public enum FovAlgorithm { RecursiveShadowcasting = 0 }

    [CreateAssetMenu(menuName = "Farwander/Config/Dungeon", fileName = "DungeonConfig")]
    public sealed class DungeonConfigSO : ScriptableObject
    {
        [Header("Grid")]
        public int Width = 48;
        public int Height = 32;

        [Header("Rooms")]
        public Vector2Int RoomMin = new Vector2Int(4, 3);
        public Vector2Int RoomMax = new Vector2Int(10, 7);
        public int RoomCount = 8;

        [Header("FOV")]
        public FovAlgorithm Fov = FovAlgorithm.RecursiveShadowcasting;
        public int FovRadius = 8;

        [Header("Seed")]
        public int Seed = 1337;
    }
}