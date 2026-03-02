using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Phase 2: MPC pad-style track button with LED progress strip, depress animation,
/// and inner glow ring on selection.
/// </summary>
public class MixtapeTrackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image lockIcon;
    [SerializeField] private Image completeBadge;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private Image selectionBorder;
    [SerializeField] private Image colorAccentStripe;
    [SerializeField] private Image backgroundImage;

    [Header("MPC Pad")]
    [SerializeField] private float depressScale = 0.97f;
    [SerializeField] private float depressDuration = 0.05f;
    [SerializeField] private float releaseDuration = 0.08f;
    [SerializeField] private float depressDarken = 0.08f;

    private Button button;
    private int trackIndex;
    private MixtapeMapController controller;
    private Color trackPrimaryColor = Color.cyan;
    private TrackCardGlowEffect glowEffect;
    private Material bevelMat;

    // LED strip
    private Material ledStripMat;
    private RawImage ledStripImage;
    private float cachedProgress;

    // Depress
    private RectTransform rect;
    private Vector3 baseScale;
    private bool isDepressed;
    private Coroutine depressRoutine;

    void Awake()
    {
        button = GetComponent<Button>();
        rect = GetComponent<RectTransform>();
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        ApplyBevelMaterial();
        CreateLEDStrip();

        // Auto-add glow effect
        glowEffect = GetComponent<TrackCardGlowEffect>();
        if (glowEffect == null)
            glowEffect = gameObject.AddComponent<TrackCardGlowEffect>();
    }

    void Start()
    {
        baseScale = rect.localScale;
    }

    void ApplyBevelMaterial()
    {
        if (backgroundImage == null) return;
        Shader bevelShader = Shader.Find("Mixtape/UIBevelCard");
        if (bevelShader == null) return;

        bevelMat = new Material(bevelShader);
        bevelMat.SetColor("_BaseColor", MixtapeColors.PadUnselected);
        bevelMat.SetFloat("_CornerRadius", 0.05f);
        bevelMat.SetFloat("_BevelStrength", 0.12f);
        bevelMat.SetFloat("_ShadowInset", 0.04f);
        bevelMat.SetFloat("_NoiseIntensity", 0.025f); // slightly more texture
        bevelMat.SetFloat("_HighlightStrength", 0.08f);
        bevelMat.SetFloat("_DepressAmount", 0f);
        bevelMat.SetColor("_InnerGlowColor", new Color(0f, 0f, 0f, 0f));
        bevelMat.SetFloat("_InnerGlowWidth", 0f);
        backgroundImage.material = bevelMat;
        backgroundImage.color = Color.white;
    }

    void CreateLEDStrip()
    {
        Shader ledShader = Shader.Find("Mixtape/UILEDStrip");
        if (ledShader == null) return;

        GameObject go = new GameObject("LEDStrip", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        go.layer = 5;

        // Position at bottom edge with slight bleed
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, -2f);
        rt.sizeDelta = new Vector2(0f, 16f); // 16px tall strip area

        ledStripImage = go.AddComponent<RawImage>();
        ledStripMat = new Material(ledShader);
        ledStripMat.SetColor("_StripColor", MixtapeColors.BurntOrange);
        ledStripMat.SetFloat("_Progress", 0f);
        ledStripMat.SetFloat("_StripHeight", 0.25f);
        ledStripMat.SetFloat("_StripY", 0.5f);
        ledStripMat.SetFloat("_Glow", 0.15f);
        ledStripMat.SetFloat("_PulseSpeed", 0f);
        ledStripMat.SetFloat("_RingMode", 0f);
        ledStripMat.SetFloat("_SweepMode", 0f);
        ledStripImage.material = ledStripMat;
        ledStripImage.raycastTarget = false;
    }

    public void Refresh(TrackSaveData save, TrackLevelData data, int index, MixtapeMapController ctrl)
    {
        trackIndex = index;
        controller = ctrl;

        if (data != null)
            trackPrimaryColor = data.primaryColor;

        if (numberText != null) numberText.text = (index + 1).ToString("D2");
        if (lockIcon != null) lockIcon.enabled = !save.isUnlocked;
        if (completeBadge != null) completeBadge.enabled = save.isCompleted;

        // LED strip progress
        float duration = (data?.audioClip != null) ? data.audioClip.length : 1f;
        cachedProgress = (save.isUnlocked && duration > 0f)
            ? Mathf.Clamp01(save.lastTimestamp / duration)
            : 0f;

        if (ledStripMat != null)
        {
            ledStripMat.SetFloat("_Progress", cachedProgress);
            if (data != null)
                ledStripMat.SetColor("_StripColor", data.primaryColor);
        }

        // Keep original progressFill for backward compat but hide it
        if (progressFill != null)
        {
            progressFill.fillAmount = cachedProgress;
            if (data != null)
                progressFill.color = data.primaryColor;
            progressFill.enabled = false; // LED strip replaces it
        }

        if (titleText != null)
        {
            titleText.text = data?.trackTitle ?? $"Track {index + 1}";
            titleText.color = MixtapeColors.TextPrimary;
        }

        if (colorAccentStripe != null && data != null)
            colorAccentStripe.color = data.primaryColor;

        if (button != null)
        {
            button.interactable = save.isUnlocked;
            button.onClick.RemoveAllListeners();
            if (save.isUnlocked)
            {
                int captured = trackIndex;
                button.onClick.AddListener(() => controller?.SelectTrack(captured));
            }
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.enabled = selected;

        // Update card color via bevel material
        if (bevelMat != null)
        {
            bevelMat.SetColor("_BaseColor", selected ? MixtapeColors.PadSelected : MixtapeColors.PadUnselected);

            // Inner glow ring on selected
            if (selected)
            {
                bevelMat.SetColor("_InnerGlowColor", new Color(trackPrimaryColor.r, trackPrimaryColor.g, trackPrimaryColor.b, 0.6f));
                bevelMat.SetFloat("_InnerGlowWidth", 0.03f);
            }
            else
            {
                bevelMat.SetColor("_InnerGlowColor", new Color(0f, 0f, 0f, 0f));
                bevelMat.SetFloat("_InnerGlowWidth", 0f);
            }
        }

        if (numberText != null)
            numberText.color = selected ? MixtapeColors.BurntOrange : MixtapeColors.TextMutedWarm;

        // LED strip pulse when selected
        if (ledStripMat != null)
        {
            ledStripMat.SetFloat("_PulseSpeed", selected ? 1.5f : 0f);
            ledStripMat.SetFloat("_PulseAmount", selected ? 0.15f : 0f);
        }

        // Drive outer glow effect
        if (glowEffect != null)
        {
            if (selected)
                glowEffect.SetGlowColor(trackPrimaryColor);
            glowEffect.SetGlowing(selected);
        }
    }

    // --- MPC Pad Depress Animation ---

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        isDepressed = true;
        MixtapeUIAudio.PlayClick();
        if (depressRoutine != null) StopCoroutine(depressRoutine);
        depressRoutine = StartCoroutine(AnimateDepress(true));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDepressed = false;
        if (depressRoutine != null) StopCoroutine(depressRoutine);
        depressRoutine = StartCoroutine(AnimateDepress(false));
    }

    IEnumerator AnimateDepress(bool down)
    {
        float targetScale = down ? depressScale : 1f;
        float targetDarken = down ? depressDarken : 0f;
        float duration = down ? depressDuration : releaseDuration;

        Vector3 startScale = rect.localScale;
        Vector3 endScale = baseScale * targetScale;
        float startDarken = bevelMat != null ? bevelMat.GetFloat("_DepressAmount") : 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            rect.localScale = Vector3.Lerp(startScale, endScale, t);
            if (bevelMat != null)
                bevelMat.SetFloat("_DepressAmount", Mathf.Lerp(startDarken, targetDarken, t));
            yield return null;
        }

        rect.localScale = endScale;
        if (bevelMat != null)
            bevelMat.SetFloat("_DepressAmount", targetDarken);
        depressRoutine = null;
    }

    void OnDestroy()
    {
        if (bevelMat != null) Destroy(bevelMat);
        if (ledStripMat != null) Destroy(ledStripMat);
    }
}
