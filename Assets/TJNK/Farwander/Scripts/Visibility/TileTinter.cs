using UnityEngine;
using UnityEngine.Tilemaps;

namespace TJNK.Farwander.Systems.Visibility
{
    /// <summary>Applies per-tile tint based on VisibilityMap.</summary>
    [RequireComponent(typeof(Tilemap))]
    public class TileTinter : MonoBehaviour
    {
        [Tooltip("Assigned at runtime by GameBootstrap.")]
        public VisibilityMap visibility;

        [Tooltip("Unseen tiles color (usually black).")]
        public Color unseen = Color.black;

        [Tooltip("Explored but not currently visible.")]
        public Color exploredDim = new Color(0.25f, 0.25f, 0.25f, 1f);

        [Tooltip("Currently visible (white = no tint).")]
        public Color visible = Color.white;

        private Tilemap _tm;
        private int _w, _h;

        public void Init(VisibilityMap vis, int width, int height)
        {
            _tm = GetComponent<Tilemap>();
            visibility = vis;
            _w = width; _h = height;

            if (visibility != null)
                visibility.OnVisibilityChanged += ApplyAll;

            ApplyAll();
        }

        private void OnDestroy()
        {
            if (visibility != null)
                visibility.OnVisibilityChanged -= ApplyAll;
        }

        private void ApplyAll()
        {
            if (_tm == null || visibility == null) return;

            // Iterate bounds once; small maps make this trivial
            for (int x = 0; x < _w; x++)
            for (int y = 0; y < _h; y++)
            {
                var pos = new Vector3Int(x, y, 0);
                if (!_tm.HasTile(pos))
                    continue;

                Color c = unseen;
                if (visibility.IsExplored(x,y))
                    c = visibility.IsVisible(x,y) ? visible : exploredDim;

                _tm.SetTileFlags(pos, TileFlags.None); // allow color override
                _tm.SetColor(pos, c);
            }
        }
    }
}