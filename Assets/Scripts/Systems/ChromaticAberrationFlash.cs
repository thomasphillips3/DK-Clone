using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChromaticAberrationFlash : MonoBehaviour
{
    public static ChromaticAberrationFlash instance { get; private set; }

    [SerializeField] private RawImage overlayImage;

    private Material chromaMat;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;

        if (overlayImage != null && overlayImage.material != null)
        {
            chromaMat = new Material(overlayImage.material);
            overlayImage.material = chromaMat;
            chromaMat.SetFloat("_Intensity", 0f);
            overlayImage.gameObject.SetActive(false);
        }
    }

    public void TriggerFlash(float duration = 0.3f)
    {
        if (overlayImage == null) return;
        StartCoroutine(FlashCoroutine(duration));
    }

    IEnumerator FlashCoroutine(float duration)
    {
        overlayImage.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Bell curve via sin: peaks at t=0.5
            float intensity = Mathf.Sin(t * Mathf.PI) * 0.04f;
            if (chromaMat != null) chromaMat.SetFloat("_Intensity", intensity);
            yield return null;
        }
        if (chromaMat != null) chromaMat.SetFloat("_Intensity", 0f);
        overlayImage.gameObject.SetActive(false);
    }
}
