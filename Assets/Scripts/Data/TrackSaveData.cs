using System;

[Serializable]
public struct TrackSaveData
{
    public int trackIndex;
    public float lastTimestamp;
    public bool isCompleted;
    public bool isUnlocked;
    public int highScore;
    public int playCount;
    public long lastPlayedUtcTicks;
}
