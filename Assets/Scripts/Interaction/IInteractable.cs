/// <summary>
/// Interface for objects the player can interact with.
/// Interactions are visual-only — they never affect audio.
/// </summary>
public interface IInteractable
{
    string GetPromptText();
    void OnInteract();
    bool IsInteractable { get; }
}
