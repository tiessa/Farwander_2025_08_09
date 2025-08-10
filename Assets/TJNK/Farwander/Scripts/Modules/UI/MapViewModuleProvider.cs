using UnityEngine;
using UnityEngine.Tilemaps;
using TJNK.Farwander.Core;
using TJNK.Farwander.Modules.Game;
using TJNK.Farwander.Modules.Generation;
using TJNK.Farwander.Modules.Game.Runtime.Entities;
using TJNK.Farwander.Modules.Game.Runtime.State;
using TJNK.Farwander.Modules.Fov;
using System.Collections.Generic;

namespace TJNK.Farwander.Modules.UI
{
    /// <summary>
    /// Renders dungeon tilemaps, player, enemy sprites, and a fog overlay tilemap.
    /// - Only moves sprites on successful Move_Resolved.
    /// - Fog updates on Fov_Updated; enemies outside LOS are hidden.
    /// </summary>
    public sealed class MapViewModuleProvider : ModuleProvider
    { 
        // Add to fields:
        [SerializeField] bool revealAllEnemies = false;
        [SerializeField] bool showEnemiesWhenExplored = false;
        
        public override string ModuleId { get { return "UI.MapView"; } }

        private EventBus _bus; private QueryRegistry _q;
        private Tilemap _floorTM, _wallTM, _fogTM;
        private Tile _floorTile, _wallTile, _fogDarkTile, _fogDimTile;
        private GameObject _playerGo;
        private readonly Dictionary<int, GameObject> _enemyViews = new Dictionary<int, GameObject>();

        public override void Bind(GameCore core)
        {
            base.Bind(core);
            _bus = core.Bus; _q = core.Queries;
            _bus.Subscribe<Gen_Complete>(OnGenComplete);
            _bus.Subscribe<Entity_Spawned>(OnEntitySpawned);
            _bus.Subscribe<Move_Resolved>(OnMoveResolved);
            _bus.Subscribe<Fov_Updated>(_ => OnFovUpdated());
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

            // Fill fog as unexplored until FOV updates
            _fogTM.ClearAllTiles();
            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                _fogTM.SetTile(new Vector3Int(x, y, 0), _fogDarkTile);

            FitCameraToMap(map.Width, map.Height);
            Debug.Log($"[MapView] Rendered map {map.Width}x{map.Height}");

            // If player already exists in state, visualize now
            TryMaterializeExistingPlayer();
        }

