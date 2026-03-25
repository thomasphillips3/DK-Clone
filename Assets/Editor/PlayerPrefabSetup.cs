using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public static class PlayerPrefabSetup
{
    [MenuItem("Tools/Setup Player Prefab")]
    public static void Setup()
    {
        // Ensure we're in Room_02
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Room_02_ControlRoom")
        {
            scene = EditorSceneManager.OpenScene("Assets/Scenes/Room_02_ControlRoom.unity");
        }

        // Find Player
        GameObject playerGO = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "Player")
            {
                playerGO = go;
                break;
            }
        }

        if (playerGO == null)
        {
            Debug.LogError("[PlayerPrefabSetup] Player not found in scene.");
            return;
        }

        // Wire InputActionAsset to PlayerInput
        var playerInput = playerGO.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/Input/AlbumControls.inputactions");
            if (inputActions != null)
            {
                var so = new SerializedObject(playerInput);
                var actionsProp = so.FindProperty("m_Actions");
                actionsProp.objectReferenceValue = inputActions;

                var mapProp = so.FindProperty("m_DefaultActionMap");
                mapProp.stringValue = "Player";

                // Use SendMessage behavior for OnMove/OnLook/OnInteract/OnPause callbacks
                var behaviorProp = so.FindProperty("m_NotificationBehavior");
                behaviorProp.intValue = 1; // SendMessages

                so.ApplyModifiedProperties();
                Debug.Log("[PlayerPrefabSetup] PlayerInput wired to AlbumControls.inputactions");
            }
            else
            {
                Debug.LogError("[PlayerPrefabSetup] AlbumControls.inputactions not found.");
            }
        }

        // Save as prefab
        string prefabDir = "Assets/Prefabs/Player";
        if (!AssetDatabase.IsValidFolder(prefabDir))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Player");
        }

        string prefabPath = prefabDir + "/Player.prefab";
        bool success;
        PrefabUtility.SaveAsPrefabAssetAndConnect(playerGO, prefabPath, InteractionMode.AutomatedAction, out success);

        if (success)
        {
            Debug.Log($"[PlayerPrefabSetup] Player prefab saved to {prefabPath}");
        }
        else
        {
            Debug.LogError("[PlayerPrefabSetup] Failed to save Player prefab.");
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[PlayerPrefabSetup] Scene saved.");
    }
}
