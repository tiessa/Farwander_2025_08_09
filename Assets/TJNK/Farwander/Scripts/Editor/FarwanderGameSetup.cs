#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TJNK.Farwander.Core;
using TJNK.Farwander.Modules.Game;
using TJNK.Farwander.Modules.UI;
using TJNK.Farwander.Modules.Input;

namespace TJNK.Farwander.Editor
{
    public static class FarwanderGameSetup
    {
        private const string Root = "Assets/TJNK/Farwander";
        private const string Cfg = Root + "/Configs";

        [MenuItem("TJNK/Farwander/Game/Create Project Config Assets", priority = 10)]
        public static void CreateProjectConfigs()
        {
            Directory.CreateDirectory(Cfg);

            var dungeon = ScriptableObject.CreateInstance<DungeonConfigSO>();
            var player = ScriptableObject.CreateInstance<PlayerConfigSO>();
            // NOTE: To avoid type conflicts, we do not create an EnemyRoster asset here.
            var itemDB = ScriptableObject.CreateInstance<ItemDatabaseSO>();
            var spellDB = ScriptableObject.CreateInstance<SpellDatabaseSO>();
            var combat = ScriptableObject.CreateInstance<CombatConfigSO>();
            var ui = ScriptableObject.CreateInstance<UIConfigSO>();
            var project = ScriptableObject.CreateInstance<ProjectConfigSO>();

            AssetDatabase.CreateAsset(dungeon, Cfg + "/DungeonConfig.asset");
            AssetDatabase.CreateAsset(player, Cfg + "/PlayerConfig.asset");
            AssetDatabase.CreateAsset(itemDB, Cfg + "/ItemDatabase.asset");
            AssetDatabase.CreateAsset(spellDB, Cfg + "/SpellDatabase.asset");
            AssetDatabase.CreateAsset(combat, Cfg + "/CombatConfig.asset");
            AssetDatabase.CreateAsset(ui, Cfg + "/UIConfig.asset");

            project.Dungeon = dungeon; project.Player = player;
            project.ItemDB = itemDB; project.SpellDB = spellDB; project.Combat = combat; project.UI = ui;
            // Leave project.Enemies as-is/null; controller handles null by falling back to 5 enemies.
            AssetDatabase.CreateAsset(project, Cfg + "/ProjectConfig.asset");

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Farwander", "Project config assets created under" + Cfg + "(Enemy roster not auto-created to avoid type conflicts)", "OK");
        }

        [MenuItem("TJNK/Farwander/Game/Create Scene (Core + Controller + MapView + Input)", priority = 12)]
        public static void CreateSceneAll()
        {
            var project = AssetDatabase.LoadAssetAtPath<ProjectConfigSO>(Cfg + "/ProjectConfig.asset");
            if (project == null)
            {
                if (EditorUtility.DisplayDialog("Farwander", "ProjectConfig.asset not found. Create now?", "Yes", "No"))
                    TJNK.Farwander.Editor.FarwanderGameSetup.CreateProjectConfigs();
                project = AssetDatabase.LoadAssetAtPath<ProjectConfigSO>(Cfg + "/ProjectConfig.asset");
                if (project == null) return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var rootGo = new GameObject("GameCore");
            var core = rootGo.AddComponent<GameCore>();

            var controllerGo = new GameObject("GameController");
            controllerGo.transform.SetParent(rootGo.transform);
            var provider = controllerGo.AddComponent<GameControllerModuleProvider>();
            provider.Config = project;

            var viewGo = new GameObject("MapView");
            viewGo.transform.SetParent(rootGo.transform);
            viewGo.AddComponent<MapViewModuleProvider>();

            var inputGo = new GameObject("InputRouter");
            inputGo.transform.SetParent(rootGo.transform);
            inputGo.AddComponent<InputRouterModuleProvider>();

            var path = Root + "/Scenes/Farwander_All.unity";
            Directory.CreateDirectory(Root + "/Scenes");
            EditorSceneManager.SaveScene(scene, path);
            EditorUtility.DisplayDialog("Farwander", "Scene saved at" + path, "OK");
        }
    }
}
#endif
