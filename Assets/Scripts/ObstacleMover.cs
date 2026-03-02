using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public RunnerSpeed speed;
    public float destroyX = -20f;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!speed || !rb) return;

        Vector2 newPos = rb.position + Vector2.left * speed.Current * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        if (newPos.x < destroyX)
            Destroy(gameObject);
    }
}
