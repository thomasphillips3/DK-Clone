using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

/// <summary>
/// Builds an Android APK for Robo Rescue.
/// Run via: Unity -batchmode -executeMethod AndroidBuilder.Build -quit
/// Or via menu: Robo Rescue / Build Android APK
/// </summary>
public static class AndroidBuilder
{
    const string BUNDLE_ID  = "com.thomasphillips.roborescue";
    const string PRODUCT    = "Robo Rescue";
    const string COMPANY    = "Thomas Phillips";
    const string APK_PATH   = "Build/Android/RoboRescue.apk";

    
    [MenuItem("Robo Rescue/Setup Build Settings")]
    public static void SetupBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/MixtapeMap.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level01.unity", true),
        };
        Debug.Log("[AndroidBuilder] Build Settings updated with 3 scenes.");
    }
[MenuItem("Robo Rescue/Build Android APK")]
    public static void BuildMenu() => Build();

    public static void Build()
    {
        Debug.Log("[AndroidBuilder] Starting Android build...");

        // ── SDK / JDK paths (use Unity's bundled tools) ───────────────────
        string unityRoot = Path.GetDirectoryName(EditorApplication.applicationPath);
        string androidPlayer = Path.Combine(unityRoot, "PlaybackEngines/AndroidPlayer");

        string sdkPath  = Path.Combine(androidPlayer, "SDK");
        string ndkPath  = Path.Combine(androidPlayer, "NDK");
        string jdkPath  = Path.Combine(androidPlayer, "OpenJDK");

        if (Directory.Exists(sdkPath))
        {
            EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
            Debug.Log("[AndroidBuilder] SDK: " + sdkPath);
        }
        if (Directory.Exists(ndkPath))
        {
            EditorPrefs.SetString("AndroidNdkRootR16b", ndkPath);
            EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
            Debug.Log("[AndroidBuilder] NDK: " + ndkPath);
        }
        if (Directory.Exists(jdkPath))
        {
            EditorPrefs.SetString("JdkPath", jdkPath);
            Debug.Log("[AndroidBuilder] JDK: " + jdkPath);
        }

        // ── Player Settings ────────────────────────────────────────────────
        PlayerSettings.companyName                     = COMPANY;
        PlayerSettings.productName                     = PRODUCT;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, BUNDLE_ID);
        PlayerSettings.bundleVersion                   = "1.0";
        PlayerSettings.Android.bundleVersionCode       = 1;

        // Target API 34 (stable), min API 23 (Android 6.0)
        PlayerSettings.Android.minSdkVersion           = AndroidSdkVersions.AndroidApiLevel23;
        PlayerSettings.Android.targetSdkVersion        = AndroidSdkVersions.AndroidApiLevel34;

        // IL2CPP + ARM64 (required for Pixel 9 / modern Android)
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures     = AndroidArchitecture.ARM64;

        // Portrait + landscape (auto-rotate)
        PlayerSettings.defaultInterfaceOrientation     = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait     = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft  = true;

        // Disable splash for faster startup in dev
        PlayerSettings.SplashScreen.show               = false;

        // ── Output folder ─────────────────────────────────────────────────
        string outPath = Path.Combine(
            Path.GetDirectoryName(Application.dataPath), APK_PATH);
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));

        // ── Build ─────────────────────────────────────────────────────────
        var opts = new BuildPlayerOptions
        {
            scenes           = new[] {
                "Assets/Scenes/Boot.unity",
                "Assets/Scenes/MixtapeMap.unity",
                "Assets/Scenes/Level01.unity"
            },
            locationPathName = outPath,
            target           = BuildTarget.Android,
            targetGroup      = BuildTargetGroup.Android,
            options          = BuildOptions.None,
        };

        Debug.Log("[AndroidBuilder] Building to: " + outPath);
        BuildReport report = BuildPipeline.BuildPlayer(opts);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[AndroidBuilder] ✅ Build succeeded in {summary.totalTime.TotalSeconds:F1}s  →  {outPath}");
        }
        else
        {
            Debug.LogError($"[AndroidBuilder] ❌ Build FAILED: {summary.result}  errors={summary.totalErrors}");
#if UNITY_EDITOR
            // Print all errors when running from menu
            foreach (var step in report.steps)
                foreach (var msg in step.messages)
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        Debug.LogError($"  [{step.name}] {msg.content}");
#endif
        }
    }
}
