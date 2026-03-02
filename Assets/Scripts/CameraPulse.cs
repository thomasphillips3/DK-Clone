using UnityEngine;

public class CameraPulse : MonoBehaviour
{
    Vector3 basePos;
    float strength;

    void Start()
    {
        basePos = transform.position;
    }

    public void Bump(float s)
    {
        strength = Mathf.Clamp(strength + s, 0f, 0.5f);
    }

    void Update()
    {
        strength = Mathf.Lerp(strength, 0f, Time.deltaTime * 6f);
        transform.position = basePos + (Vector3)(Random.insideUnitCircle * strength * 0.1f);
    }
}
