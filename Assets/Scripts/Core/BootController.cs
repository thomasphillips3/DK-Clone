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

    IEnumerator Start()
    {
        // Ensure GameManager exists
        if (GameManager.Instance == null)
        {
            var gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            // AlbumConfig is set via serialized field on the prefab/scene object
        }

        // Ensure AlbumAudioManager exists
        if (AlbumAudioManager.Instance == null)
        {
            var audioGO = new GameObject("AlbumAudioManager");
            var aam = audioGO.AddComponent<AlbumAudioManager>();
            if (albumConfig != null)
                aam.SetAlbumConfig(albumConfig);
        }

        // Ensure AlbumPlaybackController exists
        if (AlbumPlaybackController.Instance == null)
        {
            var pbcGO = new GameObject("AlbumPlaybackController");
            pbcGO.AddComponent<AlbumPlaybackController>();
        }

        // Ensure SceneFlowManager exists
        if (SceneFlowManager.Instance == null)
        {
            var sfGO = new GameObject("SceneFlowManager");
            sfGO.AddComponent<SceneFlowManager>();
        }

        yield return new WaitForSeconds(bootDelay);

        SceneManager.LoadScene("Menu");
    }
}
