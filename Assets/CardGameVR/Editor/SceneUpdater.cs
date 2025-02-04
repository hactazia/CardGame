#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardGameVR.Editor
{
    public class SceneUpdater : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var asset in importedAssets)
                if (asset.EndsWith(".unity"))
                {
                    Debug.Log("Scene imported: " + asset);
                    var originalScenes = EditorBuildSettings.scenes.ToList();
                    if (originalScenes.Any(s => s.path == asset)) continue;
                    originalScenes.Add(new EditorBuildSettingsScene(asset, true));
                    EditorBuildSettings.scenes = originalScenes.ToArray();
                }

            foreach (var asset in deletedAssets)
                if (asset.EndsWith(".unity"))
                {
                    Debug.Log("Scene deleted: " + asset);
                    var originalScenes = EditorBuildSettings.scenes.ToList();
                    originalScenes.RemoveAll(s => s.path == asset);
                    EditorBuildSettings.scenes = originalScenes.ToArray();
                }

            foreach (var asset in movedAssets)
                if (asset.EndsWith(".unity"))
                {
                    Debug.Log("Scene moved: " + asset);
                    var originalScenes = EditorBuildSettings.scenes.ToList();
                    originalScenes.RemoveAll(s => s.path == asset);
                    originalScenes.Add(new EditorBuildSettingsScene(asset, true));
                    EditorBuildSettings.scenes = originalScenes.ToArray();
                }

            foreach (var asset in movedFromAssetPaths)
                if (asset.EndsWith(".unity"))
                {
                    Debug.Log("Scene moved from: " + asset);
                    var originalScenes = EditorBuildSettings.scenes.ToList();
                    originalScenes.RemoveAll(s => s.path == asset);
                    EditorBuildSettings.scenes = originalScenes.ToArray();
                }

            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Distinct().ToArray();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif