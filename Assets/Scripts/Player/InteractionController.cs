using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Raycast-based interaction with IInteractable objects.
/// Shows subtle UI prompt when looking at something interactive.
/// </summary>
public class InteractionController : MonoBehaviour
{
    [SerializeField] private float interactRange = 2.5f;
    [SerializeField] private LayerMask interactLayer = ~0;

    private Camera playerCamera;
    private IInteractable currentTarget;

    public IInteractable CurrentTarget => currentTarget;
    public bool HasTarget => currentTarget != null && currentTarget.IsInteractable;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null && interactable.IsInteractable)
            {
                currentTarget = interactable;
                return;
            }
        }

        currentTarget = null;
    }

    // Input System callback
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;
        if (currentTarget != null && currentTarget.IsInteractable)
            currentTarget.OnInteract();
    }
}
