using UnityEngine;

/// <summary>
/// Turntable in the lounge. Interact to drop the needle — platter spins visually.
/// No audio effect — the actual track is managed by AlbumAudioManager.
/// </summary>
public class RecordPlayerInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform platter;
    [SerializeField] private Transform toneArm;
    [SerializeField] private float spinSpeed = 33.33f; // RPM
    [SerializeField] private float armRestAngle = -30f;
    [SerializeField] private float armPlayAngle = 0f;

    private bool isSpinning;
    private bool isAnimating;

    public bool IsInteractable => !isAnimating;

    public string GetPromptText() => isSpinning ? "Lift needle" : "Drop needle";

    public void OnInteract()
    {
        if (isAnimating) return;
        isAnimating = true;
        isSpinning = !isSpinning;
        StartCoroutine(AnimateArm());
    }

    System.Collections.IEnumerator AnimateArm()
    {
        if (toneArm == null)
        {
            isAnimating = false;
            yield break;
        }

        float fromAngle = isSpinning ? armRestAngle : armPlayAngle;
        float toAngle = isSpinning ? armPlayAngle : armRestAngle;
        float elapsed = 0f;
        float duration = 0.8f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            float angle = Mathf.Lerp(fromAngle, toAngle, t);
            toneArm.localRotation = Quaternion.Euler(0f, angle, 0f);
            yield return null;
        }

        isAnimating = false;
    }

    void Update()
    {
        if (isSpinning && platter != null)
            platter.Rotate(Vector3.up, spinSpeed * 6f * Time.deltaTime); // degrees per second
    }
}
