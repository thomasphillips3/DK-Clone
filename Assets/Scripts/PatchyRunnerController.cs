using UnityEngine;

public class PatchyRunnerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public float jumpForce = 14f;

    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundRadius = 0.2f;

    SpriteRenderer sr;
    Vector3 baseScale;

    // Coyote time: allow jump for a short window after walking off the ground
    const float coyoteTime = 0.18f;
    float coyoteTimer = 0f;

    // Jump buffer: if player presses just before landing, fire the jump on next ground contact
    const float jumpBufferTime = 0.15f;
    float jumpBufferTimer = 0f;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale; // preserve scene scale (e.g. 1.5x)

        // Boost sorting so player draws on top
        if (sr) sr.sortingOrder = 10;
    }

    void Update()
    {
        bool grounded = IsGrounded();

        // --- Coyote timer ---
        if (grounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // --- Detect jump intent ---
        bool pressed = false;

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Touchscreen.current != null)
            pressed = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        if (!pressed && UnityEngine.InputSystem.Mouse.current != null)
            pressed = UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame;
        if (!pressed && UnityEngine.InputSystem.Keyboard.current != null)
            pressed = UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        pressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
#endif

        // --- Jump buffer ---
        if (pressed)
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        // --- Fire jump when buffered input + coyote window ---
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f; // consume so we can't double-jump
        }

        // --- Squash / stretch ---
        if (sr)
        {
            float vy = rb.linearVelocity.y;
            if (!grounded && vy > 1f)
                transform.localScale = Vector3.Scale(baseScale, new Vector3(0.85f, 1.15f, 1f));
            else if (!grounded && vy < -1f)
                transform.localScale = Vector3.Scale(baseScale, new Vector3(1.1f, 0.9f, 1f));
            else
                transform.localScale = baseScale;
        }
    }

    bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask) != null;
    }
}
