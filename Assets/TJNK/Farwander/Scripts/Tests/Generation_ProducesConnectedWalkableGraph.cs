using NUnit.Framework;
using TJNK.Farwander.Modules.Generation;
using UnityEngine;
using System.Collections.Generic;

namespace TJNK.Farwander.Tests.Generation
{
    public class Generation_ProducesConnectedWalkableGraph
    {
        [Test]
        public void AllFloors_Connected()
        {
            var gen = new RectDungeonGenerator();
            var map = gen.Generate(1337, new Vector2Int(48,32), new Vector2Int(4,3), new Vector2Int(10,7), 8);
            int floorCount = 0; Vector2Int start = new Vector2Int(-1,-1);
            for (int x=0;x<map.Width;x++) for (int y=0;y<map.Height;y++)
                if (map.Tiles[x,y] == MapTile.Floor) { floorCount++; if (start.x<0) start = new Vector2Int(x,y); }
            Assert.Greater(floorCount, 0);

            var seen = new bool[map.Width, map.Height];
            var q = new Queue<Vector2Int>(); q.Enqueue(start); seen[start.x,start.y] = true; int visited=1;
            var dirs = new [] { new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1) };
            while (q.Count>0)
            {
                var p = q.Dequeue();
                for (int i=0;i<4;i++)
                {
                    var n = p + dirs[i];
                    if (n.x<0||n.y<0||n.x>=map.Width||n.y>=map.Height) continue;
                    if (seen[n.x,n.y]) continue;
                    if (map.Tiles[n.x,n.y] != MapTile.Floor) continue;
                    seen[n.x,n.y] = true; visited++; q.Enqueue(n);
                }
            }
            Assert.AreEqual(floorCount, visited);
        }
    }
}