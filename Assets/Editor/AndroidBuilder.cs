using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuilder
{
    [MenuItem("Tools/Build Android APK")]
    public static void BuildAndroid()
    {
        // Configure Android settings
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.bombestmusic.timeoff3");
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel28;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        string[] scenes = new string[]
        {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/Menu.unity",
            "Assets/Scenes/Room_01_LiveRoom.unity",
            "Assets/Scenes/Room_02_ControlRoom.unity",
            "Assets/Scenes/Room_03_VocalBooth.unity",
            "Assets/Scenes/Room_04_EquipmentCloset.unity",
            "Assets/Scenes/Room_05_TapeMachineRoom.unity",
            "Assets/Scenes/Room_06_Lounge.unity",
            "Assets/Scenes/Room_07_EchoChamber.unity",
            "Assets/Scenes/Room_08_Rooftop.unity"
        };

        string outputPath = "Build/Android/timeoff3.apk";

        // Ensure output directory exists
        System.IO.Directory.CreateDirectory("Build/Android");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        Debug.Log("[AndroidBuilder] Starting Android build...");
        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[AndroidBuilder] Build succeeded! APK at: {outputPath} ({report.summary.totalSize / 1048576f:F1} MB)");
        }
        else
        {
            Debug.LogError($"[AndroidBuilder] Build failed: {report.summary.result}");
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error)
                        Debug.LogError($"  {msg.content}");
                }
            }
        }
    }
}
