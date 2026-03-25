using UnityEngine;

/// <summary>
/// Light that subtly pulses on beat. 5-10% intensity variation.
/// Gives the room a breathing, alive feeling synced to the music.
/// </summary>
[RequireComponent(typeof(Light))]
public class BeatReactiveLight : MonoBehaviour
{
    [SerializeField] private float pulseIntensity = 0.1f;
    [SerializeField] private float returnSpeed = 4f;

    private Light targetLight;
    private float baseIntensity;

    void Awake()
    {
        targetLight = GetComponent<Light>();
        baseIntensity = targetLight.intensity;
    }

    void OnEnable()
    {
        if (AlbumAudioManager.Instance != null)
            AlbumAudioManager.Instance.OnBeat += HandleBeat;
    }

    void OnDisable()
    {
        if (AlbumAudioManager.Instance != null)
            AlbumAudioManager.Instance.OnBeat -= HandleBeat;
    }

    void HandleBeat()
    {
        targetLight.intensity = baseIntensity * (1f + pulseIntensity);
    }

    void Update()
    {
        targetLight.intensity = Mathf.Lerp(
            targetLight.intensity,
            baseIntensity,
            Time.deltaTime * returnSpeed
        );
    }
}
