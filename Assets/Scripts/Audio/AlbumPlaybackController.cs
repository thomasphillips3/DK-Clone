using UnityEngine;

/// <summary>
/// Coordinates audio playback with the active navigation mode.
/// Sequential: auto-advances tracks. Connected: crossfades by room position.
/// Menu: plays the selected track.
/// </summary>
public class AlbumPlaybackController : MonoBehaviour
{
    public static AlbumPlaybackController Instance { get; private set; }

    private NavigationMode currentMode = NavigationMode.Menu;
    private int currentRoomIndex = -1;

    public NavigationMode CurrentMode
    {
        get => currentMode;
        set => currentMode = value;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        if (AlbumAudioManager.Instance != null)
            AlbumAudioManager.Instance.OnTrackEnded += HandleTrackEnded;
    }

    void OnDisable()
    {
        if (AlbumAudioManager.Instance != null)
            AlbumAudioManager.Instance.OnTrackEnded -= HandleTrackEnded;
    }

    void Start()
    {
        if (AlbumAudioManager.Instance != null)
            AlbumAudioManager.Instance.OnTrackEnded += HandleTrackEnded;
    }

    public void PlayTrackForRoom(int trackIndex)
    {
        currentRoomIndex = trackIndex;
        var audio = AlbumAudioManager.Instance;
        if (audio == null) return;

        if (audio.CurrentTrackIndex == trackIndex && audio.IsPlaying)
            return;

        if (audio.IsPlaying)
            audio.CrossfadeTo(trackIndex, 2f);
        else
            audio.PlayTrack(trackIndex);
    }

    public void OnPlayerEnteredRoom(int roomIndex)
    {
        if (currentMode != NavigationMode.Connected) return;
        if (roomIndex == currentRoomIndex) return;

        currentRoomIndex = roomIndex;
        var audio = AlbumAudioManager.Instance;
        if (audio == null) return;

        audio.CrossfadeTo(roomIndex, 3f);
    }

    void HandleTrackEnded(int trackIndex)
    {
        if (currentMode != NavigationMode.Sequential) return;

        var config = AlbumAudioManager.Instance?.Config;
        if (config == null) return;

        int nextIndex = trackIndex + 1;
        if (nextIndex >= config.TrackCount)
        {
            // Album finished — return to menu
            SceneFlowManager.Instance?.LoadMenu();
            return;
        }

        // Load next room and play next track
        SceneFlowManager.Instance?.LoadRoom(nextIndex);
    }
}
