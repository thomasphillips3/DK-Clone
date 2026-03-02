using UnityEngine;

public class RunnerSpeed : MonoBehaviour
{
    public float baseSpeed = 4f;
    public float multiplier = 1f;

    public float Current => baseSpeed * multiplier;

    public void Nudge(float amount)
        => multiplier = Mathf.Clamp(multiplier + amount, 0.8f, 2.0f);

    public void SetMultiplier(float m)
        => multiplier = Mathf.Clamp(m, 0.8f, 1.6f);
}
