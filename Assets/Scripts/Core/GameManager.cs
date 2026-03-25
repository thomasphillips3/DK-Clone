using UnityEngine;

/// <summary>
/// Global application state. No score, no lives, no game over.
/// Manages navigation mode and singleton lifecycle.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private AlbumConfig albumConfig;

    public AlbumConfig AlbumConfig => albumConfig;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (albumConfig != null && AlbumAudioManager.Instance != null)
            AlbumAudioManager.Instance.SetAlbumConfig(albumConfig);
    }

    public void SetNavigationMode(NavigationMode mode)
    {
        if (AlbumPlaybackController.Instance != null)
            AlbumPlaybackController.Instance.CurrentMode = mode;
    }

    public NavigationMode GetNavigationMode()
    {
        return AlbumPlaybackController.Instance != null
            ? AlbumPlaybackController.Instance.CurrentMode
            : NavigationMode.Menu;
    }

    public void QuitToMenu()
    {
        AlbumAudioManager.Instance?.Stop();
        SceneFlowManager.Instance?.LoadMenu();
    }
}
