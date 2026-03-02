using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioSyncManager : MonoBehaviour
{
    public static AudioSyncManager instance { get; private set; }

    public event Action OnBeat;
    public event Action OnDrop;

    private AudioSource audioSrc;
    private AudioLowPassFilter lowPass;
    private TrackLevelData currentTrack;

    private double dspStartTime;
    private float scheduledStartOffset;
    private float[] sortedBeatTimes;
    private float totalDuration;
    private int lastBeatIndex = -1;
    private int lastDropIndex = -1;
    private bool isPlaying;

    public bool IsPlaying => isPlaying;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSrc = GetComponent<AudioSource>();
        lowPass = GetComponent<AudioLowPassFilter>();
        if (lowPass == null) lowPass = gameObject.AddComponent<AudioLowPassFilter>();
        lowPass.cutoffFrequency = 22000f;
        lowPass.enabled = false;
    }

    public void LoadTrack(TrackLevelData track, float startOffset = 0f)
    {
        currentTrack = track;
        scheduledStartOffset = Mathf.Max(0f, startOffset);
        audioSrc.clip = track?.audioClip;
        totalDuration = (track?.audioClip != null) ? track.audioClip.length : 1f;

        if (track != null && track.bpm > 0f)
        {
            float beatInterval = 60f / track.bpm;
            float firstBeat = track.beatOffset;
            int beatCount = Mathf.CeilToInt((totalDuration - firstBeat) / beatInterval) + 2;
            sortedBeatTimes = new float[beatCount];
            for (int i = 0; i < beatCount; i++)
                sortedBeatTimes[i] = firstBeat + i * beatInterval;
        }
        else
        {
            sortedBeatTimes = new float[0];
        }

        lastBeatIndex = -1;
        lastDropIndex = -1;
        isPlaying = false;
    }

    public void Play()
    {
        if (audioSrc.clip == null) { isPlaying = false; return; }
        StopAllCoroutines();
        lowPass.enabled = false;
        lowPass.cutoffFrequency = 22000f;

        audioSrc.Stop();
        audioSrc.time = Mathf.Clamp(scheduledStartOffset, 0f, totalDuration - 0.01f);

        double scheduledDspTime = AudioSettings.dspTime + 0.1;
        audioSrc.PlayScheduled(scheduledDspTime);
        // dspStartTime set so that GetCurrentSongTime() = scheduledStartOffset when DSP = scheduledDspTime
        dspStartTime = scheduledDspTime;
        isPlaying = true;

        lastBeatIndex = FindBeatIndexBefore(scheduledStartOffset);
        lastDropIndex = FindDropIndexBefore(scheduledStartOffset);
    }

    public void Pause()
    {
        if (!isPlaying) return;
        isPlaying = false;
        StartCoroutine(LowPassRampAndPause());
    }

    public void Resume()
    {
        if (isPlaying || audioSrc.clip == null) return;
        scheduledStartOffset = GetCurrentSongTime();
        Play();
    }

    public void RewindOnDeath()
    {
        float currentTime = GetCurrentSongTime();
        scheduledStartOffset = Mathf.Max(0f, currentTime - 5f);
        audioSrc.Stop();
        isPlaying = false;
        Play();
    }

    public float GetCurrentSongTime()
    {
        if (!isPlaying) return scheduledStartOffset;
        double elapsed = AudioSettings.dspTime - dspStartTime;
        if (elapsed < 0) return scheduledStartOffset;
        return scheduledStartOffset + (float)elapsed;
    }

    public float GetTotalDuration() => totalDuration;

    public float GetBeatProgress()
    {
        if (currentTrack == null || currentTrack.bpm <= 0f) return 0f;
        float beatInterval = 60f / currentTrack.bpm;
        float time = GetCurrentSongTime();
        return (time % beatInterval) / beatInterval;
    }

    public bool IsBeatNow(float tolerance = 0.05f)
    {
        float p = GetBeatProgress();
        return p < tolerance || p > (1f - tolerance);
    }

    void Update()
    {
        if (!isPlaying || sortedBeatTimes == null) return;

        float currentTime = GetCurrentSongTime();

        for (int i = lastBeatIndex + 1; i < sortedBeatTimes.Length; i++)
        {
            if (sortedBeatTimes[i] <= currentTime)
            {
                lastBeatIndex = i;
                OnBeat?.Invoke();
            }
            else break;
        }

        if (currentTrack?.dropTimestamps != null)
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

        if (currentTime >= totalDuration)
            isPlaying = false;
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
        audioSrc.Pause();
    }

    int FindBeatIndexBefore(float time)
    {
        if (sortedBeatTimes == null || sortedBeatTimes.Length == 0) return -1;
        int idx = -1;
        for (int i = 0; i < sortedBeatTimes.Length; i++)
        {
            if (sortedBeatTimes[i] < time) idx = i;
            else break;
        }
        return idx;
    }

    int FindDropIndexBefore(float time)
    {
        if (currentTrack?.dropTimestamps == null) return -1;
        int idx = -1;
        for (int i = 0; i < currentTrack.dropTimestamps.Length; i++)
        {
            if (currentTrack.dropTimestamps[i] < time) idx = i;
            else break;
        }
        return idx;
    }
}
