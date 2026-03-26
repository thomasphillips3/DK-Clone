using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Splash screen shown during Boot. Fades in album title and artist,
/// holds briefly, then fades out before loading Menu.
/// Gives singletons time to initialize and sets the mood.
/// </summary>
public class SplashController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI artistText;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private float fadeOutDuration = 1f;

    void Start()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        var config = GameManager.Instance?.AlbumConfig;
        if (config != null)
        {
            if (titleText != null) titleText.text = config.albumTitle;
            if (artistText != null) artistText.text = config.artistName;
        }

        StartCoroutine(SplashSequence());
    }

    IEnumerator SplashSequence()
    {
        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        // Hold
        yield return new WaitForSecondsRealtime(holdDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.SmoothStep(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
    }
}
