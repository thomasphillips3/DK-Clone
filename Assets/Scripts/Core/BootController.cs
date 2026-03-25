using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Boot scene entry point. Ensures all singletons are initialized,
/// then transitions to the menu.
/// </summary>
public class BootController : MonoBehaviour
{
    [SerializeField] private AlbumConfig albumConfig;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        InitializeSingletons();
        SceneManager.LoadScene("Menu");
    }

    void InitializeSingletons()
    {
        if (GameManager.Instance == null)
        {
            var go = new GameObject("GameManager");
            var gm = go.AddComponent<GameManager>();
            if (albumConfig != null)
                gm.SetAlbumConfig(albumConfig);
        }

        if (AlbumAudioManager.Instance == null)
        {
            var go = new GameObject("AlbumAudioManager");
            var aam = go.AddComponent<AlbumAudioManager>();
            if (albumConfig != null)
                aam.SetAlbumConfig(albumConfig);
        }

        if (AlbumPlaybackController.Instance == null)
        {
            var go = new GameObject("AlbumPlaybackController");
            go.AddComponent<AlbumPlaybackController>();
        }

        if (SceneFlowManager.Instance == null)
        {
            var go = new GameObject("SceneFlowManager");
            go.AddComponent<SceneFlowManager>();
        }
    }

    void Update()
    {
        // Self-destruct once we're past Boot scene
        if (SceneManager.GetActiveScene().name != "Boot")
            Destroy(gameObject);
    }
}