        private void OnEntitySpawned(Entity_Spawned e)
        {
            EnsureSceneObjects();
            EnsureTiles();

            if (e.IsPlayer)
            {
                if (_playerGo == null)
                {
                    _playerGo = new GameObject("PlayerView");
                    _playerGo.transform.SetParent(this.transform, false);
                    var sr = _playerGo.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = 10; // above tiles & fog
                    sr.sprite = MakeSolidSprite(new Color32(40, 140, 255, 255)); // blue
                    Debug.Log($"[MapView] PlayerView created (id={e.EntityId})");
                }
                _playerGo.transform.position = new Vector3(e.Pos.x + 0.5f, e.Pos.y + 0.5f, 0f);
                FollowCamera(_playerGo.transform.position);
                return;
            }

            // Enemy
            if (!_enemyViews.TryGetValue(e.EntityId, out var go) || go == null)
            {
                go = new GameObject($"EnemyView_{e.EntityId}");
                go.transform.SetParent(this.transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 9; // below player
                sr.sprite = MakeSolidSprite(new Color32(200, 60, 60, 255)); // red
                _enemyViews[e.EntityId] = go;
                Debug.Log($"[MapView] EnemyView created (id={e.EntityId}, name={e.SpriteName})");
            }
            go.transform.position = new Vector3(e.Pos.x + 0.5f, e.Pos.y + 0.5f, 0f);
        }

        private void OnMoveResolved(Move_Resolved e)
        {
            var ps = _q != null ? _q.Get<IPlayerState>() : null;
            if (ps != null && e.EntityId == ps.PlayerId)
            {
                if (_playerGo == null) return;
                if (!e.Succeeded)
                {
                    var locs = _q.Get<IEntityLocations>();
                    if (locs.TryGet(e.EntityId, out var pos))
                        _playerGo.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
                    return;
                }
                _playerGo.transform.position = new Vector3(e.To.x + 0.5f, e.To.y + 0.5f, 0f);
                FollowCamera(_playerGo.transform.position);
                return;
            }

            if (_enemyViews.TryGetValue(e.EntityId, out var go) && go != null)
            {
                if (!e.Succeeded)
                {
                    var locs = _q.Get<IEntityLocations>();
                    if (locs.TryGet(e.EntityId, out var pos))
                        go.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
                    return;
                }
                go.transform.position = new Vector3(e.To.x + 0.5f, e.To.y + 0.5f, 0f);
            }
        }

        private void OnFovUpdated()
        {
            var fov = _q.Get<IFovQuery>(); if (fov == null) return;
            var mq  = _q.Get<IMapQuery>(); if (mq == null) return;

            for (int x = 0; x < mq.Size.x; x++)
            for (int y = 0; y < mq.Size.y; y++)
            {
                var p = new Vector3Int(x, y, 0);
                if (!fov.IsExplored(x, y))
                    _fogTM.SetTile(p, _fogDarkTile);
                else if (!fov.IsVisible(x, y))
                    _fogTM.SetTile(p, _fogDimTile);
                else
                    _fogTM.SetTile(p, null);
            }

            // Replace the toggle section in OnFovUpdated():
            foreach (var kv in _enemyViews)
            {
                var go = kv.Value; if (!go) continue;

                var wp = go.transform.position;
                int cx = Mathf.FloorToInt(wp.x), cy = Mathf.FloorToInt(wp.y);

                if (revealAllEnemies) { if (!go.activeSelf) go.SetActive(true); continue; }

                var fov2 = _q.Get<IFovQuery>();
                bool vis = fov2.IsVisible(cx, cy);
                if (!vis && showEnemiesWhenExplored) vis = fov2.IsExplored(cx, cy); // silhouettes

                if (go.activeSelf != vis) go.SetActive(vis);
            }
        }

        private void TryMaterializeExistingPlayer()
        {
            if (_playerGo != null || _q == null) return;
            var ps = _q.Get<IPlayerState>();
            var locs = _q.Get<IEntityLocations>();
            if (ps == null || locs == null) return;
            if (ps.PlayerId == 0) return;
            if (!locs.TryGet(ps.PlayerId, out var pos)) return;

            _playerGo = new GameObject("PlayerView");
            _playerGo.transform.SetParent(this.transform, false);
            var sr = _playerGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            sr.sprite = MakeSolidSprite(new Color32(40, 140, 255, 255));
            Debug.Log($"[MapView] PlayerView materialized from state (id={ps.PlayerId})");

            _playerGo.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
            FollowCamera(_playerGo.transform.position);
        }

        private void EnsureSceneObjects()
        {
            if (_floorTM != null && _wallTM != null && _fogTM != null) return;

            var gridGo = new GameObject("Grid");
            gridGo.transform.SetParent(this.transform, false);
            gridGo.AddComponent<Grid>();

            _floorTM = CreateLayer(gridGo.transform, "Floor", 0);
            _wallTM = CreateLayer(gridGo.transform, "Wall", 1);
            _fogTM  = CreateLayer(gridGo.transform, "Fog", 2);
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
            if (_wallTile == null)  _wallTile  = MakeSolidTile(new Color32(90,  90,  90,  255));
            if (_fogDarkTile == null) _fogDarkTile = MakeSolidTile(new Color32(10, 10, 10, 240));
            if (_fogDimTile == null)  _fogDimTile  = MakeSolidTile(new Color32(10, 10, 10, 160));
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

        private static Sprite MakeSolidSprite(Color color)
        {
            var size = 32; var ppu = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var c32 = (Color32)color; var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c32;
            tex.SetPixels32(pixels); tex.filterMode = FilterMode.Point; tex.wrapMode = TextureWrapMode.Clamp; tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
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
            var halfH = height / 2f + 1f;
            var halfW = width / 2f + 1f;
            var neededSizeByWidth = halfW / Mathf.Max(0.0001f, cam.aspect);
            cam.orthographicSize = Mathf.Max(halfH, neededSizeByWidth);
        }

        private static void FollowCamera(Vector3 worldPos)
        {
            var cam = Camera.main; if (cam == null) return;
            cam.transform.position = new Vector3(worldPos.x, worldPos.y, cam.transform.position.z);
        }
    }
}
