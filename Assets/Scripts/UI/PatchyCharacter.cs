using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Phase 2: Patchy — the curious robot mascot made of spare audio parts.
/// Handles idle bobbing, reacts to track selection with bounce,
/// leans toward selected track, and displays accent-color rim light.
/// </summary>
public class PatchyCharacter : MonoBehaviour
{
    [Header("Idle Animation")]
    [SerializeField] private float bobAmplitude = 8f;
    [SerializeField] private float bobSpeed = 1.2f;
    [SerializeField] private float tiltAmplitude = 2f;
    [SerializeField] private float tiltSpeed = 0.8f;

    [Header("Bounce Reaction")]
    [SerializeField] private float bounceScale = 1.15f;
    [SerializeField] private float bounceDuration = 0.3f;

    [Header("Lean Toward Track")]
    [SerializeField] private float leanAmount = 12f;
    [SerializeField] private float leanRotation = 3f;
    [SerializeField] private float leanSpeed = 2f;

    private RectTransform rect;
    private Vector2 basePosition;
    private float bobPhase;
    private float bounceTimer;
    private bool isBouncing;
    private Vector3 baseScale;

    // Lean state
    private float targetLeanX;
    private float targetLeanRot;
    private float currentLeanX;
    private float currentLeanRot;

    // Rim light
    private Material rimLightMat;
    private Color currentRimColor = new Color(1f, 0.52f, 0f, 0.3f);

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        basePosition = rect.anchoredPosition;
        baseScale = rect.localScale;
        bobPhase = Random.Range(0f, Mathf.PI * 2f);
        CreateRimLight();
    }

    void CreateRimLight()
    {
        Shader rimShader = Shader.Find("Mixtape/UIRimLight");
        if (rimShader == null) return;

        GameObject go = new GameObject("PatchyRimLight", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        go.transform.SetAsFirstSibling(); // behind Patchy's image
        go.layer = 5;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-6f, -6f);
        rt.offsetMax = new Vector2(6f, 6f);

        RawImage img = go.AddComponent<RawImage>();
        rimLightMat = new Material(rimShader);
        rimLightMat.SetColor("_RimColor", currentRimColor);
        rimLightMat.SetFloat("_RimWidth", 0.04f);
        rimLightMat.SetFloat("_RimSoftness", 1.8f);
        rimLightMat.SetFloat("_RimIntensity", 0.4f);
        rimLightMat.SetFloat("_InnerAlpha", 0f);
        img.material = rimLightMat;
        img.raycastTarget = false;
    }

    void Update()
    {
        float time = Time.unscaledTime;

        // Smooth lean toward target
        currentLeanX = Mathf.Lerp(currentLeanX, targetLeanX, Time.unscaledDeltaTime * leanSpeed);
        currentLeanRot = Mathf.Lerp(currentLeanRot, targetLeanRot, Time.unscaledDeltaTime * leanSpeed);

        // Gentle floating bob + lean offset
        float bobOffset = Mathf.Sin((time * bobSpeed) + bobPhase) * bobAmplitude;
        rect.anchoredPosition = basePosition + new Vector2(currentLeanX, bobOffset);

        // Subtle tilt oscillation + lean rotation
        float tilt = Mathf.Sin((time * tiltSpeed) + bobPhase + 0.7f) * tiltAmplitude;
        Vector3 euler = rect.localEulerAngles;
        euler.z = tilt + currentLeanRot;
        rect.localEulerAngles = euler;

        // Bounce reaction (scale pulse on track select)
        if (isBouncing)
        {
            bounceTimer += Time.unscaledDeltaTime;
            float progress = bounceTimer / bounceDuration;
            if (progress >= 1f)
            {
                isBouncing = false;
                rect.localScale = baseScale;
            }
            else
            {
                float curve = Mathf.Sin(progress * Mathf.PI) * (1f - progress);
                float scale = 1f + (bounceScale - 1f) * curve;
                rect.localScale = baseScale * scale;
            }
        }

        // Rim light subtle pulse
        if (rimLightMat != null)
        {
            float pulse = 0.35f + Mathf.Sin(time * 0.8f) * 0.1f;
            rimLightMat.SetFloat("_RimIntensity", pulse);
        }
    }

    /// <summary>
    /// Call this when the player selects a new track. Patchy does a happy bounce.
    /// </summary>
    public void OnTrackSelected()
    {
        isBouncing = true;
        bounceTimer = 0f;
    }

    /// <summary>
    /// Phase 2: Lean toward the selected track and update rim light color.
    /// </summary>
    public void OnTrackSelected(int trackIndex, int totalTracks, Color accentColor)
    {
        OnTrackSelected(); // bounce

        // Compute lean direction: left tracks (even indices in 2-col grid) → lean left
        // 2-column layout: column = trackIndex % 2, 0 = left, 1 = right
        int column = trackIndex % 2;
        float normalizedDir = (column == 0) ? -1f : 1f;
        targetLeanX = normalizedDir * leanAmount;
        targetLeanRot = -normalizedDir * leanRotation; // tilt opposite to lean

        // Update rim light color
        currentRimColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.4f);
        if (rimLightMat != null)
            rimLightMat.SetColor("_RimColor", currentRimColor);
    }

    void OnDestroy()
    {
        if (rimLightMat != null) Destroy(rimLightMat);
    }
}
