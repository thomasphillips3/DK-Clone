using UnityEngine;

[CreateAssetMenu(fileName = "AlbumConfig", menuName = "Album/Album Config")]
public class AlbumConfig : ScriptableObject
{
    public string albumTitle = "time off 3";
    public string artistName = "Bombest Music";
    public TrackData[] tracks = new TrackData[8];

    public int TrackCount => tracks != null ? tracks.Length : 0;

    public TrackData GetTrack(int index)
    {
        if (tracks == null || index < 0 || index >= tracks.Length)
            return null;
        return tracks[index];
    }

    public int GetTrackIndex(TrackData track)
    {
        if (tracks == null || track == null) return -1;
        for (int i = 0; i < tracks.Length; i++)
        {
            if (tracks[i] == track) return i;
        }
        return -1;
    }
}
