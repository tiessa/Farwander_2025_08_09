using UnityEngine;
using UnityEngine.Tilemaps;
using TJNK.Farwander.Core;
using TJNK.Farwander.Modules.Game;
using TJNK.Farwander.Modules.Generation;

namespace TJNK.Farwander.Modules.UI
{
    /// <summary>
    /// Minimal runtime renderer that draws the generated DungeonMap onto Unity Tilemaps.
    /// Pure view module: subscribes to Gen_Complete and builds a Grid + two Tilemaps (Floor/Wall).
    /// Requires no configs; uses solid-color tiles if no atlas wiring exists yet.
    /// </summary>
    public sealed class MapViewModuleProvider : ModuleProvider
    {
        public override string ModuleId { get { return "UI.MapView"; } }

        private EventBus _bus;
        private Tilemap _floorTM, _wallTM;
        private Tile _floorTile, _wallTile;

        public override void Bind(GameCore core)
        {
            base.Bind(core);
            _bus = core.Bus;
            _bus.Subscribe<Gen_Complete>(OnGenComplete);
        }

        private void OnGenComplete(Gen_Complete e)
        {
            var map = e.DungeonMap as DungeonMap;
            if (map == null) return;
            EnsureSceneObjects();
            EnsureTiles();

            _floorTM.ClearAllTiles();
            _wallTM.ClearAllTiles();

            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var p = new Vector3Int(x, y, 0);
                if (map.Tiles[x, y] == MapTile.Floor) _floorTM.SetTile(p, _floorTile);
                else _wallTM.SetTile(p, _wallTile);
            }

            FitCameraToMap(map.Width, map.Height);
            Debug.Log($"[MapView] Rendered map {map.Width}x{map.Height}");
        }

        private void EnsureSceneObjects()
        {
            if (_floorTM != null && _wallTM != null) return;

            var gridGo = new GameObject("Grid");
            gridGo.transform.SetParent(this.transform, false);
            gridGo.AddComponent<Grid>();

            _floorTM = CreateLayer(gridGo.transform, "Floor", 0);
            _wallTM = CreateLayer(gridGo.transform, "Wall", 1);
        }

        private static Tilemap CreateLayer(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tm = go.AddComponent<Tilemap>();
            var tmr = go.AddComponent<TilemapRenderer>();
            tmr.sortingOrder = sortingOrder;
            return tm;
        }

        private void EnsureTiles()
        {
            if (_floorTile == null) _floorTile = MakeSolidTile(new Color32(180, 180, 180, 255));
            if (_wallTile == null) _wallTile = MakeSolidTile(new Color32(90, 90, 90, 255));
        }

        private static Tile MakeSolidTile(Color color)
        {
            var size = 32; var ppu = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var c32 = (Color32)color; var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c32;
            tex.SetPixels32(pixels); tex.filterMode = FilterMode.Point; tex.wrapMode = TextureWrapMode.Clamp; tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
            var tile = ScriptableObject.CreateInstance<Tile>(); tile.sprite = sprite; return tile;
        }

        private static void FitCameraToMap(int width, int height)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                cam.orthographic = true; cam.tag = "MainCamera";
            }
            cam.orthographic = true;
            cam.transform.position = new Vector3(width / 2f, height / 2f, -10f);
            // Size so that the entire map fits vertically; adjust for aspect horizontally
            var halfH = height / 2f + 1f;
            var halfW = width / 2f + 1f;
            var neededSizeByWidth = halfW / Mathf.Max(0.0001f, cam.aspect);
            cam.orthographicSize = Mathf.Max(halfH, neededSizeByWidth);
        }
    }
}
