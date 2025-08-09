using UnityEngine.Tilemaps;
using TJNK.Farwander.Content;
using TJNK.Farwander.Systems.Visibility;

namespace TJNK.Farwander.Generation
{
    public class MapRuntime
    {
        public MapGenerator Generator { get; private set; }
        public VisibilityMap Visibility { get; private set; }

        public MapRuntime(Tilemap tm, Tileset tiles, int w, int h, int seed = 0)
        {
            Generator = new MapGenerator(tm, tiles, w, h, seed);
            Generator.Generate();
            Visibility = new VisibilityMap(Generator.Width, Generator.Height);
        }
    }
}