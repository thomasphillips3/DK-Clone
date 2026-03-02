using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class BootController : MonoBehaviour
{
    [SerializeField] private Image logoImage;
    [SerializeField] private TMP_Text artistNameText;
    [SerializeField] private float logoFadeDuration = 1.5f;
    [SerializeField] private float holdDuration = 1.5f;

    void Start()
    {
        SetAlpha(logoImage, 0f);
        SetAlpha(artistNameText, 0f);
        if (artistNameText != null && MixtapeManager.instance?.artistConfig != null)
            artistNameText.text = MixtapeManager.instance.artistConfig.albumTitle;

        // Use Invoke chain instead of Update — immune to first-frame deltaTime spike
        // and the mysterious Update-stops-after-1-call editor bug
        Invoke(nameof(FadeInLogo), 0.05f);
    }

    void FadeInLogo()
    {
        StartCoroutine(FadeGraphic(logoImage, logoFadeDuration, () =>
        {
            StartCoroutine(FadeGraphic(artistNameText, logoFadeDuration, () =>
            {
                Invoke(nameof(GoToMap), holdDuration);
            }));
        }));
    }

    void GoToMap()
    {
        if (SceneTransitionManager.instance != null)
            SceneTransitionManager.instance.TransitionToScene("MixtapeMap");
        else
            SceneManager.LoadScene("MixtapeMap");
    }

    IEnumerator FadeGraphic(Graphic target, float duration, System.Action onComplete)
    {
        if (target == null) { onComplete?.Invoke(); yield break; }

        float t = 0f;
        while (t < duration)
        {
            // Skip first frame to avoid domain-reload deltaTime spike
            yield return null;
            t += Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            SetAlpha(target, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration)));
        }
        SetAlpha(target, 1f);
        onComplete?.Invoke();
    }

    void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }
}
