using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause menu with resume, return to menu, and settings.
/// Audio pauses with a low-pass ramp.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Toggle cameraBobToggle;

    private bool isPaused;

    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToMenu);
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        if (cameraBobToggle != null)
            cameraBobToggle.onValueChanged.AddListener(OnCameraBobChanged);
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        AlbumAudioManager.Instance?.Pause();
    }

    void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        AlbumAudioManager.Instance?.Resume();
    }

    void ReturnToMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        GameManager.Instance?.QuitToMenu();
    }

    void OnSensitivityChanged(float value)
    {
        var explorer = FindFirstObjectByType<StudioExplorer>();
        if (explorer != null)
            explorer.SetSensitivity(value);
    }

    void OnCameraBobChanged(bool enabled)
    {
        var explorer = FindFirstObjectByType<StudioExplorer>();
        if (explorer != null)
            explorer.SetCameraBob(enabled);
    }
}
