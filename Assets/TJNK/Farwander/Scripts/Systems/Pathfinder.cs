using System.Collections.Generic;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Systems
{
    public static class Pathfinder
    {
        private static readonly GridPosition[] Neigh = new[]
        {
            new GridPosition(1,0), new GridPosition(-1,0),
            new GridPosition(0,1), new GridPosition(0,-1)
        };

        public static List<GridPosition> FindPath(
            GridPosition start, GridPosition goal,
            System.Func<GridPosition, bool> passable,
            int maxNodes = 8000)
        {
            var open = new PriorityQueue<GridPosition>();
            var cameFrom = new Dictionary<GridPosition, GridPosition>();
            var gScore = new Dictionary<GridPosition, int> { [start] = 0 };
            var fScore = new Dictionary<GridPosition, int> { [start] = Heuristic(start, goal) };

            open.Enqueue(start, fScore[start]);
            int popped = 0;

            while (open.Count > 0 && popped < maxNodes)
            {
                var current = open.Dequeue();
                popped++;

                if (current == goal)
                    return Reconstruct(cameFrom, current);

                foreach (var d in Neigh)
                {
                    var next = current + d;
                    if (!passable(next)) continue;
                    int tentative = gScore[current] + 1;

                    if (!gScore.TryGetValue(next, out int old) || tentative < old)
                    {
                        cameFrom[next] = current;
                        gScore[next] = tentative;
                        int f = tentative + Heuristic(next, goal);
                        fScore[next] = f;
                        open.EnqueueOrDecreaseKey(next, f);
                    }
                }
            }
            return new List<GridPosition>();
        }

        private static int Heuristic(GridPosition a, GridPosition b)
            => System.Math.Abs(a.x - b.x) + System.Math.Abs(a.y - b.y);

        private static List<GridPosition> Reconstruct(
            Dictionary<GridPosition, GridPosition> cameFrom, GridPosition current)
        {
            var path = new List<GridPosition> { current };
            while (cameFrom.TryGetValue(current, out var prev))
            {
                current = prev;
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        private class PriorityQueue<T>
        {
            private readonly List<(T item, int prio)> heap = new();
            private readonly Dictionary<T, int> index = new();

            public int Count => heap.Count;

            public void Enqueue(T item, int priority)
            {
                heap.Add((item, priority));
                index[item] = heap.Count - 1;
                SiftUp(heap.Count - 1);
            }

            public void EnqueueOrDecreaseKey(T item, int priority)
            {
                if (index.TryGetValue(item, out int i))
                {
                    if (priority < heap[i].prio)
                    {
                        heap[i] = (item, priority);
                        SiftUp(i);
                    }
                }
                else Enqueue(item, priority);
            }

            public T Dequeue()
            {
                var root = heap[0].item;
                var last = heap[^1];
                heap[0] = last;
                index[last.item] = 0;
                heap.RemoveAt(heap.Count - 1);
                index.Remove(root);
                if (heap.Count > 0) SiftDown(0);
                return root;
            }

            private void SiftUp(int i)
            {
                while (i > 0)
                {
                    int p = (i - 1) / 2;
                    if (heap[i].prio >= heap[p].prio) break;
                    Swap(i, p);
                    i = p;
                }
            }

            private void SiftDown(int i)
            {
                while (true)
                {
                    int l = i * 2 + 1, r = i * 2 + 2, smallest = i;
                    if (l < heap.Count && heap[l].prio < heap[smallest].prio) smallest = l;
                    if (r < heap.Count && heap[r].prio < heap[smallest].prio) smallest = r;
                    if (smallest == i) break;
                    Swap(i, smallest);
                    i = smallest;
                }
            }

            private void Swap(int a, int b)
            {
                (heap[a], heap[b]) = (heap[b], heap[a]);
                index[heap[a].item] = a;
                index[heap[b].item] = b;
            }
        }
    }
}
