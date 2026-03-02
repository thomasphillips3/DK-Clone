using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PatchyDeath : MonoBehaviour
{
    public AudioSource source;
    public GameObject endPanel;

    bool dead;
    bool menuMode; // true = next tap goes to main menu

    void OnCollisionEnter2D(Collision2D col)
    {
        if (dead) return;
        if (!col.collider.CompareTag("Obstacle")) return;

        dead = true;

        if (source) source.Pause();
        Time.timeScale = 0f;

        ShowDeathUI();
    }

    void ShowDeathUI()
    {
        // Find or create a Canvas for the death overlay
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (!canvas)
        {
            var canvasGO = new GameObject("DeathCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Create overlay panel
        var panel = new GameObject("DeathOverlay");
        panel.transform.SetParent(canvas.transform, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.7f);

        // "GAME OVER" text
        var titleGO = new GameObject("GameOverText");
        titleGO.transform.SetParent(panel.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.6f);
        titleRT.anchorMax = new Vector2(0.5f, 0.6f);
        titleRT.sizeDelta = new Vector2(600, 100);
        var titleText = titleGO.AddComponent<Text>();
        titleText.text = "GAME OVER";
        titleText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        titleText.fontSize = 64;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;

        // Score text
        var scoreGO = new GameObject("ScoreText");
        scoreGO.transform.SetParent(panel.transform, false);
        var scoreRT = scoreGO.AddComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(0.5f, 0.45f);
        scoreRT.anchorMax = new Vector2(0.5f, 0.45f);
        scoreRT.sizeDelta = new Vector2(400, 60);
        var scoreText = scoreGO.AddComponent<Text>();
        var scoreMgr = FindAnyObjectByType<ScoreManager>();
        scoreText.text = scoreMgr ? $"SCORE: {scoreMgr.score}" : "";
        scoreText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        scoreText.fontSize = 36;
        scoreText.color = new Color(1f, 0.6f, 0.2f);
        scoreText.alignment = TextAnchor.MiddleCenter;

        // "Retry" button
        var retryGO = new GameObject("RetryBtn");
        retryGO.transform.SetParent(panel.transform, false);
        var retryRT = retryGO.AddComponent<RectTransform>();
        retryRT.anchorMin = new Vector2(0.5f, 0.32f);
        retryRT.anchorMax = new Vector2(0.5f, 0.32f);
        retryRT.sizeDelta = new Vector2(300, 50);
        var retryImg = retryGO.AddComponent<Image>();
        retryImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var retryBtn = retryGO.AddComponent<Button>();
        retryBtn.onClick.AddListener(Retry);

        var retryTextGO = new GameObject("Text");
        retryTextGO.transform.SetParent(retryGO.transform, false);
        var retryTextRT = retryTextGO.AddComponent<RectTransform>();
        retryTextRT.anchorMin = Vector2.zero;
        retryTextRT.anchorMax = Vector2.one;
        retryTextRT.sizeDelta = Vector2.zero;
        var retryText = retryTextGO.AddComponent<Text>();
        retryText.text = "RETRY";
        retryText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        retryText.fontSize = 28;
        retryText.color = Color.white;
        retryText.alignment = TextAnchor.MiddleCenter;

        // "Main Menu" button
        var menuGO = new GameObject("MainMenuBtn");
        menuGO.transform.SetParent(panel.transform, false);
        var menuRT = menuGO.AddComponent<RectTransform>();
        menuRT.anchorMin = new Vector2(0.5f, 0.2f);
        menuRT.anchorMax = new Vector2(0.5f, 0.2f);
        menuRT.sizeDelta = new Vector2(300, 50);
        var menuImg = menuGO.AddComponent<Image>();
        menuImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var menuBtn = menuGO.AddComponent<Button>();
        menuBtn.onClick.AddListener(GoToMainMenu);

        var menuTextGO = new GameObject("Text");
        menuTextGO.transform.SetParent(menuGO.transform, false);
        var menuTextRT = menuTextGO.AddComponent<RectTransform>();
        menuTextRT.anchorMin = Vector2.zero;
        menuTextRT.anchorMax = Vector2.one;
        menuTextRT.sizeDelta = Vector2.zero;
        var menuText = menuTextGO.AddComponent<Text>();
        menuText.text = "MAIN MENU";
        menuText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        menuText.fontSize = 24;
        menuText.color = new Color(1f, 0.6f, 0.2f);
        menuText.alignment = TextAnchor.MiddleCenter;

        // Activate endPanel reference too (if something else depends on it)
        if (endPanel) endPanel.SetActive(true);
    }

    void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MixtapeMap");
    }

    void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (!dead) return;

        // Space key also retries
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
            pressed = UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        pressed = Input.GetKeyDown(KeyCode.Space);
#endif
        if (pressed) Retry();
    }
}
