using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RetroPostProcessEffect : MonoBehaviour
{
    [Range(2, 256)]
    public int colorLevels = 128;

    [Range(0, 1)]
    public float pixelGridStrength = 0.08f;

    [Range(0, 1)]
    public float vignetteStrength = 0.04f;

    [SerializeField] private Shader retroShader;
    private Material retroMat;

    void OnEnable()
    {
        if (retroShader == null)
            retroShader = Shader.Find("Mixtape/RetroPostProcess");
        if (retroShader != null && retroMat == null)
            retroMat = new Material(retroShader);
    }

    void OnDisable()
    {
        if (retroMat != null)
        {
            DestroyImmediate(retroMat);
            retroMat = null;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (retroMat == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        retroMat.SetInt("_ColorLevels", colorLevels);
        retroMat.SetFloat("_PixelGridStrength", pixelGridStrength);
        retroMat.SetFloat("_VignetteStrength", vignetteStrength);
        Graphics.Blit(source, destination, retroMat);
    }
}
