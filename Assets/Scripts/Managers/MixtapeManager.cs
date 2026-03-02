using UnityEngine;
using System;
using System.IO;

public class MixtapeManager : MonoBehaviour
{
    public static MixtapeManager instance { get; private set; }

    [Header("Configuration")]
    public ArtistConfig artistConfig;

    private int activeTrackIndex;
    private TrackSaveData[] saveData;

    public int TrackCount => artistConfig?.tracks?.Length ?? 9;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        saveData = new TrackSaveData[TrackCount];
        LoadAllSaveData();
        // Track 0 is always unlocked
        if (!saveData[0].isUnlocked)
        {
            saveData[0].isUnlocked = true;
            saveData[0].trackIndex = 0;
            SaveAllSaveData();
        }
    }

    public void SetActiveTrack(int index)
    {
        activeTrackIndex = Mathf.Clamp(index, 0, TrackCount - 1);
    }

    public int GetActiveTrackIndex() => activeTrackIndex;

    public TrackLevelData GetTrack(int index)
    {
        if (artistConfig == null || artistConfig.tracks == null) return null;
        if (index < 0 || index >= artistConfig.tracks.Length) return null;
        return artistConfig.tracks[index];
    }

    public TrackLevelData GetActiveTrack() => GetTrack(activeTrackIndex);

    public TrackSaveData GetSaveData(int index)
    {
        if (index < 0 || index >= saveData.Length) return default;
        return saveData[index];
    }

    public void SaveTrackProgress(float timestamp, int score, bool completed)
    {
        if (activeTrackIndex < 0 || activeTrackIndex >= saveData.Length) return;

        TrackSaveData data = saveData[activeTrackIndex];
        data.trackIndex = activeTrackIndex;
        data.lastTimestamp = timestamp;
        data.playCount++;
        data.lastPlayedUtcTicks = DateTime.UtcNow.Ticks;
        if (score > data.highScore) data.highScore = score;
        if (completed)
        {
            data.isCompleted = true;
            int next = activeTrackIndex + 1;
            if (next < saveData.Length)
            {
                saveData[next].isUnlocked = true;
                saveData[next].trackIndex = next;
            }
        }
        saveData[activeTrackIndex] = data;
        SaveAllSaveData();
    }

    public void LoadAllSaveData()
    {
        for (int i = 0; i < TrackCount; i++)
        {
            string path = Path.Combine(Application.persistentDataPath, $"mixtape_track_{i}.json");
            if (File.Exists(path))
            {
                try
                {
                    saveData[i] = JsonUtility.FromJson<TrackSaveData>(File.ReadAllText(path));
                }
                catch
                {
                    saveData[i] = new TrackSaveData { trackIndex = i };
                }
            }
            else
            {
                saveData[i] = new TrackSaveData { trackIndex = i };
            }
        }
    }

    public void SaveAllSaveData()
    {
        for (int i = 0; i < TrackCount; i++)
        {
            string path = Path.Combine(Application.persistentDataPath, $"mixtape_track_{i}.json");
            try
            {
                File.WriteAllText(path, JsonUtility.ToJson(saveData[i]));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MixtapeManager] Failed to save track {i}: {e.Message}");
            }
        }
    }
}
