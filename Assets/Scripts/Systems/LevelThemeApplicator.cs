using UnityEngine;

public class LevelThemeApplicator : MonoBehaviour
{
    void Start()
    {
        if (MixtapeManager.instance == null) return;
        TrackLevelData track = MixtapeManager.instance.GetActiveTrack();
        if (track == null) return;

        // Apply visual theme
        if (Camera.main != null)
            Camera.main.backgroundColor = track.primaryColor * 0.1f;

        ApplyColorToTag("Platform", track.primaryColor);
        ApplyColorToTag("Ladder", track.accentColor);

        // Load and start audio for this level
        if (AudioSyncManager.instance != null)
        {
            float resumeTime = 0f;
            int idx = MixtapeManager.instance.GetActiveTrackIndex();
            TrackSaveData save = MixtapeManager.instance.GetSaveData(idx);
            // Resume from last position if not at start
            resumeTime = (save.lastTimestamp > 1f) ? save.lastTimestamp : 0f;

            AudioSyncManager.instance.LoadTrack(track, resumeTime);
            AudioSyncManager.instance.Play();
        }
    }

    void ApplyColorToTag(string tag, Color color)
    {
        try
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_Color", color);
            foreach (var obj in objects)
            {
                Renderer r = obj.GetComponent<Renderer>();
                if (r != null) r.SetPropertyBlock(block);
            }
        }
        catch { /* tag not registered — safe to ignore */ }
    }
}
