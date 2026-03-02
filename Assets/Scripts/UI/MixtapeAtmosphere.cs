using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Phase 2: Creates and manages all atmospheric visual layers for the MixtapeMap screen.
/// Self-bootstraps: gradient background, vignette, grain, ambient glow, hero glow,
/// cassette glass overlay, drop shadow, tilt, reflection sweep, dust, and parallax.
/// </summary>
public class MixtapeAtmosphere : MonoBehaviour
{
    [Header("Dust Settings")]
    [SerializeField] private int dustCount = 12;
    [SerializeField] private float dustMinSpeed = 3f;
    [SerializeField] private float dustMaxSpeed = 8f;
    [SerializeField] private float dustDriftAmount = 15f;

    [Header("Glow Flicker")]
    [SerializeField] private float flickerSpeed = 0.7f;
    [SerializeField] private float flickerAmount = 0.08f;

    [Header("Cassette Hero")]
    [SerializeField] private float cassetteTilt = 2f;
    [SerializeField] private float sweepInterval = 9f;
    [SerializeField] private float sweepDuration = 1.5f;

    // Runtime-created layers
    private RawImage ambientGlowImage;
    private Material ambientGlowMat;
    private Material bgGradientMat;
    private Material grainMat;
    private Material glassOverlayMat;
    private Material sweepMat;
    private float baseGlowIntensity = 0.25f;

    // Sweep state
    private float sweepTimer;
    private bool isSweeping;

    // Parallax
    private RectTransform ambientGlowRT;
    private RectTransform heroGlowRT;
    private Vector2 ambientGlowBasePos;
    private Vector2 heroGlowBasePos;

    // Dust
    private DustMote[] dustMotes;
    private RectTransform dustContainer;
    private float containerHeight = 1920f;
    private float containerWidth = 1080f;

    private struct DustMote
    {
        public RectTransform rect;
        public float speed;
        public float phase;
        public float driftSpeed;
        public float baseX;
    }

    void Start()
    {
        CreateAtmosphereLayers();
        SpawnDust();
    }

    void CreateAtmosphereLayers()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();

        // ---- Background Container (behind everything) ----
        GameObject bgContainer = CreateUIObject("_AtmoBG", canvasRT);
        bgContainer.transform.SetAsFirstSibling();
        RectTransform bgRT = StretchFill(bgContainer);

        // Background — gradient + vignette + breathing bloom
        Shader gradientShader = Shader.Find("Mixtape/UIGradientVignette");
        if (gradientShader != null)
        {
            RawImage bgImage = bgContainer.AddComponent<RawImage>();
            bgGradientMat = new Material(gradientShader);
            bgGradientMat.SetColor("_TopColor", MixtapeColors.NavyTop);
            bgGradientMat.SetColor("_BottomColor", MixtapeColors.CharcoalBottom);
            bgGradientMat.SetFloat("_GradientBias", 0.4f);
            bgGradientMat.SetFloat("_VignetteStrength", 0.6f);
            bgGradientMat.SetFloat("_VignetteRadius", 0.8f);
            bgGradientMat.SetColor("_BloomColor", new Color(1f, 0.52f, 0f, 0.12f));
            bgGradientMat.SetFloat("_BloomSpeed", 0.3f);
            bgGradientMat.SetFloat("_BloomAmount", 0.08f);
            bgGradientMat.SetFloat("_BloomCenterX", 0.5f);
            bgGradientMat.SetFloat("_BloomCenterY", 0.65f);
            bgGradientMat.SetFloat("_BloomSoftness", 2.0f);
            bgGradientMat.SetFloat("_GlassMode", 0f);
            bgImage.material = bgGradientMat;
            bgImage.raycastTarget = false;
        }
        else
        {
            // Fallback: flat dark fill
            Image bgFill = bgContainer.AddComponent<Image>();
            bgFill.color = new Color(0.051f, 0.043f, 0.039f, 1f);
            bgFill.raycastTarget = false;
        }

        // Background grain overlay — enhanced with vignette grain
        Shader grainShader = Shader.Find("Mixtape/UINoiseOverlay");
        if (grainShader != null)
        {
            GameObject grainGO = CreateUIObject("BgGrain", bgRT);
            StretchFill(grainGO);
            RawImage grainImg = grainGO.AddComponent<RawImage>();
            grainMat = new Material(grainShader);
            grainMat.SetFloat("_GrainIntensity", 0.06f);
            grainMat.SetFloat("_GrainSpeed", 3f);
            grainMat.SetFloat("_GrainScale", 400f);
            grainMat.SetFloat("_GrainBrightness", 0.005f);
            grainMat.SetFloat("_VignetteGrain", 0.6f); // stronger at edges
            grainImg.material = grainMat;
            grainImg.color = new Color(1f, 1f, 1f, 0.10f);
            grainImg.raycastTarget = false;
        }

