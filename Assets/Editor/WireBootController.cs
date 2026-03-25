using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WireBootController
{
    [MenuItem("Tools/Wire Boot Controller")]
    public static void Wire()
    {
        // Load Boot scene
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity", OpenSceneMode.Single);

        // Find BootController in scene - search all root objects
        GameObject bootGO = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "BootController")
            {
                bootGO = go;
                break;
            }
        }
        if (bootGO == null)
        {
            Debug.LogError("[WireBootController] BootController not found in scene.");
            return;
        }

        var boot = bootGO.GetComponent<BootController>();
        if (boot == null)
        {
            Debug.LogError("[WireBootController] BootController component not found.");
            return;
        }

        // Load AlbumConfig asset
        var config = AssetDatabase.LoadAssetAtPath<AlbumConfig>("Assets/Data/AlbumConfig.asset");
        if (config == null)
        {
            Debug.LogError("[WireBootController] AlbumConfig.asset not found.");
            return;
        }

        // Wire it up via SerializedObject
        var so = new SerializedObject(boot);
        var prop = so.FindProperty("albumConfig");
        prop.objectReferenceValue = config;
        so.ApplyModifiedProperties();

        // Save scene
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[WireBootController] BootController.albumConfig wired to AlbumConfig.asset and scene saved.");
    }
}
