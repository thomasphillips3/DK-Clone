using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages a per-card selection glow effect. Self-bootstraps: creates its own GlowLayer RawImage at runtime.
/// </summary>
public class TrackCardGlowEffect : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float pulseMin = 0.1f;
    [SerializeField] private float pulseMax = 0.35f;
    [SerializeField] private float fadeOutDuration = 0.2f;

    private RawImage glowImage;
    private Material glowMat;
    private Coroutine activeRoutine;

    void Awake()
    {
        CreateGlowLayer();
    }

    void CreateGlowLayer()
    {
        Shader glowShader = Shader.Find("Mixtape/UIGlowPulse");
        if (glowShader == null) return;

        // Create glow image as first child (renders behind card content)
        GameObject go = new GameObject("GlowLayer", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        go.layer = 5;
        go.transform.SetParent(transform, false);
        go.transform.SetAsFirstSibling();

        // Stretch fill with 14px bleed for soft edge beyond card bounds
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-14f, -14f);
        rt.offsetMax = new Vector2(14f, 14f);

        glowImage = go.GetComponent<RawImage>();
        glowImage.raycastTarget = false;

        glowMat = new Material(glowShader);
        glowMat.SetColor("_GlowColor", new Color(1f, 0.52f, 0f, 1f));
        glowMat.SetFloat("_GlowIntensity", 0f);
        glowMat.SetFloat("_PulseSpeed", 0f); // we drive pulse manually
        glowMat.SetFloat("_PulseAmount", 0f);
        glowMat.SetFloat("_Softness", 2.5f);
        glowImage.material = glowMat;
        glowImage.enabled = false;
    }

    public void SetGlowing(bool active)
    {
        if (glowMat == null || glowImage == null) return;

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (active)
        {
            glowImage.enabled = true;
            activeRoutine = StartCoroutine(PulseGlow());
        }
        else
        {
            activeRoutine = StartCoroutine(FadeOutGlow());
        }
    }

    public void SetGlowColor(Color color)
    {
        if (glowMat == null) return;
        glowMat.SetColor("_GlowColor", color);
    }

    IEnumerator PulseGlow()
    {
        while (true)
        {
            float t = Time.unscaledTime * pulseSpeed;
            float intensity = Mathf.Lerp(pulseMin, pulseMax, (Mathf.Sin(t) + 1f) * 0.5f);
            glowMat.SetFloat("_GlowIntensity", intensity);
            yield return null;
        }
    }

    IEnumerator FadeOutGlow()
    {
        float startIntensity = glowMat.GetFloat("_GlowIntensity");
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            glowMat.SetFloat("_GlowIntensity", Mathf.Lerp(startIntensity, 0f, elapsed / fadeOutDuration));
            yield return null;
        }

        glowMat.SetFloat("_GlowIntensity", 0f);
        glowImage.enabled = false;
        activeRoutine = null;
    }

    void OnDestroy()
    {
        if (glowMat != null) Destroy(glowMat);
    }
}
