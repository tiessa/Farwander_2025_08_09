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
        private static readonly string ItemsSOFolder = SOs + "/Items";
        private static readonly string ItemIcons = Art + "/Items";
        
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
            
            // 8) Default Item SOs (idempotent)
            var apple = EnsureItem(ItemsSOFolder + "/Apple.asset", "apple", "Apple",
                TJNK.Farwander.Items.ItemKind.Consumable, 10, TJNK.Farwander.Items.EquipSlot.None,
                ItemIcons + "/apple.png", new Color(0.9f, 0.2f, 0.2f));

            var sword = EnsureItem(ItemsSOFolder + "/IronSword.asset", "iron_sword", "Iron Sword",
                TJNK.Farwander.Items.ItemKind.Weapon, 1, TJNK.Farwander.Items.EquipSlot.MainHand,
                ItemIcons + "/iron_sword.png", new Color(0.7f, 0.7f, 0.75f));

            var shield = EnsureItem(ItemsSOFolder + "/WoodenShield.asset", "wood_shield", "Wooden Shield",
                TJNK.Farwander.Items.ItemKind.Armor, 1, TJNK.Farwander.Items.EquipSlot.OffHand,
                ItemIcons + "/wood_shield.png", new Color(0.55f, 0.4f, 0.2f));

            var potion = EnsureItem(ItemsSOFolder + "/HealthPotion.asset", "heal_potion", "Health Potion",
                TJNK.Farwander.Items.ItemKind.Consumable, 10, TJNK.Farwander.Items.EquipSlot.None,
                ItemIcons + "/health_potion.png", new Color(0.9f, 0.1f, 0.9f));

            var wand = EnsureItem(ItemsSOFolder + "/MagicMissileWand.asset", "wand_missile", "Magic Missile Wand",
                TJNK.Farwander.Items.ItemKind.Wand, 1, TJNK.Farwander.Items.EquipSlot.MainHand,
                ItemIcons + "/wand_missile.png", new Color(0.4f, 0.6f, 1f));            

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

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
            Directory.CreateDirectory(ItemsSOFolder);
            Directory.CreateDirectory(ItemIcons);
            AssetDatabase.Refresh();
        }

        // --------- Asset Creation Helpers ---------

        private static Texture2D MakeSolidTex(int w, int h, Color c)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var arr = tex.GetPixels32();
            for (int i = 0; i < arr.Length; i++) arr[i] = c;
            tex.SetPixels32(arr);
            tex.Apply(false, false);
            return tex;
        }

        private static Sprite CreateIconSprite(string pathPng, Color color)
        {
            var tex = MakeSolidTex(32, 32, color);
            var png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            System.IO.File.WriteAllBytes(pathPng, png);
            AssetDatabase.ImportAsset(pathPng, ImportAssetOptions.ForceUpdate);
            var imp = (TextureImporter)AssetImporter.GetAtPath(pathPng);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            // imp.spriteMeshType = SpriteMeshType.FullRect;
            imp.spritePixelsPerUnit = 32;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(pathPng);
        }

        private static T CreateOrLoadAsset<T>(string assetPath) where T : ScriptableObject
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (a) return a;
            a = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(a, assetPath);
            return a;
        }

        private static TJNK.Farwander.Items.ItemDef EnsureItem(
            string soPath, string id, string name, TJNK.Farwander.Items.ItemKind kind,
            int maxStack, TJNK.Farwander.Items.EquipSlot slot, string iconPath, Color iconColor)
        {
            var item = CreateOrLoadAsset<TJNK.Farwander.Items.ItemDef>(soPath);
            item.id = id;
            item.displayName = name;
            item.kind = kind;
            item.maxStack = Mathf.Max(1, maxStack);
            item.slot = slot;
            if (!item.icon)
            {
                var spr = CreateIconSprite(iconPath, iconColor);
                item.icon = spr;
                EditorUtility.SetDirty(item);
            }
            return item;
        }
        
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
