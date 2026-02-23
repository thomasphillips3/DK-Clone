using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BatteryPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int scoreValue = 100;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float rotationSpeed = 50f;
    
    private Vector3 startPosition;
    private bool collected = false;
    
    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
        
        // Ensure on Collectible layer
        gameObject.layer = LayerMask.NameToLayer("Collectible");
        
        startPosition = transform.position;
    }
    
    void Update()
    {
        if (!collected)
        {
            // Bob animation
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
            
            // Rotate
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;
        
        if (other.CompareTag("Player"))
        {
            Collect(other.gameObject);
        }
    }
    
    void Collect(GameObject player)
    {
        collected = true;
        
        // Give player extra life
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.AddLife();
        }
        
        // Add score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }
        
        // Play effects
        if (pickupEffect != null)
        {
            ParticleSystem effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, effect.main.duration);
        }
        
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        // Destroy pickup
        Destroy(gameObject);
    }
}

