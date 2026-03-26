using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Boot scene entry point. Initializes singletons, shows splash screen
/// with album title, then transitions to Menu.
/// </summary>
public class BootController : MonoBehaviour
{
    [SerializeField] private AlbumConfig albumConfig;
    [SerializeField] private float splashFadeIn = 1.2f;
    [SerializeField] private float splashHold = 1.8f;
    [SerializeField] private float splashFadeOut = 0.8f;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        InitializeSingletons();
        StartCoroutine(SplashThenLoadMenu());
    }

    IEnumerator SplashThenLoadMenu()
    {
        // Build splash UI
        var canvasGO = new GameObject("SplashCanvas");
        canvasGO.transform.SetParent(transform);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        var cg = canvasGO.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // Background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.04f, 0.03f, 0.02f, 1f);
        var bgRt = bgImg.rectTransform;
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;

        // Title
        var titleGO = new GameObject("SplashTitle");
        titleGO.transform.SetParent(canvasGO.transform, false);
        var titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = albumConfig != null ? albumConfig.albumTitle : "time off 3";
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(1f, 0.92f, 0.78f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        var titleRt = titleText.rectTransform;
        titleRt.anchorMin = new Vector2(0, 0.45f);
        titleRt.anchorMax = new Vector2(1, 0.55f);
        titleRt.offsetMin = titleRt.offsetMax = Vector2.zero;

        // Artist
        var artistGO = new GameObject("SplashArtist");
        artistGO.transform.SetParent(canvasGO.transform, false);
        var artistText = artistGO.AddComponent<TextMeshProUGUI>();
        artistText.text = albumConfig != null ? albumConfig.artistName : "Bombest Music";
        artistText.fontSize = 28;
        artistText.color = new Color(0.85f, 0.65f, 0.4f, 0.8f);
        artistText.alignment = TextAlignmentOptions.Center;
        var artistRt = artistText.rectTransform;
        artistRt.anchorMin = new Vector2(0, 0.38f);
        artistRt.anchorMax = new Vector2(1, 0.44f);
        artistRt.offsetMin = artistRt.offsetMax = Vector2.zero;

        // Fade in
        float elapsed = 0f;
        while (elapsed < splashFadeIn)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.SmoothStep(0f, 1f, elapsed / splashFadeIn);
            yield return null;
        }
        cg.alpha = 1f;

        // Hold
        yield return new WaitForSecondsRealtime(splashHold);

        // Fade out
        elapsed = 0f;
        while (elapsed < splashFadeOut)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.SmoothStep(1f, 0f, elapsed / splashFadeOut);
            yield return null;
        }

        // Load menu
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
        if (SceneManager.GetActiveScene().name != "Boot")
            Destroy(gameObject);
    }
}
