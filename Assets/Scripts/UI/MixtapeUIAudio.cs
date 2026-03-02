using UnityEngine;

/// <summary>
/// Procedurally generates and plays short tactile UI sound effects.
/// Self-bootstrapping singleton — creates its own AudioSource.
/// Sounds: analog click, tape head scrape, mechanical thunk.
/// </summary>
public class MixtapeUIAudio : MonoBehaviour
{
    public static MixtapeUIAudio instance { get; private set; }

    private AudioSource audioSrc;
    private AudioClip clickClip;   // ~20ms analog click
    private AudioClip selectClip;  // ~80ms tape head scrape
    private AudioClip pressClip;   // ~120ms mechanical thunk

    private const int SampleRate = 44100;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(this); return; }
        instance = this;

        audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        audioSrc.volume = 0.15f;
        audioSrc.spatialBlend = 0f; // 2D

        GenerateClips();
    }

    void GenerateClips()
    {
        clickClip = GenerateClick();
        selectClip = GenerateSelect();
        pressClip = GeneratePress();
    }

    /// <summary>Short white noise burst with exponential decay — analog click.</summary>
    AudioClip GenerateClick()
    {
        int samples = SampleRate * 20 / 1000; // 20ms
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-t * 12f); // fast decay
            float noise = Random.Range(-1f, 1f);
            // Simple bandpass by mixing two noise octaves
            data[i] = noise * envelope * 0.6f;
        }
        // Apply simple low-pass smoothing
        for (int i = 1; i < samples; i++)
            data[i] = data[i] * 0.4f + data[i - 1] * 0.6f;

        AudioClip clip = AudioClip.Create("UIClick", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Filtered noise + sine sweep 800→200Hz — tape head scrape.</summary>
    AudioClip GenerateSelect()
    {
        int samples = SampleRate * 80 / 1000; // 80ms
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-t * 6f) * (1f - t);
            // Sine sweep from 800Hz down to 200Hz
            float freq = Mathf.Lerp(800f, 200f, t);
            float phase = 2f * Mathf.PI * freq * i / SampleRate;
            float sine = Mathf.Sin(phase) * 0.4f;
            // Add filtered noise
            float noise = Random.Range(-1f, 1f) * 0.3f;
            data[i] = (sine + noise) * envelope;
        }
        // Smooth
        for (int i = 1; i < samples; i++)
            data[i] = data[i] * 0.5f + data[i - 1] * 0.5f;

        AudioClip clip = AudioClip.Create("UISelect", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>80Hz sine with quick decay + noise transient — mechanical thunk.</summary>
    AudioClip GeneratePress()
    {
        int samples = SampleRate * 120 / 1000; // 120ms
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            // Initial transient click (first 5ms)
            float transient = (i < SampleRate * 5 / 1000)
                ? Random.Range(-1f, 1f) * Mathf.Exp(-t * 40f) * 0.5f
                : 0f;
            // Low thump: 80Hz sine with fast decay
            float thump = Mathf.Sin(2f * Mathf.PI * 80f * i / SampleRate) * Mathf.Exp(-t * 8f) * 0.7f;
            data[i] = thump + transient;
        }

        AudioClip clip = AudioClip.Create("UIPress", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static void PlayClick()
    {
        if (instance != null && instance.clickClip != null)
            instance.audioSrc.PlayOneShot(instance.clickClip);
    }

    public static void PlayTrackSelect()
    {
        if (instance != null && instance.selectClip != null)
            instance.audioSrc.PlayOneShot(instance.selectClip);
    }

    public static void PlayPress()
    {
        if (instance != null && instance.pressClip != null)
            instance.audioSrc.PlayOneShot(instance.pressClip);
    }
}
