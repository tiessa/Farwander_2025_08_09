using UnityEngine;
using UnityEngine.Tilemaps;
using TJNK.Farwander.Content;
using TJNK.Farwander.Core;
using TJNK.Farwander.Generation;
using TJNK.Farwander.Actors;
using TJNK.Farwander.Actors.UI;
using TJNK.Farwander.Systems.Visibility;
using UnityEngine.UI;

namespace TJNK.Farwander.Systems
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        public Grid grid;                // Assign your Grid
        public Tilemap groundTilemap;    // Assign the Ground Tilemap

        [Header("Content")]
        public Tileset tileset;          // Assign the Tileset SO

        [Header("Map Settings")]
        public int width = 64;
        public int height = 48;
        public int seed = 0;

        [Header("Prefabs")]
        public Actor playerPrefab;       // Prefab with Actor + PlayerController
        public Actor enemyPrefab;        // Prefab with Actor + EnemyController
        public int enemyCount = 6;

        private MapRuntime runtime;

        void Start()
        {
            // Ensure ActorIndex exists
            if (ActorIndex.Instance == null)
            {
                new GameObject("ActorIndex").AddComponent<ActorIndex>();
            }
            
            if (TJNK.Farwander.Systems.ItemIndex.Instance == null)
                new GameObject("ItemIndex").AddComponent<TJNK.Farwander.Systems.ItemIndex>();
            
            // Build map
            runtime = new MapRuntime(groundTilemap, tileset, width, height, seed);

            // Spawn player
            var p = Instantiate(playerPrefab);
            if (!p.GetComponent<TJNK.Farwander.Actors.Health>())
                p.gameObject.AddComponent<TJNK.Farwander.Actors.Health>();
            if (!p.GetComponentInChildren<HealthBar>())
            {
                var hb = new GameObject("HealthBar").AddComponent<HealthBar>();
                hb.transform.SetParent(p.transform, false);
            }            p.grid = grid;
            var playerStart = runtime.Generator.GetRandomFloor();
            p.Place(playerStart);

            var pc = p.GetComponent<PlayerController>();
            pc.Runtime = runtime;

            // Attach/init TileTinter on the Ground tilemap
            var tinter = groundTilemap.GetComponent<TileTinter>();
            if (tinter == null) tinter = groundTilemap.gameObject.AddComponent<TileTinter>();
            tinter.Init(runtime.Visibility, runtime.Generator.Width, runtime.Generator.Height); 
            
            // Initial FOV compute (match player’s starting position)
            runtime.Visibility.Recompute(playerStart, 8, runtime.Generator.BlocksSight); 
            
            // Player
            var pv = p.GetComponent<ActorVisibility>();
            if (pv == null) pv = p.gameObject.AddComponent<ActorVisibility>();
            pv.Init(runtime.Visibility);
            
            // Snap camera to player
            var cam = Camera.main;
            if (cam != null)
            {
                var follow = cam.GetComponent<TJNK.Farwander.Systems.CameraFollow>();
                if (follow == null) follow = cam.gameObject.AddComponent<TJNK.Farwander.Systems.CameraFollow>();
                follow.target = p.transform;     // follow the player
                follow.SnapNow();                // start centered
            } 
            
            // Ensure Inventory on player
            var inv = p.GetComponent<TJNK.Farwander.Items.Inventory>();
            if (!inv) inv = p.gameObject.AddComponent<TJNK.Farwander.Items.Inventory>();
            
            // Build HUD & bind Inventory UI
            var hud = EnsureHud();
            var invPanel = hud.transform.Find("InventoryPanel");
            if (invPanel)
            {
                var invUI = invPanel.GetComponent<TJNK.Farwander.Systems.UI.InventoryUI>();
                if (invUI) invUI.Bind(inv);
            }            
            
            // Spawn enemies
            for (int i = 0; i < enemyCount; i++)
            {
                var e = Instantiate(enemyPrefab);
                if (!e.GetComponent<TJNK.Farwander.Actors.Health>())
                    e.gameObject.AddComponent<TJNK.Farwander.Actors.Health>();
                if (!e.GetComponentInChildren<HealthBar>())
                {
                    var hb = new GameObject("HealthBar").AddComponent<HealthBar>();
                    hb.transform.SetParent(e.transform, false);
                }                
                e.grid = grid;
                e.Place(runtime.Generator.GetRandomFloor());
                
                var ec = e.GetComponent<EnemyController>();
                ec.Runtime = runtime;
                ec.Player = p;
                
                var ev = e.GetComponent<ActorVisibility>();
                if (ev == null) ev = e.gameObject.AddComponent<ActorVisibility>();
                ev.Init(runtime.Visibility);                
            }
        }

        private GameObject EnsureHud()
        {
            // Find or create HUD root
            var hud = GameObject.Find("HUD");
            if (hud != null) return hud;

            hud = new GameObject("HUD");
            var canvas = hud.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hud.AddComponent<CanvasScaler>();
            hud.AddComponent<GraphicRaycaster>();

            // ---------- Combat Log Panel (bottom-left) ----------
            var logPanel = new GameObject("CombatLogPanel");
            logPanel.transform.SetParent(hud.transform, false);
            var logBg = logPanel.AddComponent<Image>();
            logBg.color = new Color(0f, 0f, 0f, 0.4f);
            var logRt = logPanel.GetComponent<RectTransform>();
            logRt.anchorMin = new Vector2(0, 0);
            logRt.anchorMax = new Vector2(0, 0);
            logRt.pivot = new Vector2(0, 0);
            logRt.anchoredPosition = new Vector2(8, 8);
            logRt.sizeDelta = new Vector2(520, 180);

            var textGO = new GameObject("CombatLogText");
            textGO.transform.SetParent(logPanel.transform, false);
            var text = textGO.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.LowerLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.fontSize = 16;
            text.color = Color.white;
            var textRt = textGO.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(8, 8);
            textRt.offsetMax = new Vector2(-8, -8);

            var log = hud.AddComponent<TJNK.Farwander.Systems.UI.CombatLog>();
            log.logText = text;

            // ---------- Inventory Panel (bottom-right) ----------
            var invPanel = new GameObject("InventoryPanel");
            invPanel.transform.SetParent(hud.transform, false);
            var invBg = invPanel.AddComponent<Image>();
            invBg.color = new Color(0f, 0f, 0f, 0.35f);
            var invRt = invPanel.GetComponent<RectTransform>();
            invRt.anchorMin = new Vector2(1, 0);
            invRt.anchorMax = new Vector2(1, 0);
            invRt.pivot = new Vector2(1, 0);
            invRt.anchoredPosition = new Vector2(-8, 8);
            invRt.sizeDelta = new Vector2(360, 232);

            // Grid container
            var grid = new GameObject("Grid");
            grid.transform.SetParent(invPanel.transform, false);
            var gridRt = grid.AddComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0, 0);
            gridRt.anchorMax = new Vector2(1, 1);
            gridRt.offsetMin = new Vector2(8, 8);
            gridRt.offsetMax = new Vector2(-8, -8);

            var gl = grid.AddComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(64, 64);
            gl.spacing = new Vector2(6, 6);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 5;

            // Slot prefab (disabled child)
            var slotPrefab = new GameObject("SlotPrefab");
            slotPrefab.transform.SetParent(invPanel.transform, false);
            slotPrefab.SetActive(false);
            var slotRt = slotPrefab.AddComponent<RectTransform>();
            slotRt.sizeDelta = new Vector2(64, 64);
            var slotBtn = slotPrefab.AddComponent<Button>();

            // Icon
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(slotPrefab.transform, false);
            var icon = iconGO.AddComponent<Image>();
            var iconRt = iconGO.GetComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;

            // Count
            var countGO = new GameObject("Count");
            countGO.transform.SetParent(slotPrefab.transform, false);
            var count = countGO.AddComponent<Text>();
            count.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            count.alignment = TextAnchor.LowerRight;
            count.fontSize = 16;
            count.color = Color.white;
            var countRt = countGO.GetComponent<RectTransform>();
            countRt.anchorMin = Vector2.zero;
            countRt.anchorMax = Vector2.one;
            countRt.offsetMin = new Vector2(4, 4);
            countRt.offsetMax = new Vector2(-4, -4);

            // Highlight
            var hlGO = new GameObject("Highlight");
            hlGO.transform.SetParent(slotPrefab.transform, false);
            var hl = hlGO.AddComponent<Image>();
            hl.color = new Color(1f, 1f, 1f, 0.18f);
            hl.enabled = false;
            var hlRt = hlGO.GetComponent<RectTransform>();
            hlRt.anchorMin = Vector2.zero;
            hlRt.anchorMax = Vector2.one;
            hlRt.offsetMin = Vector2.zero;
            hlRt.offsetMax = Vector2.zero;

            // InventoryUI component on panel; wire references (Bind happens later in Start)
            var invUI = invPanel.AddComponent<TJNK.Farwander.Systems.UI.InventoryUI>();
            invUI.gridRoot = grid.transform;
            invUI.slotPrefab = slotPrefab;

            return hud;
        }
    }
}