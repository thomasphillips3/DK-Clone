using UnityEngine;

/// <summary>
/// Cable lying on floor near a patch bay. Interact to plug it in — a VU meter lights up.
/// Visual only — no audio effect.
/// </summary>
public class CablePlug : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform cableEnd;
    [SerializeField] private Transform plugTarget;
    [SerializeField] private GameObject vuMeterLight;
    [SerializeField] private float plugSpeed = 2f;

    private bool isPlugged;
    private bool isAnimating;

    public bool IsInteractable => !isPlugged && !isAnimating;

    public string GetPromptText() => "Plug in cable";

    public void OnInteract()
    {
        if (isPlugged || isAnimating) return;
        isAnimating = true;
        StartCoroutine(AnimatePlug());
    }

    System.Collections.IEnumerator AnimatePlug()
    {
        if (cableEnd == null || plugTarget == null)
        {
            isPlugged = true;
            isAnimating = false;
            if (vuMeterLight != null) vuMeterLight.SetActive(true);
            yield break;
        }

        Vector3 startPos = cableEnd.position;
        Vector3 endPos = plugTarget.position;
        Quaternion startRot = cableEnd.rotation;
        Quaternion endRot = plugTarget.rotation;
        float elapsed = 0f;
        float duration = 1f / plugSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            cableEnd.position = Vector3.Lerp(startPos, endPos, t);
            cableEnd.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        cableEnd.position = endPos;
        cableEnd.rotation = endRot;
        isPlugged = true;
        isAnimating = false;

        if (vuMeterLight != null)
            vuMeterLight.SetActive(true);
    }
}
