using UnityEngine;

namespace TJNK.Farwander.Actors.UI
{
    /// <summary>Simple world-space 2-sprite health bar that follows the Actor.</summary>
    [ExecuteAlways]
    public class HealthBar : MonoBehaviour
    {
        [Tooltip("Pixels per unit used by your sprites (just affects size).")]
        public float pixelsPerUnit = 32f;

        [Tooltip("Bar width in tiles (1 = width of one cell).")]
        public float widthTiles = 0.8f;

        [Tooltip("Bar height in tiles.")]
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

        void Awake()
        {
            _health = GetComponentInParent<TJNK.Farwander.Actors.Health>();
            _target = _health ? _health.transform : transform.parent;
            EnsureSprites();
            Rebuild();
            if (_health)
            {
                _health.OnHealthChanged -= OnHealthChanged;
                _health.OnHealthChanged += OnHealthChanged;
                _health.OnDeath       -= OnDeath;
                _health.OnDeath       += OnDeath;
            }
        }

        void OnDestroy()
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
            // Keep bar scale stable regardless of parent scaling
            transform.rotation = Quaternion.identity;
        }

        private void OnHealthChanged(TJNK.Farwander.Actors.Health h)
        {
            if (!_fill) return;
            float pct = Mathf.Clamp01(h.maxHp > 0 ? (float)h.hp / h.maxHp : 0f);
            _fill.size = new Vector2(_w * pct, _h);
            _fill.transform.localPosition = new Vector3(-_w * 0.5f + (_w * pct) * 0.5f, 0f, 0f);
            // Optional color shift when low
            _fill.color = Color.Lerp(new Color(1f, 0.2f, 0.2f, fillColor.a), fillColor, pct);
            gameObject.SetActive(h.hp < h.maxHp); // hide when full HP
        }

        private void OnDeath(TJNK.Farwander.Actors.Health h)
        {
            // Let the bar disappear with the actor (parent will be destroyed)
        }

        private void EnsureSprites()
        {
            if (!_bg)
            {
                var bgGO = new GameObject("HPBar_BG");
                bgGO.transform.SetParent(transform, false);
                _bg = bgGO.AddComponent<SpriteRenderer>();
                _bg.sortingOrder = 1000;
                _bg.sprite = MakeUnitSprite();
                _bg.drawMode = SpriteDrawMode.Sliced;
                _bg.color = bgColor;
            }
            if (!_fill)
            {
                var fillGO = new GameObject("HPBar_Fill");
                fillGO.transform.SetParent(transform, false);
                _fill = fillGO.AddComponent<SpriteRenderer>();
                _fill.sortingOrder = 1001;
                _fill.sprite = MakeUnitSprite();
                _fill.drawMode = SpriteDrawMode.Sliced;
                _fill.color = fillColor;
            }
        }

        private void Rebuild()
        {
            _w = widthTiles;
            _h = heightTiles;
            _bg.size = new Vector2(_w, _h);
            _bg.transform.localPosition = new Vector3(0f, 0f, 0f);
            if (_health) OnHealthChanged(_health);
        }

        private static Sprite _unitSprite;
        private static Sprite MakeUnitSprite()
        {
            if (_unitSprite) return _unitSprite;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            _unitSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 32f);
            _unitSprite.name = "UnitWhite";
            return _unitSprite;
        }
    }
}
