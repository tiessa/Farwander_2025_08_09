#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

using TJNK.Farwander.Content;
using TJNK.Farwander.Systems;
using TJNK.Farwander.Actors;

namespace TJNK.Farwander.EditorTools
{
    public static class FarwanderSetup
    {
        private const string Root = "Assets/TJNK/Farwander";
        private static readonly string Art     = Root + "/Art";
        private static readonly string Tiles   = Root + "/Tiles";
        private static readonly string SOs     = Root + "/ScriptableObjects";
        private static readonly string Prefabs = Root + "/Prefabs";
        private static readonly string Scenes  = Root + "/Scenes";

        private const int PPU = 32; // pixels per unit for generated sprites

        [MenuItem("TJNK/Farwander/Setup")]
        public static void CreateAll()
        {
            EnsureFolders();

            // 1) Generate textures + sprites
            var floorSprite  = MakeColorSprite(Art + "/floor.png",  new Color32(180, 180, 180, 255)); // light gray
            var wallSprite   = MakeColorSprite(Art + "/wall.png",   new Color32(70, 70, 70, 255));    // dark gray
            var playerSprite = MakeColorSprite(Art + "/player.png", new Color32(52, 152, 219, 255));  // blue
            var enemySprite  = MakeColorSprite(Art + "/enemy.png",  new Color32(231, 76, 60, 255));   // red

            // 2) Create Tile assets from sprites
            var floorTile = CreateTile(Tiles + "/Floor.asset", floorSprite);
            var wallTile  = CreateTile(Tiles + "/Wall.asset",  wallSprite);

            // 3) Create TileDef SOs
            var floorDef = CreateTileDef(SOs + "/FloorDef.asset", floorTile, walkable: true,  blocksSight: false);
            var wallDef  = CreateTileDef(SOs + "/WallDef.asset",  wallTile,  walkable: false, blocksSight: true);

            // 4) Create Tileset SO
            var tileset = CreateTileset(SOs + "/Tileset.asset", floorDef, wallDef);

            // 5) Create prefabs: Player & Enemy
            var playerPrefab = CreateActorPrefab(Prefabs + "/Player.prefab", "Player", playerSprite, typeof(PlayerController));
            var enemyPrefab  = CreateActorPrefab(Prefabs + "/Enemy.prefab",  "Enemy",  enemySprite,  typeof(EnemyController));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 6) Build scene objects
            BuildScene(tileset, playerPrefab, enemyPrefab);

            // 7) Save a scene file the first time
            var scenePath = Scenes + "/Farwander.unity";
            if (!File.Exists(scenePath))
            {
                Directory.CreateDirectory(Scenes);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(), scenePath);
            }

            EditorUtility.DisplayDialog("Farwander Setup", "Scene and assets created. Press Play to test!", "Nice");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(Root);
            Directory.CreateDirectory(Art);
            Directory.CreateDirectory(Tiles);
            Directory.CreateDirectory(SOs);
            Directory.CreateDirectory(Prefabs);
            Directory.CreateDirectory(Scenes);
            AssetDatabase.Refresh();
        }

        // --------- Asset Creation Helpers ---------

        private static Sprite MakeColorSprite(string path, Color32 color, int size = 32)
        {
            path = path.Replace("\\", "/");

            // Create a flat color texture and write PNG
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = tex.GetPixels32();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels32(pixels);
            tex.Apply(false, false);

            var png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(path, png);

            // Import and configure as Sprite (Point)
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                // Fallback: refresh and retry (can happen if import is still batching)
                AssetDatabase.Refresh();
                importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogWarning("Failed to get TextureImporter for " + path + ". Using default sprite import settings.");
                    return AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = PPU;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Tile CreateTile(string assetPath, Sprite sprite)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            AssetDatabase.CreateAsset(tile, assetPath);
            return tile;
        }

        private static TileDef CreateTileDef(string assetPath, Tile tile, bool walkable, bool blocksSight)
        {
            var def = ScriptableObject.CreateInstance<TileDef>();
            def.tile = tile;
            def.walkable = walkable;
            def.blocksSight = blocksSight;
            AssetDatabase.CreateAsset(def, assetPath);
            return def;
        }

        private static Tileset CreateTileset(string assetPath, TileDef floor, TileDef wall)
        {
            var ts = ScriptableObject.CreateInstance<Tileset>();
            ts.floor = floor;
            ts.wall = wall;
            AssetDatabase.CreateAsset(ts, assetPath);
            return ts;
        }

        private static GameObject CreateActorPrefab(string path, string name, Sprite sprite, System.Type controllerType)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;

            // IMPORTANT: Only add the controller, which inherits Actor
            go.AddComponent(controllerType);

            // Overwrite/create prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }
        
        // --------- Scene Creation ---------

        private static void BuildScene(Tileset tileset, GameObject playerPrefab, GameObject enemyPrefab)
        {
            // Grid + Tilemap
            var gridGO = GameObject.Find("Grid");
            Grid grid;
            Tilemap tilemap;

            if (gridGO == null)
            {
                gridGO = new GameObject("Grid");
                grid = gridGO.AddComponent<Grid>();
                grid.cellLayout = GridLayout.CellLayout.Rectangle;
                grid.cellSize = new Vector3(1, 1, 0);

                var groundGO = new GameObject("Ground");
                groundGO.transform.SetParent(gridGO.transform);
                tilemap = groundGO.AddComponent<Tilemap>();
                groundGO.AddComponent<TilemapRenderer>();
            }
            else
            {
                grid = gridGO.GetComponent<Grid>();
                tilemap = gridGO.transform.Find("Ground")?.GetComponent<Tilemap>();
            }

            // TurnManager
            if (GameObject.Find("TurnManager") == null)
                new GameObject("TurnManager").AddComponent<TurnManager>();

            // GameBootstrap
            var gbGO = GameObject.Find("GameBootstrap");
            if (gbGO == null)
            {
                gbGO = new GameObject("GameBootstrap");
                var bootstrap = gbGO.AddComponent<GameBootstrap>();
                bootstrap.grid = grid;
                bootstrap.groundTilemap = tilemap;
                bootstrap.tileset = tileset;

                var so = new SerializedObject(bootstrap);
                so.FindProperty("playerPrefab").objectReferenceValue = playerPrefab.GetComponent<Actor>();
                so.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab.GetComponent<Actor>();
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Camera framing
            var cam = Camera.main;
            if (cam == null)
            {
                var camGO = new GameObject("Main Camera");
                cam = camGO.AddComponent<Camera>();
                camGO.tag = "MainCamera";
            }
            cam.orthographic = true;
            cam.orthographicSize = 10f;
        }
    }
}
#endif
