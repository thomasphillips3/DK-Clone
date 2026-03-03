using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelHUDController : MonoBehaviour
{
    [Header("Now Playing")]
    [SerializeField] private TMP_Text nowPlayingPill;

    [Header("Waveform Scrubber")]
    [SerializeField] private RawImage waveformScrubber;
    [SerializeField] private RectTransform scrubberCursor;

    [Header("Lives")]
    [SerializeField] private Image[] vinylLives;

    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;

    private float totalDuration = 180f;
    private PlayerHealth playerHealth;
    private ScoreManager scoreManager;

    void Start()
    {
        TrackLevelData track = MixtapeManager.instance?.GetActiveTrack();

        if (nowPlayingPill != null && track != null)
            nowPlayingPill.text = $"♫  {track.trackTitle}  —  {track.artistName}";

        if (waveformScrubber != null && track != null)
        {
            Texture2D tex = WaveformScrubberRenderer.Render(track);
            waveformScrubber.texture = tex;
            waveformScrubber.color = Color.white;
        }

        totalDuration = AudioSyncManager.instance?.GetTotalDuration() ?? 180f;

        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null) playerHealth.OnLivesChanged += UpdateLives;

        scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
            scoreManager.OnScoreChanged += UpdateScore;
        else if (GameManager.Instance != null)
            GameManager.Instance.OnScoreChanged += UpdateScore;

        if (AudioSyncManager.instance != null)
            AudioSyncManager.instance.OnBeat += FlashNowPlaying;

        UpdateScore(scoreManager != null ? scoreManager.score : 0);
        if (vinylLives != null) UpdateLives(playerHealth != null ? 3 : 0);
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnLivesChanged -= UpdateLives;
        if (scoreManager != null) scoreManager.OnScoreChanged -= UpdateScore;
        if (GameManager.Instance != null) GameManager.Instance.OnScoreChanged -= UpdateScore;
        if (AudioSyncManager.instance != null) AudioSyncManager.instance.OnBeat -= FlashNowPlaying;
    }

    void Update()
    {
        if (scrubberCursor == null || AudioSyncManager.instance == null) return;
        float time = AudioSyncManager.instance.GetCurrentSongTime();
        float fraction = Mathf.Clamp01(time / totalDuration);
        float scrubberWidth = (waveformScrubber != null) ? waveformScrubber.rectTransform.sizeDelta.x : 512f;
        scrubberCursor.anchoredPosition = new Vector2(fraction * scrubberWidth, 0f);
    }

    void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = score.ToString("D6");
    }

    void UpdateLives(int lives)
    {
        if (vinylLives == null) return;
        for (int i = 0; i < vinylLives.Length; i++)
        {
            if (vinylLives[i] != null)
                vinylLives[i].enabled = i < lives;
        }
    }

    void FlashNowPlaying()
    {
        if (nowPlayingPill != null)
            StartCoroutine(FlashPillCoroutine());
    }

    IEnumerator FlashPillCoroutine()
    {
        if (nowPlayingPill == null) yield break;
        Color original = nowPlayingPill.color;
        nowPlayingPill.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        nowPlayingPill.color = original;
    }
}
