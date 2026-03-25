using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Scene loading with warm black fade overlay. Supports menu, connected, and sequential modes.
/// Audio manager is DontDestroyOnLoad — audio continues seamlessly during transitions.
/// </summary>
public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance { get; private set; }

    [SerializeField] private float fadeDuration = 0.8f;

    private Image overlayImage;
    private bool isTransitioning;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateOverlay();
    }

    void CreateOverlay()
    {
        var canvasGO = new GameObject("_TransitionCanvas");
        canvasGO.transform.SetParent(transform);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasGO.AddComponent<CanvasScaler>();

        var imgGO = new GameObject("_Overlay");
        imgGO.transform.SetParent(canvasGO.transform, false);

        overlayImage = imgGO.AddComponent<Image>();
        var rt = overlayImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        overlayImage.color = new Color(0.05f, 0.03f, 0.02f, 0f); // warm near-black
        overlayImage.gameObject.SetActive(false);
    }

    public void LoadMenu()
    {
        if (!isTransitioning)
            StartCoroutine(TransitionCoroutine("Menu"));
    }

    public void LoadRoom(int trackIndex)
    {
        var config = GameManager.Instance?.AlbumConfig;
        if (config == null) return;

        TrackData track = config.GetTrack(trackIndex);
        if (track == null || string.IsNullOrEmpty(track.sceneName)) return;

        if (!isTransitioning)
            StartCoroutine(TransitionToRoom(trackIndex, track));
    }

    IEnumerator TransitionToRoom(int trackIndex, TrackData track)
    {
        isTransitioning = true;
        overlayImage.gameObject.SetActive(true);

        // Fade to black
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(1f);

        // Load scene
        AsyncOperation op = SceneManager.LoadSceneAsync(track.sceneName);
        if (op != null) yield return op;

        // Start audio for this room
        AlbumPlaybackController.Instance?.PlayTrackForRoom(trackIndex);

        // Brief hold on black
        yield return new WaitForSecondsRealtime(0.3f);

        // Fade from black
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.SmoothStep(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);

        overlayImage.gameObject.SetActive(false);
        isTransitioning = false;
    }

    IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;
        overlayImage.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(1f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op != null) yield return op;

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.SmoothStep(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);

        overlayImage.gameObject.SetActive(false);
        isTransitioning = false;
    }

    void SetAlpha(float alpha)
    {
        if (overlayImage == null) return;
        Color c = overlayImage.color;
        c.a = alpha;
        overlayImage.color = c;
    }
}
