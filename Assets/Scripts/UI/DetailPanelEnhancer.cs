using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Phase 2: Self-bootstrapping detail panel upgrade.
/// Adds frosted glass look, metallic border, and digital counter styling.
/// Attach to the DetailPanel GameObject.
/// </summary>
public class DetailPanelEnhancer : MonoBehaviour
{
    private Material glassMat;
    private Material borderMat;

    void Start()
    {
        ApplyGlassPanel();
        CreateMetallicBorder();
        StyleDigitalCounters();
    }

    void ApplyGlassPanel()
    {
        Image panelImage = GetComponent<Image>();
        if (panelImage == null) return;

        Shader glassShader = Shader.Find("Mixtape/UIGradientVignette");
        if (glassShader == null) return;

        glassMat = new Material(glassShader);
        glassMat.SetFloat("_GlassMode", 1f);
        glassMat.SetFloat("_GlassOpacity", 0.07f);
        glassMat.SetColor("_GlassTint", new Color(0.8f, 0.85f, 0.9f, 1f));
        glassMat.SetFloat("_HighlightPos", 0.1f);
        glassMat.SetFloat("_HighlightWidth", 0.03f);
        glassMat.SetFloat("_GlassHighlight", 0.1f);
        panelImage.material = glassMat;
        panelImage.color = new Color(1f, 1f, 1f, 0.95f);
    }

    void CreateMetallicBorder()
    {
        Shader rimShader = Shader.Find("Mixtape/UIRimLight");
        if (rimShader == null) return;

        GameObject borderGO = new GameObject("MetallicBorder", typeof(RectTransform));
        borderGO.transform.SetParent(transform, false);
        borderGO.transform.SetAsFirstSibling();
        borderGO.layer = 5;

        RectTransform rt = borderGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-1f, -1f);
        rt.offsetMax = new Vector2(1f, 1f);

        RawImage borderImg = borderGO.AddComponent<RawImage>();
        borderMat = new Material(rimShader);
        borderMat.SetColor("_RimColor", new Color(0.5f, 0.5f, 0.55f, 0.4f));
        borderMat.SetFloat("_RimWidth", 0.008f);
        borderMat.SetFloat("_RimSoftness", 1.2f);
        borderMat.SetFloat("_RimIntensity", 0.6f);
        borderMat.SetFloat("_InnerAlpha", 0f);
        borderImg.material = borderMat;
        borderImg.raycastTarget = false;
    }

    void StyleDigitalCounters()
    {
        // Find stats and BPM text components by name
        TMP_Text bpmText = FindTextByName("DetailBpmText");
        TMP_Text statsText = FindTextByName("DetailStatsText");

        if (bpmText != null)
        {
            bpmText.color = MixtapeColors.TextSecondary;
            bpmText.characterSpacing = 4f;
            bpmText.fontStyle = FontStyles.UpperCase;
        }

        if (statsText != null)
        {
            statsText.color = MixtapeColors.TextSecondary;
            statsText.characterSpacing = 3f;
            statsText.fontStyle = FontStyles.UpperCase;
        }
    }

    TMP_Text FindTextByName(string name)
    {
        // Search direct children first
        Transform t = transform.Find(name);
        if (t != null) return t.GetComponent<TMP_Text>();

        // Search all descendants
        foreach (TMP_Text txt in GetComponentsInChildren<TMP_Text>(true))
        {
            if (txt.gameObject.name == name)
                return txt;
        }
        return null;
    }

    void OnDestroy()
    {
        if (glassMat != null) Destroy(glassMat);
        if (borderMat != null) Destroy(borderMat);
    }
}
