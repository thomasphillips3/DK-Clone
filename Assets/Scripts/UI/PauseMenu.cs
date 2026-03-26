using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause menu panel. Lives on the pause panel GameObject.
/// Activated/deactivated by PauseTrigger.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button backToMenuButton;

    void OnEnable()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(ReturnToMenu);
    }

    void OnDisable()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(Resume);
        if (backToMenuButton != null)
            backToMenuButton.onClick.RemoveListener(ReturnToMenu);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameObject.SetActive(false);
        AlbumAudioManager.Instance?.Resume();
    }

    void ReturnToMenu()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        GameManager.Instance?.QuitToMenu();
    }
}
