using UnityEngine;
using UnityEngine.Tilemaps;
using TJNK.Farwander.Content;
using TJNK.Farwander.Core;
using TJNK.Farwander.Generation;
using TJNK.Farwander.Actors;
using TJNK.Farwander.Systems.Visibility;

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

            // Build map
            runtime = new MapRuntime(groundTilemap, tileset, width, height, seed);

            // Spawn player
            var p = Instantiate(playerPrefab);
            if (!p.GetComponent<TJNK.Farwander.Actors.Health>())
                p.gameObject.AddComponent<TJNK.Farwander.Actors.Health>();
            p.grid = grid;
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
                e.grid = grid;
                e.Place(runtime.Generator.GetRandomFloor());

                var ec = e.GetComponent<EnemyController>();
                ec.Runtime = runtime;
                ec.Player = p;
            }
        }
    }
}