using UnityEngine;

/// <summary>
/// Controls dust mote particle system. Beat-reactive scatter — particles briefly
/// accelerate on beat hits, then settle back to lazy floating.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class DustParticleController : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 0.1f;
    [SerializeField] private float beatBurstSpeed = 0.5f;
    [SerializeField] private float returnSpeed = 2f;

    private ParticleSystem particles;
    private ParticleSystem.VelocityOverLifetimeModule velocity;
    private float currentSpeed;

    void Awake()
    {
        particles = GetComponent<ParticleSystem>();
        velocity = particles.velocityOverLifetime;
        currentSpeed = baseSpeed;
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
        currentSpeed = beatBurstSpeed;
    }

    void Update()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, baseSpeed, Time.deltaTime * returnSpeed);
        velocity.speedModifier = currentSpeed;
    }
}
