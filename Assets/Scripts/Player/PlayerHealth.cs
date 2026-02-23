using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private float respawnDelay = 1f;
    
    [Header("Visual Feedback")]
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private Color damageFlashColor = Color.red;
    
    // Components
    private PlayerController controller;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    
    // State
    private int currentLives;
    private bool isInvincible;
    private bool isDead;
    
    // Events
    public System.Action<int> OnLivesChanged;
    public System.Action OnDeath;
    public System.Action OnRespawn;
    
    void Awake()
    {
        controller = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        
        currentLives = maxLives;
    }
    
    void Start()
    {
        OnLivesChanged?.Invoke(currentLives);
    }
    
    public void TakeDamage(int damage = 1)
    {
        // Don't take damage if invincible, dead, or has power-up
        if (isInvincible || isDead || controller.HasPowerUp())
            return;
        
        currentLives -= damage;
        OnLivesChanged?.Invoke(currentLives);
        
        if (currentLives <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }
    
    void Die()
    {
        if (isDead)
            return;
        
        isDead = true;
        OnDeath?.Invoke();
        
        // Disable player controls and collision
        enabled = false;
        controller.enabled = false;
        col.enabled = false;
        
        // Play death animation if available
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }
        
        StartCoroutine(RespawnCoroutine());
    }
    
    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        
        if (currentLives <= 0)
        {
            // Game Over - let GameManager handle this
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
        else
        {
            Respawn();
        }
    }
    
    void Respawn()
    {
        isDead = false;
        
        // Re-enable components
        enabled = true;
        controller.enabled = true;
        col.enabled = true;
        
        // Reset position
        controller.Respawn(controller.GetSpawnPosition());
        
        // Start invincibility
        StartCoroutine(InvincibilityCoroutine());
        
        OnRespawn?.Invoke();
    }
    
    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;
        
        // Blink effect
        while (elapsed < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        
        spriteRenderer.enabled = true;
        spriteRenderer.color = originalColor;
        isInvincible = false;
    }
    
    public void Heal(int amount = 1)
    {
        currentLives = Mathf.Min(currentLives + amount, maxLives);
        OnLivesChanged?.Invoke(currentLives);
    }
    
    public void AddLife()
    {
        currentLives++;
        OnLivesChanged?.Invoke(currentLives);
    }
    
    public void InstantKill()
    {
        currentLives = 0;
        Die();
    }
    
    // Called when player falls into kill zone
    public void FallDeath()
    {
        TakeDamage(1);
    }
    
    public int GetCurrentLives()
    {
        return currentLives;
    }
    
    public int GetMaxLives()
    {
        return maxLives;
    }
    
    public bool IsInvincible()
    {
        return isInvincible;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if hit by enemy
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // If player has power-up, destroy the enemy instead
            if (controller.HasPowerUp())
            {
                Destroy(collision.gameObject);
                
                // Add score
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(100);
                }
            }
            else
            {
                TakeDamage(1);
            }
        }
        // Check if hit hazard
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Hazard"))
        {
            TakeDamage(1);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Handle hazard triggers
        if (other.gameObject.layer == LayerMask.NameToLayer("Hazard"))
        {
            TakeDamage(1);
        }
    }
}

