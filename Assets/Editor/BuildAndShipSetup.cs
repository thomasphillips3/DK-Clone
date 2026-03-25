using UnityEditor;
using UnityEngine;

/// <summary>
/// Configures project settings for shipping: product name, company, resolution, quality.
/// </summary>
public static class BuildAndShipSetup
{
    [MenuItem("Tools/Configure Build Settings")]
    public static void ConfigureBuild()
    {
        // Product identity
        PlayerSettings.companyName = "Bombest Music";
        PlayerSettings.productName = "time off 3";
        PlayerSettings.bundleVersion = "1.0.0";

        // Resolution
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.resizableWindow = true;

        // Quality
        PlayerSettings.colorSpace = ColorSpace.Linear;
        Application.targetFrameRate = 60;

        // macOS
        PlayerSettings.SetApplicationIdentifier(
            BuildTargetGroup.Standalone, "com.bombestmusic.timeoff3");

        // WebGL
        PlayerSettings.SetApplicationIdentifier(
            BuildTargetGroup.WebGL, "com.bombestmusic.timeoff3");

        // Audio
        var audioConfig = AudioSettings.GetConfiguration();
        audioConfig.sampleRate = 44100;
        audioConfig.dspBufferSize = 1024; // Low latency for beat sync
        AudioSettings.Reset(audioConfig);

        Debug.Log("[BuildSetup] Build configuration applied: time off 3 v1.0.0");
    }
}