        // Ambient glow — large soft orange centered
        Shader glowShader = Shader.Find("Mixtape/UIGlowPulse");
        if (glowShader != null)
        {
            GameObject glowGO = CreateUIObject("AmbientGlow", bgRT);
            ambientGlowRT = glowGO.GetComponent<RectTransform>();
            ambientGlowRT.anchorMin = new Vector2(0.5f, 0.5f);
            ambientGlowRT.anchorMax = new Vector2(0.5f, 0.5f);
            ambientGlowBasePos = new Vector2(0f, 100f);
            ambientGlowRT.anchoredPosition = ambientGlowBasePos;
            ambientGlowRT.sizeDelta = new Vector2(700f, 700f);

            ambientGlowImage = glowGO.AddComponent<RawImage>();
            ambientGlowMat = new Material(glowShader);
            ambientGlowMat.SetColor("_GlowColor", MixtapeColors.BurntOrange);
            ambientGlowMat.SetFloat("_GlowIntensity", baseGlowIntensity);
            ambientGlowMat.SetFloat("_PulseSpeed", 0.8f);
            ambientGlowMat.SetFloat("_PulseAmount", 0.1f);
            ambientGlowMat.SetFloat("_Softness", 2.2f);
            ambientGlowImage.material = ambientGlowMat;
            ambientGlowImage.color = new Color(1f, 1f, 1f, 0.15f);
            ambientGlowImage.raycastTarget = false;

            // Hero glow — behind cassette area, brighter
            GameObject heroGlowGO = CreateUIObject("HeroGlow", bgRT);
            heroGlowRT = heroGlowGO.GetComponent<RectTransform>();
            heroGlowRT.anchorMin = new Vector2(0.5f, 0.5f);
            heroGlowRT.anchorMax = new Vector2(0.5f, 0.5f);
            heroGlowBasePos = new Vector2(0f, 530f);
            heroGlowRT.anchoredPosition = heroGlowBasePos;
            heroGlowRT.sizeDelta = new Vector2(1000f, 400f);

            RawImage heroGlowImg = heroGlowGO.AddComponent<RawImage>();
            Material heroMat = new Material(glowShader);
            heroMat.SetColor("_GlowColor", MixtapeColors.BurntOrange);
            heroMat.SetFloat("_GlowIntensity", 0.35f);
            heroMat.SetFloat("_PulseSpeed", 1.2f);
            heroMat.SetFloat("_PulseAmount", 0.08f);
            heroMat.SetFloat("_Softness", 1.8f);
            heroGlowImg.material = heroMat;
            heroGlowImg.color = new Color(1f, 1f, 1f, 0.2f);
            heroGlowImg.raycastTarget = false;
        }

        // ---- Dust Container (above background, below UI) ----
        GameObject dustGO = CreateUIObject("_AtmoDust", canvasRT);
        dustGO.transform.SetSiblingIndex(1);
        dustContainer = StretchFill(dustGO);

