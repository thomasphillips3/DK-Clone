using UnityEditor;
using System.IO;

/// <summary>
/// Syncs level art from Assets/Art/Level01 to Assets/Resources/Level01.
/// Run "Tools / Sync Level Art to Resources" after editing art in Art/Level01
/// so the game (which loads from Resources) sees your updates.
/// </summary>
public static class LevelArtSync
{
    const string ArtPath = "Assets/Art/Level01";
    const string ResourcesPath = "Assets/Resources/Level01";

    [MenuItem("Tools/Sync Level Art to Resources")]
    static void SyncLevelArt()
    {
        if (!Directory.Exists(ArtPath))
        {
            UnityEngine.Debug.LogWarning($"[LevelArtSync] Source folder not found: {ArtPath}");
            return;
        }

        if (!Directory.Exists(ResourcesPath))
        {
            Directory.CreateDirectory(ResourcesPath);
        }

        string[] pngFiles = Directory.GetFiles(ArtPath, "*.png", SearchOption.TopDirectoryOnly);
        int count = 0;

        foreach (string src in pngFiles)
        {
            string filename = Path.GetFileName(src);
            string dst = Path.Combine(ResourcesPath, filename);
            File.Copy(src, dst, overwrite: true);
            count++;
        }

        AssetDatabase.Refresh();
        UnityEngine.Debug.Log($"[LevelArtSync] Synced {count} file(s) from {ArtPath} to {ResourcesPath}");
    }
}
