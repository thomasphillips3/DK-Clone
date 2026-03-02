using UnityEngine;

public class DeathRewindHandler : MonoBehaviour
{
    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null) playerHealth.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnDeath -= HandleDeath;
    }

    void HandleDeath()
    {
        if (AudioSyncManager.instance != null)
            AudioSyncManager.instance.RewindOnDeath();

        if (ChromaticAberrationFlash.instance != null)
            ChromaticAberrationFlash.instance.TriggerFlash();

        HapticManager.Death();
    }
}
