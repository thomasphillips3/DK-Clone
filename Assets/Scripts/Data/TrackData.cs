using UnityEngine;

[CreateAssetMenu(fileName = "TrackData", menuName = "Album/Track Data")]
public class TrackData : ScriptableObject
{
    [Header("Track Info")]
    public string trackTitle = "Track Title";
    public string artistName = "Bombest Music";
    public AudioClip audioClip;

    [Header("Beat Sync")]
    public float bpm = 90f;
    public float beatOffset = 0f;
    public float[] dropTimestamps = new float[0];

    [Header("Room")]
    public RoomType roomType;
    public string sceneName;
    public int roomSeed = 42;

    [Header("Visual Theme")]
    public Color primaryColor = new Color(0.8f, 0.6f, 0.3f);
    public Color secondaryColor = new Color(0.3f, 0.25f, 0.2f);
    public Color accentColor = new Color(1f, 0.85f, 0.5f);

    [Header("Album Art")]
    public Texture2D albumArtTexture;

    public float Duration => audioClip != null ? audioClip.length : 0f;
}
