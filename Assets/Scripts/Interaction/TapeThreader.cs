using UnityEngine;

/// <summary>
/// Tape machine in the tape room. Interact to thread tape — reels spin visually.
/// No audio effect.
/// </summary>
public class TapeThreader : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform supplyReel;
    [SerializeField] private Transform takeupReel;
    [SerializeField] private float reelSpeed = 60f; // degrees per second

    private bool isThreaded;
    private bool isAnimating;

    public bool IsInteractable => !isAnimating;

    public string GetPromptText() => isThreaded ? "Stop tape" : "Thread tape";

    public void OnInteract()
    {
        if (isAnimating) return;
        isThreaded = !isThreaded;
    }

    void Update()
    {
        if (!isThreaded) return;

        float rotation = reelSpeed * Time.deltaTime;
        if (supplyReel != null)
            supplyReel.Rotate(Vector3.forward, -rotation);
        if (takeupReel != null)
            takeupReel.Rotate(Vector3.forward, rotation);
    }
}
