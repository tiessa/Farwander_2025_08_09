using NUnit.Framework;
using TJNK.Farwander.Modules.Generation;
using UnityEngine;

namespace TJNK.Farwander.Tests.Generation
{
    public class Generation_TilesMatchConfigSize
    {
        [Test]
        public void TilesArray_Matches_ConfigSize()
        {
            var gen = new RectDungeonGenerator();
            var size = new Vector2Int(48, 32);
            var map = gen.Generate(1337, size, new Vector2Int(4,3), new Vector2Int(10,7), 8);
            Assert.AreEqual(size.x, map.Width);
            Assert.AreEqual(size.y, map.Height);
            Assert.AreEqual(size.x, map.Tiles.GetLength(0));
            Assert.AreEqual(size.y, map.Tiles.GetLength(1));
        }
    }
}