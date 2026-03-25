using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Track selection menu. Shows 8 tracks, navigation mode selector, and play button.
/// </summary>
public class MenuController : MonoBehaviour
{
    [SerializeField] private AlbumConfig albumConfig;
    [SerializeField] private Transform trackListParent;
    [SerializeField] private GameObject trackButtonPrefab;
    [SerializeField] private TextMeshProUGUI albumTitleText;
    [SerializeField] private TextMeshProUGUI artistNameText;

    [Header("Navigation Mode")]
    [SerializeField] private Button menuModeButton;
    [SerializeField] private Button connectedModeButton;
    [SerializeField] private Button sequentialModeButton;

    private int selectedTrack = 0;
    private NavigationMode selectedMode = NavigationMode.Sequential;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        if (albumConfig == null && GameManager.Instance != null)
            albumConfig = GameManager.Instance.AlbumConfig;

        SetupUI();
        SetupModeButtons();
    }

    void SetupUI()
    {
        if (albumConfig == null) return;

        if (albumTitleText != null)
            albumTitleText.text = albumConfig.albumTitle;
        if (artistNameText != null)
            artistNameText.text = albumConfig.artistName;

        if (trackListParent != null && trackButtonPrefab != null)
        {
            for (int i = 0; i < albumConfig.TrackCount; i++)
            {
                TrackData track = albumConfig.GetTrack(i);
                if (track == null) continue;

                GameObject btn = Instantiate(trackButtonPrefab, trackListParent);
                var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = $"{i + 1}. {track.trackTitle}";

                int trackIndex = i;
                var button = btn.GetComponent<Button>();
                if (button != null)
                    button.onClick.AddListener(() => SelectAndPlay(trackIndex));
            }
        }
    }

    void SetupModeButtons()
    {
        if (menuModeButton != null)
            menuModeButton.onClick.AddListener(() => SetMode(NavigationMode.Menu));
        if (connectedModeButton != null)
            connectedModeButton.onClick.AddListener(() => SetMode(NavigationMode.Connected));
        if (sequentialModeButton != null)
            sequentialModeButton.onClick.AddListener(() => SetMode(NavigationMode.Sequential));
    }

    void SetMode(NavigationMode mode)
    {
        selectedMode = mode;
        GameManager.Instance?.SetNavigationMode(mode);
    }

    void SelectAndPlay(int trackIndex)
    {
        selectedTrack = trackIndex;
        GameManager.Instance?.SetNavigationMode(selectedMode);

        if (selectedMode == NavigationMode.Sequential)
            SceneFlowManager.Instance?.LoadRoom(0); // Start from track 1
        else
            SceneFlowManager.Instance?.LoadRoom(trackIndex);
    }
}
