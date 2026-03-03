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

    float stumbleTimer;
    const float stumbleDuration = 0.25f;

    // Coyote time: allow jump for a short window after walking off the ground
    const float coyoteTime = 0.18f;
    float coyoteTimer = 0f;

    // Jump buffer: if player presses just before landing, fire the jump on next ground contact
    const float jumpBufferTime = 0.15f;
    float jumpBufferTimer = 0f;

    // Landing squash
    bool wasGrounded;
    float landSquashTimer;
    const float landSquashDuration = 0.12f;

    // Smooth scale
    Vector3 targetScale;

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

        // --- Landing detection ---
        if (grounded && !wasGrounded)
            landSquashTimer = landSquashDuration;
        wasGrounded = grounded;

        if (landSquashTimer > 0f)
            landSquashTimer -= Time.deltaTime;

        // --- Squash / stretch (or stumble) ---
        Vector3 scaleMultiplier = Vector3.one;
        if (stumbleTimer > 0f)
        {
            stumbleTimer -= Time.deltaTime;
            scaleMultiplier = new Vector3(1.2f, 0.6f, 1f);
        }
        else if (landSquashTimer > 0f)
        {
            float t = landSquashTimer / landSquashDuration;
            scaleMultiplier = Vector3.Lerp(Vector3.one, new Vector3(1.2f, 0.8f, 1f), t);
        }
        else if (sr)
        {
            float vy = rb.linearVelocity.y;
            if (!grounded && vy > 1f)
                scaleMultiplier = new Vector3(0.85f, 1.15f, 1f);
            else if (!grounded && vy < -1f)
                scaleMultiplier = new Vector3(1.1f, 0.9f, 1f);
        }

        targetScale = Vector3.Scale(baseScale, scaleMultiplier);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 15f);
    }

    public void Stumble()
    {
        stumbleTimer = stumbleDuration;
    }

    public bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask) != null;
    }
}
