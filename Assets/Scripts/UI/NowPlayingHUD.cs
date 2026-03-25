using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Minimal overlay showing current track info. Fades out after a few seconds,
/// reappears on track change.
/// </summary>
public class NowPlayingHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI trackTitleText;
    [SerializeField] private TextMeshProUGUI artistText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float showDuration = 4f;
    [SerializeField] private float fadeDuration = 1f;

    private float showTimer;
    private bool isVisible;

    void OnEnable()
    {
        if (AlbumAudioManager.Instance != null)
            AlbumAudioManager.Instance.OnTrackChanged += HandleTrackChanged;
    }

    void OnDisable()
    {
        if (AlbumAudioManager.Instance != null)
            AlbumAudioManager.Instance.OnTrackChanged -= HandleTrackChanged;
    }

    void Start()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        // Show immediately if a track is already playing
        if (AlbumAudioManager.Instance != null && AlbumAudioManager.Instance.IsPlaying)
            HandleTrackChanged(AlbumAudioManager.Instance.CurrentTrackIndex);
    }

    void HandleTrackChanged(int trackIndex)
    {
        var track = AlbumAudioManager.Instance?.CurrentTrack;
        if (track == null) return;

        if (trackTitleText != null)
            trackTitleText.text = track.trackTitle;
        if (artistText != null)
            artistText.text = track.artistName;

        showTimer = showDuration;
        isVisible = true;
    }

    void Update()
    {
        UpdateTime();
        UpdateFade();
    }

    void UpdateTime()
    {
        if (timeText == null) return;
        var audio = AlbumAudioManager.Instance;
        if (audio == null || !audio.IsPlaying) return;

        float current = audio.GetCurrentTime();
        float total = audio.GetDuration();
        timeText.text = $"{FormatTime(current)} / {FormatTime(total)}";
    }

    void UpdateFade()
    {
        if (canvasGroup == null) return;

        if (isVisible)
        {
            showTimer -= Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, Time.unscaledDeltaTime / fadeDuration);

            if (showTimer <= 0f)
                isVisible = false;
        }
        else
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, Time.unscaledDeltaTime / fadeDuration);
        }
    }

    static string FormatTime(float seconds)
    {
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);
        return $"{min}:{sec:D2}";
    }
}
