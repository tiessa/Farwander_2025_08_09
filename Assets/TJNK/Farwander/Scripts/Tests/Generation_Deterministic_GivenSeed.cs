using NUnit.Framework;
using TJNK.Farwander.Modules.Generation;
using UnityEngine;

namespace TJNK.Farwander.Tests.Generation
{
    public class Generation_Deterministic_GivenSeed
    {
        private static int HashTiles(DungeonMap map)
        {
            unchecked
            {
                int h = 17;
                for (int x=0;x<map.Width;x++)
                for (int y=0;y<map.Height;y++)
                    h = h * 31 + (int)map.Tiles[x,y];
                for (int i=0;i<map.Rooms.Count;i++)
                {
                    var r = map.Rooms[i];
                    h = h*31 + r.x + (r.y<<8) + (r.width<<16) + (r.height<<24);
                }
                return h;
            }
        }

        [Test]
        public void SameSeed_SameResultHash()
        {
            var gen = new RectDungeonGenerator();
            var size = new Vector2Int(48, 32);
            var mapA = gen.Generate(1337, size, new Vector2Int(4,3), new Vector2Int(10,7), 8);
            var mapB = gen.Generate(1337, size, new Vector2Int(4,3), new Vector2Int(10,7), 8);
            Assert.AreEqual(HashTiles(mapA), HashTiles(mapB));
        }
    }
}