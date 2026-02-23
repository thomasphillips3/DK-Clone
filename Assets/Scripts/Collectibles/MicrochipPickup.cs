using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MicrochipPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int scoreValue = 200;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseScale = 0.2f;
    
    private Vector3 startPosition;
    private Vector3 originalScale;
    private bool collected = false;
    
    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
        
        // Ensure on Collectible layer
        gameObject.layer = LayerMask.NameToLayer("Collectible");
        
        startPosition = transform.position;
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        if (!collected)
        {
            // Bob animation
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
            
            // Pulse scale
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = originalScale * scale;
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
        
        // Add bonus score
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

