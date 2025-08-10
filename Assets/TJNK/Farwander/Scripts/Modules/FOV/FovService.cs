using UnityEngine;
using TJNK.Farwander.Core;
using TJNK.Farwander.Modules.Game;
using TJNK.Farwander.Modules.Game.Runtime.Entities;
using TJNK.Farwander.Modules.Generation;

namespace TJNK.Farwander.Modules.Fov
{
    /// <summary>
    /// Fog-Of-War service using simple LOS (Bresenham) per cell within radius.
    /// Listens for Fov_Recompute (Action), computes visibility using IMapQuery + IEntityLocations,
    /// updates a persistent Explored mask, then publishes Fov_Updated (Dispatch @ Now).
    /// Exposed to views via IFovQuery (registered by the controller).
    /// </summary>
    public sealed class FovService : IFovQuery
    {
        private readonly EventBus _bus;
        private readonly TimedScheduler _sch;
        private readonly QueryRegistry _q;

        private bool[,] _visible;
        private bool[,] _explored;
        private int _w, _h;

        public int Width => _w;
        public int Height => _h;

        public FovService(EventBus bus, TimedScheduler sch, QueryRegistry q)
        {
            _bus = bus; _sch = sch; _q = q;
            _bus.Subscribe<Fov_Recompute>(OnRecompute);
        }

        public bool InBounds(int x, int y) => (x >= 0 && y >= 0 && x < _w && y < _h);
        public bool IsVisible(int x, int y) => InBounds(x, y) && _visible[x, y];
        public bool IsExplored(int x, int y) => InBounds(x, y) && _explored[x, y];

        private void EnsureSize(int w, int h)
        {
            if (w == _w && h == _h && _visible != null && _explored != null) return;
            _w = Mathf.Max(1, w); _h = Mathf.Max(1, h);
            _visible = new bool[_w, _h];
            _explored = new bool[_w, _h];
        }

        private void ClearVisible()
        {
            for (int x = 0; x < _w; x++)
                for (int y = 0; y < _h; y++)
                    _visible[x, y] = false;
        }

        private void OnRecompute(Fov_Recompute req)
        {
            var mq = _q.Get<IMapQuery>();
            if (!TryResolveMapSize(mq, out var mw, out var mh))
            {
                Debug.LogWarning("[FOV] Could not resolve map size from IMapQuery; skipping recompute.");
                return;
            }
            EnsureSize(mw, mh);

            var locs = _q.Get<IEntityLocations>();
            if (!locs.TryGet(req.ViewerId, out var origin)) return;

            int radius = Mathf.Max(0, req.Radius);

            ClearVisible();
            SetVisible(origin.x, origin.y);
            ComputeFovLOS(mq, origin, radius);

            // Persist exploration
            for (int x = 0; x < _w; x++)
                for (int y = 0; y < _h; y++)
                    if (_visible[x, y]) _explored[x, y] = true;

            // Notify this tick (Dispatch @ Now)
            _sch.Schedule(_sch.Now, EventPriority.World, EventLane.Dispatch, this,
                () => _bus.Publish(new Fov_Updated { }));
        }

        private void SetVisible(int x, int y)
        {
            if (InBounds(x, y)) _visible[x, y] = true;
        }

        private void ComputeFovLOS(IMapQuery mq, Vector2Int origin, int radius)
        {
            int xmin = Mathf.Max(0, origin.x - radius);
            int xmax = Mathf.Min(_w - 1, origin.x + radius);
            int ymin = Mathf.Max(0, origin.y - radius);
            int ymax = Mathf.Min(_h - 1, origin.y + radius);
            int r2 = radius * radius;

            for (int y = ymin; y <= ymax; y++)
            for (int x = xmin; x <= xmax; x++)
            {
                int dx = x - origin.x; int dy = y - origin.y;
                if (dx*dx + dy*dy > r2) continue;
                if (LineOfSight(mq, origin.x, origin.y, x, y))
                    SetVisible(x, y);
            }
        }

        // Bresenham LOS from (x0,y0) to (x1,y1). Blocks along the path (excluding the target cell) stop sight.
        // If the target cell itself is blocking, it remains visible but nothing beyond it is (handled by the scan).
        private static bool LineOfSight(IMapQuery mq, int x0, int y0, int x1, int y1)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            int x = x0, y = y0;

            while (x != x1 || y != y1)
            {
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 <  dx) { err += dx; y += sy; }

                // If we've reached the target, allow it (even if it's a wall)
                if (x == x1 && y == y1) return true;

                // Any blocker before the target stops sight
                if (!mq.IsWalkable(new Vector2Int(x, y))) return false;
            }
            return true;
        }

        /// <summary>
        /// Resolve map size in a decoupled way (Width/Height, Size, Bounds or Map.Width/Map.Height).
        /// </summary>
        private static bool TryResolveMapSize(object mq, out int w, out int h)
        {
            w = h = 0; if (mq == null) return false;
            var t = mq.GetType();

            var pW = t.GetProperty("Width"); var pH = t.GetProperty("Height");
            if (pW != null && pH != null)
            { w = System.Convert.ToInt32(pW.GetValue(mq)); h = System.Convert.ToInt32(pH.GetValue(mq)); if (w>0&&h>0) return true; }

            var pSize = t.GetProperty("Size");
            if (pSize != null)
            {
                var size = pSize.GetValue(mq);
                if (size != null)
                {
                    var st = size.GetType(); var px = st.GetProperty("x"); var py = st.GetProperty("y");
                    if (px != null && py != null)
                    { w = System.Convert.ToInt32(px.GetValue(size)); h = System.Convert.ToInt32(py.GetValue(size)); if (w>0&&h>0) return true; }
                }
            }

            var pBounds = t.GetProperty("Bounds");
            if (pBounds != null)
            {
                var b = pBounds.GetValue(mq);
                if (b != null)
                {
                    var bt = b.GetType(); var pw = bt.GetProperty("width"); var ph = bt.GetProperty("height");
                    if (pw != null && ph != null)
                    { w = System.Convert.ToInt32(pw.GetValue(b)); h = System.Convert.ToInt32(ph.GetValue(b)); if (w>0&&h>0) return true; }
                }
            }

            object mapObj = null;
            var pMap = t.GetProperty("Map"); if (pMap != null) mapObj = pMap.GetValue(mq);
            if (mapObj == null)
            { var fMap = t.GetField("Map", System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic); if (fMap != null) mapObj = fMap.GetValue(mq); }
            if (mapObj != null)
            {
                var mt = mapObj.GetType(); var pw = mt.GetProperty("Width"); var ph = mt.GetProperty("Height");
                if (pw != null && ph != null)
                { w = System.Convert.ToInt32(pw.GetValue(mapObj)); h = System.Convert.ToInt32(ph.GetValue(mapObj)); if (w>0&&h>0) return true; }
            }

            return false;
        }
    }
}
