using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WrenchPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int scoreValue = 50;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobHeight = 0.2f;
    
    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool collected = false;
    
    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
        
        // Ensure on Collectible layer
        gameObject.layer = LayerMask.NameToLayer("Collectible");
        
        audioSource = GetComponent<AudioSource>();
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Bob animation
        if (!collected)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
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
        
        // Activate wrench power-up on player
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ActivateWrench();
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
            // Play sound at position (survives object destruction)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        // Destroy pickup
        Destroy(gameObject);
    }
}

