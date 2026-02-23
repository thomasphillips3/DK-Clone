using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private int startingScore = 0;
    
    [Header("Timer")]
    [SerializeField] private float levelTimeLimit = 180f; // 3 minutes
    [SerializeField] private bool useTimer = true;
    
    [Header("Scene Management")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    // Game state
    private int currentScore;
    private int currentLives;
    private float currentTime;
    private bool gameActive;
    private bool levelComplete;
    
    // Events
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnLivesChanged;
    public System.Action<float> OnTimeChanged;
    public System.Action OnGameStart;
    public System.Action OnGameOver;
    public System.Action OnLevelCompleted;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeGame();
    }
    
    void Update()
    {
        if (gameActive && !levelComplete)
        {
            UpdateTimer();
        }
    }
    
    void InitializeGame()
    {
        currentScore = startingScore;
        currentLives = startingLives;
        currentTime = levelTimeLimit;
        gameActive = false;
        levelComplete = false;
    }
    
    public void StartGame()
    {
        gameActive = true;
        currentTime = levelTimeLimit;
        OnGameStart?.Invoke();
    }
    
    void UpdateTimer()
    {
        if (!useTimer)
            return;
        
        currentTime -= Time.deltaTime;
        OnTimeChanged?.Invoke(currentTime);
        
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            TimeOut();
        }
    }
    
    void TimeOut()
    {
        // Player loses a life when time runs out
        PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(1);
        }
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    public void OnPlayerDeath()
    {
        // Called by PlayerHealth when player dies
        // Lives are managed by PlayerHealth
    }
    
    public void GameOver()
    {
        if (!gameActive)
            return;
        
        gameActive = false;
        OnGameOver?.Invoke();
        
        // Optional: Load game over scene
        // SceneManager.LoadScene(gameOverSceneName);
    }
    
    public void OnLevelComplete()
    {
        if (levelComplete)
            return;
        
        levelComplete = true;
        gameActive = false;
        OnLevelCompleted?.Invoke();
    }
    
    public void RestartLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        
        // Reset state
        currentTime = levelTimeLimit;
        levelComplete = false;
        gameActive = true;
    }
    
    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        
        // Reset for new level
        currentTime = levelTimeLimit;
        levelComplete = false;
        gameActive = true;
    }
    
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
        InitializeGame();
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    // Getters
    public int GetScore() => currentScore;
    public int GetLives() => currentLives;
    public float GetTime() => currentTime;
    public bool IsGameActive() => gameActive;
    public bool IsLevelComplete() => levelComplete;
    
    // Pause functionality
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}

