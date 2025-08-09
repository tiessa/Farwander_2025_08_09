using System;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Systems.Visibility
{
    /// <summary>Recursive shadowcasting FOV (RogueBasin-style), 8 octants.</summary>
    public static class FovCalculator
    {
        // Octant deltas (row,col) mapped to (dx,dy)
        private static readonly (int xx, int xy, int yx, int yy)[] Octants = new (int,int,int,int)[]
        {
            ( 1, 0, 0,  1), ( 0, 1, 1,  0), ( 0, 1,-1,  0), ( 1, 0, 0, -1),
            (-1, 0, 0, -1), ( 0,-1,-1, 0), ( 0,-1, 1, 0), (-1, 0, 0,  1)
        };

        public static void Compute(
            GridPosition origin,
            int radius,
            Func<GridPosition, bool> blocksSight,
            Action<GridPosition> setVisible)
        {
            // Origin always visible
            setVisible(origin);

            for (int oct = 0; oct < 8; oct++)
            {
                CastLight(origin, 1, 1.0, 0.0, radius, Octants[oct], blocksSight, setVisible);
            }
        }

        // row = distance from origin along the octant; col sweeps across row using slopes
        private static void CastLight(
            GridPosition origin,
            int row,
            double startSlope,
            double endSlope,
            int radius,
            (int xx, int xy, int yx, int yy) o,
            Func<GridPosition, bool> blocksSight,
            Action<GridPosition> setVisible)
        {
            if (startSlope < endSlope) return;
            if (row > radius) return;

            int rSq = radius * radius;
            int prevWasBlocked = 0;
            double nextStartSlope = startSlope;

            for (int col = row; col >= 0; col--)
            {
                int dx = col;
                int dy = row - col;

                int mx = origin.x + dx * o.xx + dy * o.xy;
                int my = origin.y + dx * o.yx + dy * o.yy;

                double lSlope = (col + 0.5) / (row - 0.5);
                double rSlope = (col - 0.5) / (row + 0.5);

                if (rSlope > startSlope) continue;
                if (lSlope < endSlope) break;

                int distSq = dx * dx + dy * dy;
                var pos = new GridPosition(mx, my);

                if (distSq <= rSq) setVisible(pos);

                bool blocked = blocksSight(pos);
                if (blocked)
                {
                    if (prevWasBlocked == 0)
                    {
                        // entering a blocked span
                        CastLight(origin, row + 1, nextStartSlope, lSlope, radius, o, blocksSight, setVisible);
                        prevWasBlocked = 1;
                        nextStartSlope = rSlope;
                    }
                }
                else
                {
                    if (prevWasBlocked == 1)
                    {
                        // exiting a blocked span
                        prevWasBlocked = 0;
                    }
                }
            }

            if (prevWasBlocked == 0)
            {
                CastLight(origin, row + 1, nextStartSlope, endSlope, radius, o, blocksSight, setVisible);
            }
        }
    }
}
