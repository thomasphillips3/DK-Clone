using UnityEngine;

/// <summary>
/// Mixing console fader. Interact to slide it up — channel meter activates.
/// Visual only — no audio effect.
/// </summary>
public class FaderSlide : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform faderKnob;
    [SerializeField] private float slideDistance = 0.08f;
    [SerializeField] private GameObject channelMeter;

    private bool isUp;
    private bool isAnimating;
    private Vector3 downPosition;

    public bool IsInteractable => !isAnimating;

    void Start()
    {
        if (faderKnob != null)
            downPosition = faderKnob.localPosition;
        if (channelMeter != null)
            channelMeter.SetActive(false);
    }

    public string GetPromptText() => isUp ? "Slide fader down" : "Slide fader up";

    public void OnInteract()
    {
        if (isAnimating) return;
        isAnimating = true;
        StartCoroutine(AnimateSlide());
    }

    System.Collections.IEnumerator AnimateSlide()
    {
        if (faderKnob == null)
        {
            isUp = !isUp;
            isAnimating = false;
            yield break;
        }

        Vector3 from = faderKnob.localPosition;
        Vector3 to = isUp ? downPosition : downPosition + Vector3.up * slideDistance;
        float elapsed = 0f;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            faderKnob.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }

        faderKnob.localPosition = to;
        isUp = !isUp;
        isAnimating = false;

        if (channelMeter != null)
            channelMeter.SetActive(isUp);
    }
}
