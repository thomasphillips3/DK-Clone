using UnityEngine;

/// <summary>
/// Wall switch or equipment power button. Toggles a light or powers up a display.
/// Visual only — no audio effect.
/// </summary>
public class PowerSwitch : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject targetLight;
    [SerializeField] private Renderer indicatorRenderer;
    [SerializeField] private Color onColor = new Color(0.2f, 1f, 0.3f);
    [SerializeField] private Color offColor = new Color(0.1f, 0.1f, 0.1f);
    [SerializeField] private string promptOn = "Turn off";
    [SerializeField] private string promptOff = "Turn on";

    private bool isOn;
    private Material indicatorMat;

    public bool IsInteractable => true;

    void Start()
    {
        if (indicatorRenderer != null)
        {
            indicatorMat = indicatorRenderer.material;
            indicatorMat.SetColor("_EmissionColor", offColor);
        }
        if (targetLight != null)
            targetLight.SetActive(false);
    }

    public string GetPromptText() => isOn ? promptOn : promptOff;

    public void OnInteract()
    {
        isOn = !isOn;
        if (targetLight != null)
            targetLight.SetActive(isOn);
        if (indicatorMat != null)
            indicatorMat.SetColor("_EmissionColor", isOn ? onColor : offColor);
    }
}
