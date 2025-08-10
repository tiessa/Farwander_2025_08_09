using NUnit.Framework;
using TJNK.Farwander.Modules.Generation;
using UnityEngine;

namespace TJNK.Farwander.Tests.Generation
{
    public class Generation_NoRoomOverlap
    {
        [Test]
        public void Rooms_DoNot_Overlap()
        {
            var gen = new RectDungeonGenerator();
            var map = gen.Generate(1337, new Vector2Int(48,32), new Vector2Int(4,3), new Vector2Int(10,7), 8);
            for (int i=0;i<map.Rooms.Count;i++)
            for (int j=i+1;j<map.Rooms.Count;j++)
                Assert.IsFalse(map.Rooms[i].Overlaps(map.Rooms[j]), $"Overlap between {i} and {j}");
        }
    }
}