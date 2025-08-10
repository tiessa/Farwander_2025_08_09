#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TJNK.Farwander.Core;

namespace TJNK.Farwander.Editor
{
    public static class FarwanderEditorMenu
    {
        private const string Root = "Assets/TJNK/Farwander";

        [MenuItem("TJNK/Farwander/Create Default Scene", priority = 1)]
        public static void CreateDefaultScene()
        {
            var gameCoreType = typeof(GameCore);
            if (gameCoreType == null)
            {
                EditorUtility.DisplayDialog("Farwander", "Scripts not compiled yet. Please wait, then try again.", "OK");
                return;
            }

            var cfgPath = Root + "/Configs";
            Directory.CreateDirectory(cfgPath);
            var modules = new[] { "Items", "Combat", "Generation", "Loot", "AI", "UI", "SharedEvents" };
            var soList = new List<ModuleConfig>();
            foreach (var m in modules)
            {
                var so = ScriptableObject.CreateInstance<ModuleConfig>();
                AssetDatabase.CreateAsset(so, string.Format("{0}/{1}ModuleConfig.asset", cfgPath, m));
                soList.Add(so);
            }
            AssetDatabase.SaveAssets();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var rootGo = new GameObject("GameCore");
            rootGo.AddComponent<GameCore>();

            // Add providers
            foreach (var m in modules)
            {
                var ns = "TJNK.Farwander.Modules." + m;
                var typeName = ns + "." + m + "ModuleProvider, Assembly-CSharp";
                var providerType = Type.GetType(typeName);
                if (providerType == null) continue;
                var go = new GameObject(m + "Module");
                go.transform.SetParent(rootGo.transform);
                var prov = go.AddComponent(providerType) as MonoBehaviour;
                var cfgField = providerType.GetField("config");
                if (cfgField != null)
                {
                    var idx = Array.IndexOf(modules, m);
                    cfgField.SetValue(prov, soList[idx]);
                }
            }

            Directory.CreateDirectory(Root + "/Scenes");
            var scenePath = Root + "/Scenes/Farwander_Default.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorUtility.DisplayDialog("Farwander", "Default scene created at\n" + scenePath, "OK");
        }
    }
}
#endif
