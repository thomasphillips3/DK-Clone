using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

/// <summary>
/// Track selection menu. Shows 8 tracks, navigation mode selector with active highlight.
/// Tapping a track starts playback immediately.
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

    private static readonly Color ActiveModeColor = new Color(0.5f, 0.35f, 0.15f, 1f);
    private static readonly Color InactiveModeColor = new Color(0.15f, 0.12f, 0.08f, 0.9f);
    private static readonly Color TrackNormal = new Color(0.12f, 0.1f, 0.06f, 0.85f);
    private static readonly Color TrackPressed = new Color(0.45f, 0.3f, 0.12f, 1f);
    private static readonly Color TrackHighlight = new Color(0.2f, 0.16f, 0.08f, 1f);

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        EnsureEventSystem();

        if (albumConfig == null && GameManager.Instance != null)
            albumConfig = GameManager.Instance.AlbumConfig;

        SetupUI();
        SetupModeButtons();
        UpdateModeButtonVisuals();

        Debug.Log($"[MenuController] Started. albumConfig={albumConfig != null}, GameManager={GameManager.Instance != null}, SceneFlow={SceneFlowManager.Instance != null}");
    }

    void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            // Make sure InputSystemUIInputModule has a valid actions asset
            var isim = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (isim != null && isim.actionsAsset == null)
            {
                Debug.LogWarning("[MenuController] InputSystemUIInputModule has no actions asset — destroying and recreating");
                Destroy(isim);
                var newIsim = EventSystem.current.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("[MenuController] Created fresh InputSystemUIInputModule");
            }
            return;
        }

        // No EventSystem exists — create one
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();
        Debug.Log("[MenuController] Created EventSystem with InputSystemUIInputModule");
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
                {
                    // Configure press feedback colors
                    var colors = button.colors;
                    colors.normalColor = TrackNormal;
                    colors.highlightedColor = TrackHighlight;
                    colors.pressedColor = TrackPressed;
                    colors.selectedColor = TrackHighlight;
                    colors.fadeDuration = 0.1f;
                    button.colors = colors;

                    button.onClick.AddListener(() => SelectAndPlay(trackIndex));
                }

                // Ensure minimum touch target height (48dp)
                var rt = btn.GetComponent<RectTransform>();
                if (rt != null && rt.sizeDelta.y < 52f)
                {
                    var size = rt.sizeDelta;
                    size.y = 52f;
                    rt.sizeDelta = size;
                }
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
        UpdateModeButtonVisuals();
    }

    void UpdateModeButtonVisuals()
    {
        SetModeButtonColor(menuModeButton, selectedMode == NavigationMode.Menu);
        SetModeButtonColor(connectedModeButton, selectedMode == NavigationMode.Connected);
        SetModeButtonColor(sequentialModeButton, selectedMode == NavigationMode.Sequential);
    }

    void SetModeButtonColor(Button btn, bool active)
    {
        if (btn == null) return;
        var colors = btn.colors;
        colors.normalColor = active ? ActiveModeColor : InactiveModeColor;
        colors.highlightedColor = active ? ActiveModeColor : new Color(0.25f, 0.2f, 0.12f, 1f);
        colors.pressedColor = new Color(0.6f, 0.4f, 0.15f, 1f);
        colors.fadeDuration = 0.15f;
        btn.colors = colors;
    }

    void SelectAndPlay(int trackIndex)
    {
        Debug.Log($"[MenuController] SelectAndPlay({trackIndex}) mode={selectedMode}");
        selectedTrack = trackIndex;
        GameManager.Instance?.SetNavigationMode(selectedMode);
        SceneFlowManager.Instance?.LoadRoom(trackIndex);
    }
}
