using UnityEngine;
using UnityEngine.SceneManagement;

public class TrackEndController : MonoBehaviour
{
    public AudioSource source;
    public GameObject endPanel; // assign a UI panel
    public float autoNextDelay = 2.5f;
    public string nextSceneName = ""; // optional

    bool ended;

    void Update()
    {
        if (ended) return;
        if (!source || source.clip == null) return;

        if (!source.isPlaying && source.time > 0.1f)
        {
            EndRun();
        }
        else if (source.time >= source.clip.length - 0.02f)
        {
            EndRun();
        }
    }

    void EndRun()
    {
        ended = true;

        if (endPanel) endPanel.SetActive(true);

        // Optional auto-load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
            Invoke(nameof(LoadNext), autoNextDelay);
    }

    void LoadNext()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
