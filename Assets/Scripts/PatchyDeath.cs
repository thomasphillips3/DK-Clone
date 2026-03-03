using UnityEngine;

/// <summary>
/// Handles obstacle collisions. In neverending mode: no game over.
/// On hit: brief stumble animation, respawn to spawn point, invincibility window. Music keeps playing.
/// </summary>
public class PatchyDeath : MonoBehaviour
{
    [Header("Audio (optional - music is handled by AudioSyncManager)")]
    public AudioSource source;

    [Header("Stumble")]
    [SerializeField] private float invincibilityDuration = 0.6f;

    private float invincibilityTimer;
    private PatchyRunnerController runnerController;
    private Rigidbody2D rb;
    private Vector3 spawnPosition;

    void Awake()
    {
        runnerController = GetComponent<PatchyRunnerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        spawnPosition = transform.position;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Obstacle")) return;
        if (invincibilityTimer > 0f) return;

        invincibilityTimer = invincibilityDuration;

        if (runnerController != null)
            runnerController.Stumble();

        // Respawn to spawn point and zero velocity
        transform.position = spawnPosition;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Music and time continue - no pause, no game over
    }

    void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;
    }
}