        // ---- Cassette Hero Enhancements ----
        SetupCassetteHero(canvasRT);
    }

    void SetupCassetteHero(RectTransform canvasRT)
    {
        // Find CassetteBody in the canvas hierarchy
        Transform cassetteBodyT = null;
        foreach (Transform child in canvasRT)
        {
            if (child.name == "CassetteBody")
            {
                cassetteBodyT = child;
                break;
            }
        }
        if (cassetteBodyT == null) return;

        RectTransform cassetteRT = cassetteBodyT.GetComponent<RectTransform>();

        // Apply subtle perspective tilt
        Vector3 rot = cassetteRT.localEulerAngles;
        rot.z = cassetteTilt;
        cassetteRT.localEulerAngles = rot;

        // Create drop shadow behind cassette
        CreateCassetteShadow(cassetteRT);

        // Create frosted glass overlay on cassette
        CreateGlassOverlay(cassetteRT);

        // Create reflection sweep
        CreateReflectionSweep(cassetteRT);
    }

    void CreateCassetteShadow(RectTransform cassetteRT)
    {
        // Create shadow as sibling behind cassette
        Transform parent = cassetteRT.parent;
        int cassetteIndex = cassetteRT.GetSiblingIndex();

        GameObject shadowGO = CreateUIObject("CassetteShadow", parent);
        shadowGO.transform.SetSiblingIndex(cassetteIndex); // just behind cassette

        RectTransform shadowRT = shadowGO.GetComponent<RectTransform>();
        // Copy cassette anchoring
        shadowRT.anchorMin = cassetteRT.anchorMin;
        shadowRT.anchorMax = cassetteRT.anchorMax;
        shadowRT.anchoredPosition = cassetteRT.anchoredPosition + new Vector2(6f, -8f);
        shadowRT.sizeDelta = cassetteRT.sizeDelta + new Vector2(20f, 20f);
        shadowRT.localEulerAngles = cassetteRT.localEulerAngles;

        Image shadowImg = shadowGO.AddComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.35f);
        shadowImg.raycastTarget = false;
    }

    void CreateGlassOverlay(RectTransform cassetteRT)
    {
        Shader glassShader = Shader.Find("Mixtape/UIGradientVignette");
        if (glassShader == null) return;

        GameObject glassGO = CreateUIObject("CassetteGlass", cassetteRT);
        RectTransform glassRT = StretchFill(glassGO);
        // Slight inset so it doesn't overlap edges
        glassRT.offsetMin = new Vector2(4f, 4f);
        glassRT.offsetMax = new Vector2(-4f, -4f);

        RawImage glassImg = glassGO.AddComponent<RawImage>();
        glassOverlayMat = new Material(glassShader);
        glassOverlayMat.SetFloat("_GlassMode", 1f);
        glassOverlayMat.SetFloat("_GlassOpacity", 0.06f);
        glassOverlayMat.SetColor("_GlassTint", new Color(1f, 0.92f, 0.8f, 1f));
        glassOverlayMat.SetFloat("_HighlightPos", 0.12f);
        glassOverlayMat.SetFloat("_HighlightWidth", 0.04f);
        glassOverlayMat.SetFloat("_GlassHighlight", 0.15f);
        glassImg.material = glassOverlayMat;
        glassImg.raycastTarget = false;
    }

    void CreateReflectionSweep(RectTransform cassetteRT)
    {
        Shader sweepShader = Shader.Find("Mixtape/UILEDStrip");
        if (sweepShader == null) return;

        GameObject sweepGO = CreateUIObject("CassetteSweep", cassetteRT);
        StretchFill(sweepGO);

        RawImage sweepImg = sweepGO.AddComponent<RawImage>();
        sweepMat = new Material(sweepShader);
        sweepMat.SetFloat("_RingMode", 0f);
        sweepMat.SetFloat("_SweepMode", 1f);
        sweepMat.SetFloat("_SweepProgress", -0.3f);
        sweepMat.SetFloat("_SweepWidth", 0.08f);
        sweepMat.SetColor("_SweepColor", new Color(1f, 1f, 1f, 0.05f));
        sweepMat.SetFloat("_Progress", 0f);
        sweepMat.SetColor("_StripColor", new Color(0f, 0f, 0f, 0f));
        sweepImg.material = sweepMat;
        sweepImg.raycastTarget = false;

        sweepTimer = sweepInterval;
    }

    void SpawnDust()
    {
        if (dustContainer == null) return;

        dustMotes = new DustMote[dustCount];
        float halfW = containerWidth * 0.5f;
        float halfH = containerHeight * 0.5f;

        for (int i = 0; i < dustCount; i++)
        {
            GameObject go = CreateUIObject($"Dust_{i}", dustContainer);
            RectTransform rt = go.GetComponent<RectTransform>();
            float size = Random.Range(3f, 8f);
            rt.sizeDelta = new Vector2(size, size);

            Image img = go.AddComponent<Image>();
            float alpha = Random.Range(0.03f, 0.10f);
            img.color = new Color(1f, 0.92f, 0.8f, alpha);
            img.raycastTarget = false;

            float startX = Random.Range(-halfW, halfW);
            float startY = Random.Range(-halfH, halfH);
            rt.anchoredPosition = new Vector2(startX, startY);

            dustMotes[i] = new DustMote
            {
                rect = rt,
                speed = Random.Range(dustMinSpeed, dustMaxSpeed),
                phase = Random.Range(0f, Mathf.PI * 2f),
                driftSpeed = Random.Range(0.3f, 0.8f),
                baseX = startX
            };
        }
    }

    void Update()
    {
        UpdateAmbientFlicker();
        UpdateDust();
        UpdateReflectionSweep();
        UpdateParallax();
    }

    void UpdateAmbientFlicker()
    {
        if (ambientGlowMat == null) return;

        float t = Time.unscaledTime;
        float flicker = Mathf.Sin(t * flickerSpeed) * 0.6f
                      + Mathf.Sin(t * flickerSpeed * 2.3f) * 0.3f
                      + Mathf.Sin(t * flickerSpeed * 5.1f) * 0.1f;
        ambientGlowMat.SetFloat("_GlowIntensity", baseGlowIntensity + flicker * flickerAmount);
    }

    void UpdateDust()
    {
        if (dustMotes == null) return;

        float halfH = containerHeight * 0.5f;
        float halfW = containerWidth * 0.5f;
        float time = Time.unscaledTime;

        for (int i = 0; i < dustMotes.Length; i++)
        {
            DustMote m = dustMotes[i];
            if (m.rect == null) continue;

            Vector2 pos = m.rect.anchoredPosition;
            pos.y += m.speed * Time.unscaledDeltaTime;
            pos.x = m.baseX + Mathf.Sin(time * m.driftSpeed + m.phase) * dustDriftAmount;

            if (pos.y > halfH + 10f)
            {
                pos.y = -halfH - 10f;
                m.baseX = Random.Range(-halfW, halfW);
                pos.x = m.baseX;
                dustMotes[i] = m;
            }

            m.rect.anchoredPosition = pos;
        }
    }

    void UpdateReflectionSweep()
    {
        if (sweepMat == null) return;

        sweepTimer -= Time.unscaledDeltaTime;

        if (sweepTimer <= 0f && !isSweeping)
        {
            isSweeping = true;
            sweepTimer = sweepDuration;
        }

        if (isSweeping)
        {
            float progress = 1f - (sweepTimer / sweepDuration);
            // Sweep from -0.3 to 1.3
            sweepMat.SetFloat("_SweepProgress", Mathf.Lerp(-0.3f, 1.3f, progress));

            sweepTimer -= Time.unscaledDeltaTime;
            if (progress >= 1f)
            {
                isSweeping = false;
                sweepTimer = sweepInterval;
                sweepMat.SetFloat("_SweepProgress", -0.3f);
            }
        }
    }

    void UpdateParallax()
    {
        // Gentle slow sine-wave parallax on glow layers
        float t = Time.unscaledTime;
        float offsetX = Mathf.Sin(t * 0.15f) * 8f;
        float offsetY = Mathf.Sin(t * 0.1f + 1.2f) * 5f;

        if (ambientGlowRT != null)
            ambientGlowRT.anchoredPosition = ambientGlowBasePos + new Vector2(offsetX, offsetY);

        if (heroGlowRT != null)
            heroGlowRT.anchoredPosition = heroGlowBasePos + new Vector2(offsetX * 0.5f, offsetY * 0.3f);
    }

    /// <summary>
    /// Shifts ambient glow and bloom color toward the selected track's primary color.
    /// </summary>
    public void OnTrackChanged(TrackLevelData track)
    {
        if (track == null) return;
        StopAllCoroutines();
        StartCoroutine(LerpColors(track.primaryColor, 0.5f));
    }

    IEnumerator LerpColors(Color targetColor, float duration)
    {
        Color warmTarget = Color.Lerp(targetColor, MixtapeColors.BurntOrange, 0.5f);

        Color startGlow = ambientGlowMat != null ? ambientGlowMat.GetColor("_GlowColor") : MixtapeColors.BurntOrange;
        Color startBloom = bgGradientMat != null ? bgGradientMat.GetColor("_BloomColor") : new Color(1f, 0.52f, 0f, 0.12f);
        Color bloomTarget = new Color(warmTarget.r, warmTarget.g, warmTarget.b, 0.12f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            if (ambientGlowMat != null)
                ambientGlowMat.SetColor("_GlowColor", Color.Lerp(startGlow, warmTarget, t));
            if (bgGradientMat != null)
                bgGradientMat.SetColor("_BloomColor", Color.Lerp(startBloom, bloomTarget, t));

            yield return null;
        }

        if (ambientGlowMat != null)
            ambientGlowMat.SetColor("_GlowColor", warmTarget);
        if (bgGradientMat != null)
            bgGradientMat.SetColor("_BloomColor", bloomTarget);
    }

    // Helpers
    static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = 5; // UI layer
        return go;
    }

    static RectTransform StretchFill(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    void OnDestroy()
    {
        if (ambientGlowMat != null) Destroy(ambientGlowMat);
        if (grainMat != null) Destroy(grainMat);
        if (bgGradientMat != null) Destroy(bgGradientMat);
        if (glassOverlayMat != null) Destroy(glassOverlayMat);
        if (sweepMat != null) Destroy(sweepMat);
    }
}
