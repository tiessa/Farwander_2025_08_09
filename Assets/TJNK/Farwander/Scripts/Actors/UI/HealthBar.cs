using UnityEngine;

namespace TJNK.Farwander.Actors.UI
{
    /// <summary>Simple world-space 2-sprite health bar that follows the Actor.</summary>
    [ExecuteAlways]
    public class HealthBar : MonoBehaviour
    {
        [Tooltip("Bar width in tiles (world units).")]
        public float widthTiles = 0.8f;

        [Tooltip("Bar height in tiles (world units).")]
        public float heightTiles = 0.08f;

        [Tooltip("Vertical offset above the actor (in tiles).")]
        public float yOffsetTiles = 0.6f;

        [Tooltip("Background color.")]
        public Color bgColor = new Color(0f, 0f, 0f, 0.75f);

        [Tooltip("Fill color.")]
        public Color fillColor = new Color(0.2f, 1f, 0.2f, 0.95f);

        private SpriteRenderer _bg;
        private SpriteRenderer _fill;
        private TJNK.Farwander.Actors.Health _health;
        private Transform _target;
        private float _w, _h;

        void OnEnable()
        {
            _health = GetComponentInParent<TJNK.Farwander.Actors.Health>();
            _target = _health ? _health.transform : transform.parent;
            EnsureSprites();
            RebuildGeometry();
            if (_health)
            {
                _health.OnHealthChanged -= OnHealthChanged;
                _health.OnHealthChanged += OnHealthChanged;
                _health.OnDeath       -= OnDeath;
                _health.OnDeath       += OnDeath;
                // initialize once
                OnHealthChanged(_health);
            }
        }

        void OnDisable()
        {
            if (_health)
            {
                _health.OnHealthChanged -= OnHealthChanged;
                _health.OnDeath         -= OnDeath;
            }
        }

        void Update()
        {
            if (!_target) return;
            var basePos = _target.position;
            transform.position = new Vector3(basePos.x, basePos.y + yOffsetTiles, basePos.z);
            transform.rotation = Quaternion.identity; // no tilt
        }

        private void OnHealthChanged(TJNK.Farwander.Actors.Health h)
        {
            if (!_fill) return;
            float pct = Mathf.Clamp01(h.maxHp > 0 ? (float)h.hp / h.maxHp : 0f);

            // Adjust fill width via SpriteRenderer.size (requires FullRect sprite + Sliced draw mode)
            _fill.size = new Vector2(_w * pct, _h);
            _fill.transform.localPosition = new Vector3(-_w * 0.5f + (_w * pct) * 0.5f, 0f, 0f);

            // Optional color shift when low
            _fill.color = Color.Lerp(new Color(1f, 0.2f, 0.2f, fillColor.a), fillColor, pct);

            // Hide bar when full HP to reduce clutter (toggle if you prefer always-on)
            // gameObject.SetActive(h.hp < h.maxHp);
        }

        private void OnDeath(TJNK.Farwander.Actors.Health h)
        {
            // Parent GameObject will be destroyed by Health
        }

        private void EnsureSprites()
        {
            if (!_bg)
            {
                var bgGO = new GameObject("HPBar_BG");
                bgGO.transform.SetParent(transform, false);
                _bg = bgGO.AddComponent<SpriteRenderer>();
                _bg.sortingOrder = 1000;
                _bg.sprite = MakeUnitFullRectSprite();
                _bg.drawMode = SpriteDrawMode.Sliced; // requires FullRect
                _bg.color = bgColor;
            }
            if (!_fill)
            {
                var fillGO = new GameObject("HPBar_Fill");
                fillGO.transform.SetParent(transform, false);
                _fill = fillGO.AddComponent<SpriteRenderer>();
                _fill.sortingOrder = 1001;
                _fill.sprite = MakeUnitFullRectSprite();
                _fill.drawMode = SpriteDrawMode.Sliced; // requires FullRect
                _fill.color = fillColor;
            }
        }

        private void RebuildGeometry()
        {
            _w = Mathf.Max(0.01f, widthTiles);
            _h = Mathf.Max(0.01f, heightTiles);

            _bg.size = new Vector2(_w, _h);
            _bg.transform.localPosition = Vector3.zero;

            // Fill will be sized when OnHealthChanged fires
        }

        // ---- FullRect sprite factory (fixes tiling warning + enables proper resizing) ----
        private static Sprite _unitSprite;
        private static Texture2D _unitTex;

        private static Sprite MakeUnitFullRectSprite()
        {
            if (_unitSprite) return _unitSprite;

            _unitTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var white = new[] { Color.white, Color.white, Color.white, Color.white };
            _unitTex.SetPixels(white);
            _unitTex.Apply(false, false);

            // Use the Sprite.Create overload that sets meshType = FullRect
            // Rect = full 2x2, pivot center, PPU = 32 (any value works since we control size via SpriteRenderer.size)
            _unitSprite = Sprite.Create(
                _unitTex,
                new Rect(0, 0, 2, 2),
                new Vector2(0.5f, 0.5f),
                32f,
                0,
                SpriteMeshType.FullRect,   // <-- critical
                Vector4.zero,
                false
            );
            _unitSprite.name = "UnitWhite_FullRect";
            return _unitSprite;
        }
    }
}
