using UnityEngine;

/// <summary>
/// Per-room component. Manages room boundaries and notifies the playback controller
/// when the player enters this room (for Connected navigation mode).
/// </summary>
public class RoomManager : MonoBehaviour
{
    [SerializeField] private int trackIndex;
    [SerializeField] private TrackData trackData;
    [SerializeField] private Transform playerSpawnPoint;

    public int TrackIndex => trackIndex;
    public TrackData TrackData => trackData;
    public Vector3 SpawnPosition => playerSpawnPoint != null
        ? playerSpawnPoint.position
        : transform.position + Vector3.up;

    void Start()
    {
        // Apply room theme colors to lighting if needed
        if (trackData != null)
            ApplyTheme();
    }

    void ApplyTheme()
    {
        // Set ambient light to match track's color scheme
        RenderSettings.ambientLight = Color.Lerp(
            trackData.secondaryColor,
            trackData.primaryColor,
            0.3f
        ) * 0.4f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        AlbumPlaybackController.Instance?.OnPlayerEnteredRoom(trackIndex);
    }
}
