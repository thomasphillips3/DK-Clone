using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Boot scene entry point. Ensures all singletons are initialized,
/// then transitions to the menu.
/// </summary>
public class BootController : MonoBehaviour
{
    [SerializeField] private AlbumConfig albumConfig;
    [SerializeField] private float bootDelay = 0.5f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator Start()
    {
        // Create singletons
        if (GameManager.Instance == null)
        {
            var gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            if (albumConfig != null)
                gm.SetAlbumConfig(albumConfig);
        }

        if (AlbumAudioManager.Instance == null)
        {
            var audioGO = new GameObject("AlbumAudioManager");
            var aam = audioGO.AddComponent<AlbumAudioManager>();
            if (albumConfig != null)
                aam.SetAlbumConfig(albumConfig);
        }

        if (AlbumPlaybackController.Instance == null)
        {
            var pbcGO = new GameObject("AlbumPlaybackController");
            pbcGO.AddComponent<AlbumPlaybackController>();
        }

        if (SceneFlowManager.Instance == null)
        {
            var sfGO = new GameObject("SceneFlowManager");
            sfGO.AddComponent<SceneFlowManager>();
        }

        yield return new WaitForSeconds(bootDelay);

        SceneManager.LoadScene("Menu");

        // Self-destruct after loading menu
        Destroy(gameObject);
    }
}
