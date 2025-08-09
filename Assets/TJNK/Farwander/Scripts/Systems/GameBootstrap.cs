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
            
            EnsureHud();            

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
        
        private void EnsureHud()
        {
            // Try to find existing HUD
            var hud = GameObject.Find("HUD");
            if (hud != null) return;

            // Canvas
            hud = new GameObject("HUD");
            var canvas = hud.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hud.AddComponent<CanvasScaler>();
            hud.AddComponent<GraphicRaycaster>();

            // Panel background
            var panelGO = new GameObject("CombatLogPanel");
            panelGO.transform.SetParent(hud.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.4f);

            // Anchor panel bottom-left
            var r = panelGO.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0, 0);
            r.anchorMax = new Vector2(0, 0);
            r.pivot     = new Vector2(0, 0);
            r.anchoredPosition = new Vector2(8, 8);
            r.sizeDelta = new Vector2(520, 180);

            // Text
            var textGO = new GameObject("CombatLogText");
            textGO.transform.SetParent(panelGO.transform, false);
            var text = textGO.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.LowerLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.fontSize = 16;
            text.color = Color.white;

            var tr = textGO.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0, 0);
            tr.anchorMax = new Vector2(1, 1);
            tr.offsetMin = new Vector2(8, 8);
            tr.offsetMax = new Vector2(8, 8);

            // Component
            var log = hud.AddComponent<TJNK.Farwander.Systems.UI.CombatLog>();
            log.logText = text;
        }        
    }
}