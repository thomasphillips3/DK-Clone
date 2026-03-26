using UnityEngine;

/// <summary>
/// Always-active component that listens for pause input (Escape or tap).
/// Activates/deactivates the PauseMenu panel.
/// Put this on the InGameCanvas (which is always active).
/// </summary>
public class PauseTrigger : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool isPaused;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Toggle();

        // Tap with 3 fingers on mobile to pause (avoids conflict with look)
        if (!isPaused && Input.touchCount >= 3)
        {
            bool anyBegan = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                    anyBegan = true;
            }
            if (anyBegan) Pause();
        }
    }

    void Toggle()
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
}
