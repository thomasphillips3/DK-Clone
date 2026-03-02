using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MixtapeMapController : MonoBehaviour
{
    [Header("Track Grid")]
    [SerializeField] private MixtapeTrackButton[] trackButtons;
    [SerializeField] private Button playButton;

    [Header("Cassette Label")]
    [SerializeField] private TMP_Text labelBrandText;
    [SerializeField] private TMP_Text labelAlbumTitle;
    [SerializeField] private TMP_Text labelArtistName;
    [SerializeField] private TMP_Text labelSideInfo;

    [Header("Detail Panel")]
    [SerializeField] private CanvasGroup detailCanvasGroup;
    [SerializeField] private RawImage waveformPreview;
    [SerializeField] private TMP_Text detailTrackNumber;
    [SerializeField] private TMP_Text detailTrackTitle;
    [SerializeField] private TMP_Text detailArtistName;
    [SerializeField] private TMP_Text detailBpmText;
    [SerializeField] private TMP_Text detailStatsText;

    [Header("Patchy")]
    [SerializeField] private PatchyCharacter patchyCharacter;

    [Header("Atmosphere")]
    [SerializeField] private MixtapeAtmosphere atmosphere;

    [Header("Data (fallback for direct scene play)")]
    [SerializeField] private ArtistConfig artistConfig;

    private int selectedIndex;

    private ArtistConfig ActiveConfig => MixtapeManager.instance?.artistConfig ?? artistConfig;

    void Start()
    {
        if (waveformPreview == null && detailCanvasGroup != null)
        {
            waveformPreview = detailCanvasGroup.GetComponentInChildren<RawImage>(true);
            if (waveformPreview == null)
            {
                GameObject go = new GameObject("WaveformPreviewRT", typeof(RectTransform), typeof(RawImage));
                go.transform.SetParent(detailCanvasGroup.transform, false);
                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0f, 70f);
                rt.sizeDelta = new Vector2(880f, 48f);
                waveformPreview = go.GetComponent<RawImage>();
                waveformPreview.color = Color.white;
            }
        }

        selectedIndex = 0;
        RefreshButtons();
        PopulateCassetteLabel();
        UpdateDetailPanel(selectedIndex);
        RefreshSelectionVisuals();

        if (playButton != null)
            playButton.onClick.AddListener(LoadSelectedTrack);
    }

    public void SelectTrack(int index)
    {
        TrackSaveData save = GetSaveData(index);
        if (!save.isUnlocked) return;

        int oldIndex = selectedIndex;
        selectedIndex = index;
        MixtapeManager.instance?.SetActiveTrack(index);
        RefreshSelectionVisuals();

        if (oldIndex != index)
        {
            StartCoroutine(AnimateDetailSwap(index));
            MixtapeUIAudio.PlayTrackSelect();

            // Shift ambient glow color toward the selected track
            ArtistConfig config = ActiveConfig;
            TrackLevelData track = (config?.tracks != null && index < config.tracks.Length)
                ? config.tracks[index] : null;

            if (patchyCharacter != null)
            {
                Color accent = track?.primaryColor ?? MixtapeColors.BurntOrange;
                patchyCharacter.OnTrackSelected(index, trackButtons?.Length ?? 8, accent);
            }

            if (atmosphere != null && track != null)
                atmosphere.OnTrackChanged(track);
        }
        else
        {
            UpdateDetailPanel(index);
        }
    }

    void RefreshButtons()
    {
        if (trackButtons == null) return;
        ArtistConfig config = ActiveConfig;
        if (config == null) return;

        for (int i = 0; i < trackButtons.Length; i++)
        {
            if (trackButtons[i] == null) continue;
            TrackLevelData track = (config.tracks != null && i < config.tracks.Length)
                ? config.tracks[i]
                : null;
            TrackSaveData save = GetSaveData(i);
            trackButtons[i].Refresh(save, track, i, this);
        }
    }

    void PopulateCassetteLabel()
    {
        ArtistConfig config = ActiveConfig;
        if (config == null) return;

        if (labelBrandText != null)
            labelBrandText.text = config.cassetteLabel.ToUpper();
        if (labelAlbumTitle != null)
            labelAlbumTitle.text = config.albumTitle;
        if (labelArtistName != null)
            labelArtistName.text = config.artistName;
        if (labelSideInfo != null)
        {
            int count = MixtapeManager.instance != null
                ? MixtapeManager.instance.TrackCount
                : (config.tracks?.Length ?? 0);
            labelSideInfo.text = $"SIDE A / {count} TRACKS";
        }
    }

    void UpdateDetailPanel(int index)
    {
        ArtistConfig config = ActiveConfig;

        TrackLevelData track = (config?.tracks != null && index < config.tracks.Length)
            ? config.tracks[index]
            : null;
        TrackSaveData save = GetSaveData(index);

        if (waveformPreview != null && track != null)
        {
            if (waveformPreview.texture != null)
                Destroy(waveformPreview.texture);
            waveformPreview.texture = WaveformScrubberRenderer.Render(track, 880, 48);
            waveformPreview.color = Color.white;
        }

        if (detailTrackNumber != null)
            detailTrackNumber.text = save.isUnlocked ? $"TRACK {index + 1:D2}" : "";
        if (detailTrackTitle != null)
            detailTrackTitle.text = save.isUnlocked ? (track?.trackTitle ?? "???") : "???";
        if (detailArtistName != null)
            detailArtistName.text = save.isUnlocked ? (track?.artistName ?? "") : "";
        if (detailBpmText != null)
            detailBpmText.text = save.isUnlocked ? $"BPM: {track?.bpm ?? 0:F0}" : "";
        if (detailStatsText != null)
        {
            // Digital counter style formatting
            detailStatsText.text = save.isUnlocked
                ? $"PLAYS: {save.playCount:D3}  |  HI-SCORE: {save.highScore:D6}"
                : "LOCKED";
        }
    }

    void RefreshSelectionVisuals()
    {
        if (trackButtons == null) return;
        for (int i = 0; i < trackButtons.Length; i++)
        {
            if (trackButtons[i] != null)
                trackButtons[i].SetSelected(i == selectedIndex);
        }
    }

    IEnumerator AnimateDetailSwap(int newIndex)
    {
        // Slightly more cinematic timing: 0.12s out, 0.2s in
        if (detailCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 0.12f)
            {
                elapsed += Time.unscaledDeltaTime;
                detailCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.12f);
                yield return null;
            }
            detailCanvasGroup.alpha = 0f;
        }

        UpdateDetailPanel(newIndex);

        if (detailCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.unscaledDeltaTime;
                detailCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.2f);
                yield return null;
            }
            detailCanvasGroup.alpha = 1f;
        }
    }

    void LoadSelectedTrack()
    {
        MixtapeUIAudio.PlayPress();
        MixtapeManager.instance?.SetActiveTrack(selectedIndex);

        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.TransitionToScene("Level01");
        else
            SceneManager.LoadScene("Level01");
    }

    private TrackSaveData GetSaveData(int index)
    {
        if (MixtapeManager.instance != null)
            return MixtapeManager.instance.GetSaveData(index);
        return new TrackSaveData { isUnlocked = true, trackIndex = index };
    }
}
