using UnityEngine.Tilemaps;
using TJNK.Farwander.Content;

namespace TJNK.Farwander.Generation
{
    public class MapRuntime
    {
        public MapGenerator Generator { get; private set; }
        public MapRuntime(Tilemap tm, Tileset tiles, int w, int h, int seed = 0)
        {
            Generator = new MapGenerator(tm, tiles, w, h, seed);
            Generator.Generate();
        }
    }
}