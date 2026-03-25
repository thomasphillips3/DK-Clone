using UnityEngine;
using TMPro;

/// <summary>
/// Shows a subtle interaction prompt when the player looks at an IInteractable.
/// </summary>
public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private string interactKey = "E";

    private InteractionController interactionController;

    void Start()
    {
        interactionController = FindFirstObjectByType<InteractionController>();
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (interactionController == null || canvasGroup == null) return;

        bool hasTarget = interactionController.HasTarget;
        float targetAlpha = hasTarget ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * 6f);

        if (hasTarget && promptText != null)
        {
            string text = interactionController.CurrentTarget.GetPromptText();
            promptText.text = $"[{interactKey}] {text}";
        }
    }
}
