using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures only one AudioListener exists at any time.
/// Attach to the Player prefab's camera alongside AudioListener.
/// On scene load, disables any duplicate AudioListeners.
/// </summary>
public class AudioListenerGuard : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureSingleListener();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureSingleListener();
    }

    void EnsureSingleListener()
    {
        var listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length <= 1) return;

        // Keep the one on this object, disable all others
        foreach (var listener in listeners)
        {
            if (listener.gameObject != gameObject)
                listener.enabled = false;
        }
    }
}
