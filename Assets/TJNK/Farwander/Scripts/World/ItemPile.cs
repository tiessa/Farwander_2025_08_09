using UnityEngine;
using UnityEngine.UI; // TMP optional below
using TJNK.Farwander.Items;
using TJNK.Farwander.Core;
using TJNK.Farwander.Systems.Visibility;

namespace TJNK.Farwander.World
{
    [DisallowMultipleComponent]
    public class ItemPile : MonoBehaviour
    {
        public ItemInstance stack; // must be set by spawner
        public SpriteRenderer iconRenderer;
        public Canvas worldCanvas;
        public Text countText; // fallback
        public int sortingOrder = 20;

        public Grid grid; // assigned by spawner
        public GridPosition Cell => GridPosition.FromV3Int(grid.WorldToCell(transform.position));

        void Awake()
        {
            if (!iconRenderer)
            {
                iconRenderer = gameObject.AddComponent<SpriteRenderer>();
                iconRenderer.sortingOrder = sortingOrder;
            }

            if (!worldCanvas)
            {
                var canvasGO = new GameObject("CountCanvas");
                canvasGO.transform.SetParent(transform, false);
                worldCanvas = canvasGO.AddComponent<Canvas>();
                worldCanvas.renderMode = RenderMode.WorldSpace;
                worldCanvas.overrideSorting = true;
                worldCanvas.sortingOrder = sortingOrder + 1;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.dynamicPixelsPerUnit = 12;
                var textGO = new GameObject("Count");
                textGO.transform.SetParent(canvasGO.transform, false);
                countText = textGO.AddComponent<Text>();
                countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                countText.alignment = TextAnchor.LowerRight;
                countText.fontSize = 18;
                var rtCanvas = (RectTransform)worldCanvas.transform;
                rtCanvas.sizeDelta = new Vector2(0.9f, 0.5f);
                var rtText = (RectTransform)countText.transform;
                rtText.anchorMin = Vector2.zero; rtText.anchorMax = Vector2.one;
                rtText.offsetMin = rtText.offsetMax = Vector2.zero;
                textGO.transform.localPosition = new Vector3(0.35f, -0.35f, 0);
            }
        }

        public void Init(ItemInstance instance, Grid g, VisibilityMap vis = null)
        {
            stack = instance;
            grid = g;
            transform.localScale = Vector3.one;
            iconRenderer.sprite = stack.def.icon;
            UpdateLabel();

            // attach visibility toggle so piles respect FOV
            if (vis != null && GetComponent<ActorVisibility>() == null)
            {
                var av = gameObject.AddComponent<ActorVisibility>(); // reuses same toggle logic
                av.Init(vis);
            }
        }

        public void SetCell(GridPosition p)
        {
            transform.position = grid.CellToWorld(p.ToV3Int()) + new Vector3(0.5f, 0.5f, 0f);
        }

        public void AddCount(int amount)
        {
            stack.count += Mathf.Max(0, amount);
            if (stack.count <= 0) Destroy(gameObject);
            else UpdateLabel();
        }

        public ItemInstance TakeAll()
        {
            var take = stack;
            stack = null;
            Destroy(gameObject);
            return take;
        }

        private void UpdateLabel()
        {
            if (countText) countText.text = (stack.def.maxStack > 1 && stack.count > 1) ? stack.count.ToString() : "";
        }
    }
}
