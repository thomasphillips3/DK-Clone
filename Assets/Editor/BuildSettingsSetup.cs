using UnityEditor;
using UnityEngine;

public static class BuildSettingsSetup
{
    [MenuItem("Tools/Setup Build Settings")]
    public static void SetupBuildSettings()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Menu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Room_01_LiveRoom.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Room_02_ControlRoom.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Room_03_VocalBooth.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Room_04_EquipmentCloset.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Room_05_TapeMachineRoom.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Room_06_Lounge.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Room_07_EchoChamber.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Room_08_Rooftop.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[BuildSettingsSetup] Build settings updated with 10 scenes.");
    }
}
