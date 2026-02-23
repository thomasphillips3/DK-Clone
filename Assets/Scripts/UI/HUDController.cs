using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image[] lifeIcons;
    
    [Header("Power-Up Indicator")]
    [SerializeField] private GameObject powerUpIndicator;
    [SerializeField] private Image powerUpFillImage;
    
    [Header("Format Strings")]
    [SerializeField] private string scoreFormat = "SCORE: {0:D6}";
    [SerializeField] private string livesFormat = "x {0}";
    [SerializeField] private string timerFormat = "{0:0}";
    
    private PlayerController player;
    private PlayerHealth playerHealth;
    
    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<PlayerController>();
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            
            // Subscribe to health events
            if (playerHealth != null)
            {
                playerHealth.OnLivesChanged += UpdateLives;
            }
        }
        
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnTimeChanged += UpdateTimer;
        }
        
        // Initialize displays
        UpdateScore(0);
        UpdateLives(3);
        UpdateTimer(180f);
        
        if (powerUpIndicator != null)
        {
            powerUpIndicator.SetActive(false);
        }
    }
    
    void Update()
    {
        // Update power-up indicator
        if (player != null && powerUpIndicator != null)
        {
            bool hasPowerUp = player.HasPowerUp();
            
            if (powerUpIndicator.activeSelf != hasPowerUp)
            {
                powerUpIndicator.SetActive(hasPowerUp);
            }
        }
    }
    
    void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, score);
        }
    }
    
    void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = string.Format(livesFormat, lives);
        }
        
        // Update life icons
        if (lifeIcons != null && lifeIcons.Length > 0)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null)
                {
                    lifeIcons[i].enabled = i < lives;
                }
            }
        }
    }
    
    void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            timerText.text = string.Format(timerFormat, timeRemaining);
            
            // Change color when time is low
            if (timeRemaining < 30f)
            {
                timerText.color = Color.red;
            }
            else if (timeRemaining < 60f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged -= UpdateLives;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnTimeChanged -= UpdateTimer;
        }
    }
}

