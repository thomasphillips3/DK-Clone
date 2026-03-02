using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

/// <summary>
/// Phase 2: Boutique hardware PLAY button with bevel, glow background,
/// breathing scale, press depress, outward glow pulse, and text spacing.
/// </summary>
public class PlayButtonAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Breathing")]
    [SerializeField] private float breatheSpeed = 0.5f;
    [SerializeField] private float breatheAmount = 0.02f;

    [Header("Press")]
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float pressDownDuration = 0.05f;
    [SerializeField] private float pressUpDuration = 0.12f;

    [Header("Glow Pulse")]
    [SerializeField] private float glowPulseIntensity = 0.7f;
    [SerializeField] private float glowPulseDecay = 0.4f;

    private RectTransform rect;
    private Vector3 baseScale;
    private Image buttonImage;
    private bool isPressed;
    private Coroutine pressRoutine;

    // Glow material reference for outward pulse
    private Material glowBgMat;
    private float baseGlowIntensity = 0.4f;
    private Coroutine glowPulseRoutine;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
    }

    void Start()
    {
        baseScale = rect.localScale;
        CreateGlowBackground();
        ApplyBevelMaterial();
        ApplyTextSpacing();
    }

    void CreateGlowBackground()
    {
        Shader glowShader = Shader.Find("Mixtape/UIGlowPulse");
        if (glowShader == null) return;

        GameObject go = new GameObject("PlayGlow", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        go.layer = 5;
        go.transform.SetParent(transform, false);
        go.transform.SetAsFirstSibling();

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-24f, -24f);
        rt.offsetMax = new Vector2(24f, 24f);

        RawImage img = go.GetComponent<RawImage>();
        img.raycastTarget = false;

        glowBgMat = new Material(glowShader);
        glowBgMat.SetColor("_GlowColor", new Color(0.88f, 0.44f, 0f, 1f));
        glowBgMat.SetFloat("_GlowIntensity", baseGlowIntensity);
        glowBgMat.SetFloat("_PulseSpeed", 1f);
        glowBgMat.SetFloat("_PulseAmount", 0.1f);
        glowBgMat.SetFloat("_Softness", 1.8f);
        img.material = glowBgMat;
    }

    void ApplyBevelMaterial()
    {
        if (buttonImage == null) return;
        Shader bevelShader = Shader.Find("Mixtape/UIBevelCard");
        if (bevelShader == null) return;

        Material mat = new Material(bevelShader);
        mat.SetColor("_BaseColor", new Color(0.88f, 0.44f, 0f, 1f));
        mat.SetFloat("_CornerRadius", 0.08f);
        mat.SetFloat("_BevelStrength", 0.15f);
        mat.SetFloat("_ShadowInset", 0.05f);
        mat.SetFloat("_NoiseIntensity", 0.01f);
        mat.SetFloat("_HighlightStrength", 0.12f);
        mat.SetFloat("_DepressAmount", 0f);
        buttonImage.material = mat;
        buttonImage.color = Color.white;
    }

    void ApplyTextSpacing()
    {
        TMP_Text text = GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.characterSpacing = 10f;
            text.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        }
    }

    void Update()
    {
        if (isPressed) return;

        float breathe = 1f + Mathf.Sin(Time.unscaledTime * breatheSpeed * Mathf.PI * 2f) * breatheAmount;
        rect.localScale = baseScale * breathe;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        MixtapeUIAudio.PlayPress();

        if (pressRoutine != null) StopCoroutine(pressRoutine);
        pressRoutine = StartCoroutine(AnimatePress(true));

        // Outward glow pulse
        if (glowPulseRoutine != null) StopCoroutine(glowPulseRoutine);
        glowPulseRoutine = StartCoroutine(GlowPulseOnPress());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        if (pressRoutine != null) StopCoroutine(pressRoutine);
        pressRoutine = StartCoroutine(AnimatePress(false));
    }

    IEnumerator AnimatePress(bool down)
    {
        float targetScale = down ? pressScale : 1f;
        float duration = down ? pressDownDuration : pressUpDuration;

        Vector3 startScale = rect.localScale;
        Vector3 endScale = baseScale * targetScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            rect.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        rect.localScale = endScale;
        pressRoutine = null;
    }

    IEnumerator GlowPulseOnPress()
    {
        if (glowBgMat == null) yield break;

        // Spike intensity
        glowBgMat.SetFloat("_GlowIntensity", glowPulseIntensity);

        // Decay back to base
        float elapsed = 0f;
        while (elapsed < glowPulseDecay)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / glowPulseDecay;
            float intensity = Mathf.Lerp(glowPulseIntensity, baseGlowIntensity, t * t);
            glowBgMat.SetFloat("_GlowIntensity", intensity);
            yield return null;
        }

        glowBgMat.SetFloat("_GlowIntensity", baseGlowIntensity);
        glowPulseRoutine = null;
    }

    void OnDestroy()
    {
        if (glowBgMat != null) Destroy(glowBgMat);
    }
}
