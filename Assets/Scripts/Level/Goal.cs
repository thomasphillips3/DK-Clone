using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Goal : MonoBehaviour
{
    [Header("Level Completion")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private bool loadNextScene = true;
    [SerializeField] private float completionDelay = 2f;
    
    [Header("Rewards")]
    [SerializeField] private int completionScore = 1000;
    [SerializeField] private int timeBonus = 500;
    
    [Header("Visual Feedback")]
    [SerializeField] private ParticleSystem completionEffect;
    [SerializeField] private AudioClip completionSound;
    [SerializeField] private Animator friendBotAnimator;
    
    private bool levelCompleted = false;
    private AudioSource audioSource;
    
    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
        
        audioSource = GetComponent<AudioSource>();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!levelCompleted && other.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }
    
    void CompleteLevel()
    {
        if (levelCompleted)
            return;
        
        levelCompleted = true;
        
        // Disable player control
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = false;
        }
        
        // Play animations and effects
        if (friendBotAnimator != null)
        {
            friendBotAnimator.SetTrigger("Rescued");
        }
        
        if (completionEffect != null)
        {
            completionEffect.Play();
        }
        
        if (completionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(completionSound);
        }
        
        // Award score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(completionScore);
            // Add time bonus based on remaining time
            GameManager.Instance.AddScore(timeBonus);
        }
        
        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelComplete();
        }
        
        // Load next scene after delay
        if (loadNextScene)
        {
            StartCoroutine(LoadNextSceneCoroutine());
        }
    }
    
    IEnumerator LoadNextSceneCoroutine()
    {
        yield return new WaitForSeconds(completionDelay);
        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // If no next scene specified, reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            
            if (col is BoxCollider2D boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.offset, boxCol.size);
            }
            else if (col is CircleCollider2D circleCol)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circleCol.offset, circleCol.radius);
            }
        }
    }
}

