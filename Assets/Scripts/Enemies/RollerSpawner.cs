using UnityEngine;
using System.Collections;

public class RollerSpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject rollerPrefab;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float initialDelay = 2f;
    [SerializeField] private bool randomizeInterval = true;
    [SerializeField] private float intervalVariation = 1f;
    
    [Header("Spawn Settings")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private int maxActiveRollers = 5;
    [SerializeField] private bool spawnRight = true;
    
    [Header("Speed Variation")]
    [SerializeField] private bool randomizeSpeed = false;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float maxSpeed = 5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private ParticleSystem spawnEffect;
    [SerializeField] private AudioClip spawnSound;
    
    // State
    private bool isSpawning;
    private int activeRollerCount;
    private AudioSource audioSource;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    void Start()
    {
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }
    
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }
    
    IEnumerator SpawnRoutine()
    {
        // Initial delay
        yield return new WaitForSeconds(initialDelay);
        
        while (isSpawning)
        {
            // Check if we can spawn more
            if (rollerPrefab != null && activeRollerCount < maxActiveRollers)
            {
                SpawnRoller();
            }
            
            // Wait for next spawn
            float waitTime = spawnInterval;
            if (randomizeInterval)
            {
                waitTime += Random.Range(-intervalVariation, intervalVariation);
                waitTime = Mathf.Max(0.5f, waitTime); // Ensure minimum interval
            }
            
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    void SpawnRoller()
    {
        // Instantiate roller
        GameObject roller = Instantiate(rollerPrefab, transform.position, Quaternion.identity);
        
        // Configure roller
        ScrapRoller rollerScript = roller.GetComponent<ScrapRoller>();
        if (rollerScript != null)
        {
            rollerScript.SetDirection(spawnRight);
            
            // Randomize speed if enabled
            if (randomizeSpeed)
            {
                float speed = Random.Range(minSpeed, maxSpeed);
                rollerScript.SetSpeed(speed);
            }
        }
        
        // Track active rollers
        activeRollerCount++;
        StartCoroutine(TrackRoller(roller));
        
        // Visual/audio feedback
        if (spawnEffect != null)
        {
            spawnEffect.Play();
        }
        
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }
    
    IEnumerator TrackRoller(GameObject roller)
    {
        // Wait until roller is destroyed
        while (roller != null)
        {
            yield return null;
        }
        
        // Decrement count when destroyed
        activeRollerCount--;
    }
    
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = Mathf.Max(0.5f, interval);
    }
    
    public void SetMaxActiveRollers(int max)
    {
        maxActiveRollers = Mathf.Max(1, max);
    }
    
    public int GetActiveRollerCount()
    {
        return activeRollerCount;
    }
    
    void OnDrawGizmos()
    {
        // Draw spawn point
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        // Draw spawn direction arrow
        Vector3 direction = spawnRight ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(transform.position, direction * 1f);
    }
}

