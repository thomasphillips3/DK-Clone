using UnityEngine;

[CreateAssetMenu(fileName = "TrackLevelData", menuName = "Mixtape/Track Level Data")]
public class TrackLevelData : ScriptableObject
{
    [Header("Track Info")]
    public string trackTitle = "Track Title";
    public string artistName = "Artist Name";
    public AudioClip audioClip;

    [Header("Beat Sync")]
    public float bpm = 120f;
    public float beatOffset = 0f;
    public float[] dropTimestamps = new float[0];

    [Header("Visual Theme")]
    public Color primaryColor = Color.cyan;
    public Color secondaryColor = Color.magenta;
    public Color accentColor = Color.white;
    public Texture2D albumArtTexture;
    public Texture2D levelThumbnail;
    public Sprite levelBackground;

    [Header("Gameplay")]
    public float baseSpawnInterval = 3f;
    public int maxActiveEnemies = 5;
}
