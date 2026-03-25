using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Singleton audio manager for the interactive album. Two AudioSources for seamless
/// A/B crossfading. Beat detection via DSP time. Zero sound effects — only music.
/// </summary>
public class AlbumAudioManager : MonoBehaviour
{
    public static AlbumAudioManager Instance { get; private set; }

    public event Action OnBeat;
    public event Action OnDrop;
    public event Action<int> OnTrackChanged;
    public event Action<int> OnTrackEnded;

    [SerializeField] private AlbumConfig albumConfig;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource activeSource;
    private AudioLowPassFilter lowPass;

    private TrackData currentTrack;
    private int currentTrackIndex = -1;
    private double dspStartTime;
    private float startOffset;
    private float[] beatTimes;
    private int lastBeatIndex = -1;
    private int lastDropIndex = -1;
    private bool isPlaying;
    private Coroutine crossfadeCoroutine;

    public bool IsPlaying => isPlaying;
    public int CurrentTrackIndex => currentTrackIndex;
    public TrackData CurrentTrack => currentTrack;
    public AlbumConfig Config => albumConfig;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();
        sourceA.playOnAwake = false;
        sourceB.playOnAwake = false;
        sourceA.loop = false;
        sourceB.loop = false;
        activeSource = sourceA;

        lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        lowPass.cutoffFrequency = 22000f;
        lowPass.enabled = false;
    }

    public void SetAlbumConfig(AlbumConfig config)
    {
        albumConfig = config;
    }

    public void PlayTrack(int index)
    {
        if (albumConfig == null) return;
        TrackData track = albumConfig.GetTrack(index);
        if (track == null || track.audioClip == null) return;

        StopAllCoroutines();
        crossfadeCoroutine = null;

        currentTrack = track;
        currentTrackIndex = index;
        startOffset = 0f;

        BuildBeatArray(track);

        activeSource.Stop();
        GetInactiveSource().Stop();

        activeSource.clip = track.audioClip;
        activeSource.volume = 1f;
        activeSource.time = 0f;

        lowPass.enabled = false;
        lowPass.cutoffFrequency = 22000f;

        double scheduledTime = AudioSettings.dspTime + 0.1;
        activeSource.PlayScheduled(scheduledTime);
        dspStartTime = scheduledTime;
        isPlaying = true;

        lastBeatIndex = -1;
        lastDropIndex = -1;

        OnTrackChanged?.Invoke(index);
    }

    public void CrossfadeTo(int index, float duration = 2f)
    {
        if (albumConfig == null) return;
        TrackData track = albumConfig.GetTrack(index);
        if (track == null || track.audioClip == null) return;
        if (index == currentTrackIndex && isPlaying) return;

        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        crossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(index, track, duration));
    }

    IEnumerator CrossfadeCoroutine(int index, TrackData track, float duration)
    {
        AudioSource fadeOut = activeSource;
        AudioSource fadeIn = GetInactiveSource();

        fadeIn.clip = track.audioClip;
        fadeIn.volume = 0f;
        fadeIn.time = 0f;
        fadeIn.Play();

        activeSource = fadeIn;
        currentTrack = track;
        currentTrackIndex = index;
        startOffset = 0f;
        dspStartTime = AudioSettings.dspTime;
        isPlaying = true;

        BuildBeatArray(track);
        lastBeatIndex = -1;
        lastDropIndex = -1;

        OnTrackChanged?.Invoke(index);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            fadeIn.volume = t;
            fadeOut.volume = 1f - t;
            yield return null;
        }

        fadeIn.volume = 1f;
        fadeOut.Stop();
        fadeOut.clip = null;
        crossfadeCoroutine = null;
    }

    public void Pause()
    {
        if (!isPlaying) return;
        isPlaying = false;
        StartCoroutine(LowPassRampAndPause());
    }

    public void Resume()
    {
        if (isPlaying || activeSource.clip == null) return;
        lowPass.enabled = false;
        lowPass.cutoffFrequency = 22000f;
        startOffset = GetCurrentTime();
        activeSource.time = Mathf.Clamp(startOffset, 0f, activeSource.clip.length - 0.01f);

        double scheduledTime = AudioSettings.dspTime + 0.05;
        activeSource.PlayScheduled(scheduledTime);
        dspStartTime = scheduledTime;
        isPlaying = true;

        lastBeatIndex = FindIndexBefore(beatTimes, startOffset);
        lastDropIndex = FindIndexBefore(currentTrack?.dropTimestamps, startOffset);
    }

    public void Stop()
    {
        StopAllCoroutines();
        crossfadeCoroutine = null;
        sourceA.Stop();
        sourceB.Stop();
        isPlaying = false;
        currentTrackIndex = -1;
        currentTrack = null;
        lowPass.enabled = false;
    }

    public float GetCurrentTime()
    {
        if (!isPlaying) return startOffset;
        double elapsed = AudioSettings.dspTime - dspStartTime;
        if (elapsed < 0) return startOffset;
        return startOffset + (float)elapsed;
    }

    public float GetDuration()
    {
        return currentTrack != null ? currentTrack.Duration : 0f;
    }

    public float GetBeatProgress()
    {
        if (currentTrack == null || currentTrack.bpm <= 0f) return 0f;
        float beatInterval = 60f / currentTrack.bpm;
        float time = GetCurrentTime();
        return (time % beatInterval) / beatInterval;
    }

    void Update()
    {
        if (!isPlaying || currentTrack == null) return;

        float currentTime = GetCurrentTime();

        if (beatTimes != null)
        {
            for (int i = lastBeatIndex + 1; i < beatTimes.Length; i++)
            {
                if (beatTimes[i] <= currentTime)
                {
                    lastBeatIndex = i;
                    OnBeat?.Invoke();
                }
                else break;
            }
        }

        if (currentTrack.dropTimestamps != null)
        {
            for (int i = lastDropIndex + 1; i < currentTrack.dropTimestamps.Length; i++)
            {
                if (currentTrack.dropTimestamps[i] <= currentTime)
                {
                    lastDropIndex = i;
                    OnDrop?.Invoke();
                }
                else break;
            }
        }

        if (currentTime >= GetDuration())
        {
            isPlaying = false;
            OnTrackEnded?.Invoke(currentTrackIndex);
        }
    }

    void BuildBeatArray(TrackData track)
    {
        if (track == null || track.bpm <= 0f)
        {
            beatTimes = new float[0];
            return;
        }
        float beatInterval = 60f / track.bpm;
        float firstBeat = track.beatOffset;
        float duration = track.Duration;
        int count = Mathf.CeilToInt((duration - firstBeat) / beatInterval) + 2;
        beatTimes = new float[count];
        for (int i = 0; i < count; i++)
            beatTimes[i] = firstBeat + i * beatInterval;
    }

    int FindIndexBefore(float[] times, float time)
    {
        if (times == null || times.Length == 0) return -1;
        int idx = -1;
        for (int i = 0; i < times.Length; i++)
        {
            if (times[i] < time) idx = i;
            else break;
        }
        return idx;
    }

    IEnumerator LowPassRampAndPause()
    {
        lowPass.enabled = true;
        float elapsed = 0f;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            lowPass.cutoffFrequency = Mathf.Lerp(22000f, 800f, elapsed / duration);
            yield return null;
        }
        activeSource.Pause();
    }

    AudioSource GetInactiveSource()
    {
        return activeSource == sourceA ? sourceB : sourceA;
    }
}
