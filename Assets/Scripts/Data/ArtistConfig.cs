using UnityEngine;

[CreateAssetMenu(fileName = "ArtistConfig", menuName = "Mixtape/Artist Config")]
public class ArtistConfig : ScriptableObject
{
    public string artistName = "Artist Name";
    public string albumTitle = "Album Title";
    public string cassetteLabel = "Label";
    public Color brandColor = Color.cyan;
    public TrackLevelData[] tracks = new TrackLevelData[8];
}
