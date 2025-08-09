using System;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Systems.Visibility
{
    public class VisibilityMap
    {
        public readonly int Width;
        public readonly int Height;

        private readonly bool[,] _visible;
        private readonly bool[,] _explored;

        public event Action OnVisibilityChanged;

        public VisibilityMap(int width, int height)
        {
            Width = width; Height = height;
            _visible  = new bool[width, height];
            _explored = new bool[width, height];
        }

        public bool IsVisible(int x, int y)  => InBounds(x,y) && _visible[x,y];
        public bool IsExplored(int x, int y) => InBounds(x,y) && _explored[x,y];

        public void ClearVisible()
        {
            for (int x=0; x<Width; x++)
            for (int y=0; y<Height; y++)
                _visible[x,y] = false;
        }

        public void Recompute(GridPosition origin, int radius, Func<GridPosition,bool> blocksSight)
        {
            ClearVisible();
            FovCalculator.Compute(origin, radius, p => !InBounds(p.x,p.y) || blocksSight(p), SetVisible);
            OnVisibilityChanged?.Invoke();
        }

        private void SetVisible(GridPosition p)
        {
            if (!InBounds(p.x,p.y)) return;
            _visible[p.x,p.y] = true;
            _explored[p.x,p.y] = true;
        }

        private bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
    }
}