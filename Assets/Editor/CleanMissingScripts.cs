using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CleanMissingScripts
{
    static CleanMissingScripts()
    {
        // Auto-disable Error Pause on domain reload
        DisableErrorPauseInternal();
    }

    [MenuItem("Tools/TimeOff3/Clean Missing Scripts")]
    public static void Clean()
    {
        int removedCount = 0;
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject root in rootObjects)
            removedCount += CleanObject(root);

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                int before = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab);
                if (before > 0)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
                    PrefabUtility.SavePrefabAsset(prefab);
                    removedCount += before;
                    Debug.Log($"  Cleaned {before} from prefab: {path}");
                }
            }
        }

        if (removedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log($"[CleanMissingScripts] Removed {removedCount} total.");
    }

    [MenuItem("Tools/TimeOff3/Disable Error Pause")]
    public static void DisableErrorPause()
    {
        DisableErrorPauseInternal();
    }

    static void DisableErrorPauseInternal()
    {
        try
        {
            // Use ConsoleWindow.SetConsoleErrorPause(false) — found in Unity 6
            var consoleWindowType = typeof(Editor).Assembly.GetType("UnityEditor.ConsoleWindow");
            if (consoleWindowType != null)
            {
                var method = consoleWindowType.GetMethod("SetConsoleErrorPause",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] { false });
                    Debug.Log("[ErrorPause] Disabled via ConsoleWindow.SetConsoleErrorPause(false)");
                    return;
                }

                // Fallback: try SetFlag with ConsoleFlags enum
                var setFlag = consoleWindowType.GetMethod("SetFlag",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (setFlag != null)
                {
                    var consoleFlagsType = consoleWindowType.GetNestedType("ConsoleFlags",
                        BindingFlags.NonPublic | BindingFlags.Public);
                    if (consoleFlagsType != null)
                    {
                        var errorPauseValue = Enum.Parse(consoleFlagsType, "ErrorPause");
                        setFlag.Invoke(null, new object[] { errorPauseValue, false });
                        Debug.Log("[ErrorPause] Disabled via ConsoleWindow.SetFlag(ErrorPause, false)");
                        return;
                    }
                }
            }

            Debug.LogWarning("[ErrorPause] Could not find API to disable Error Pause.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ErrorPause] Exception: {ex.Message}");
        }
    }

    static int CleanObject(GameObject go)
    {
        int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform child in go.transform)
            count += CleanObject(child.gameObject);
        return count;
    }
}
