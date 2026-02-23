using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.4f, 0.1f);
    
    [Header("Ladder")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float ladderAcceleration = 30f;
    
    [Header("Power-Up")]
    [SerializeField] private float wrenchDuration = 10f;
    
    // Components
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    
    // Input
    private Vector2 moveInput;
    private bool jumpPressed;
    
    // State
    private bool isGrounded;
    private bool isClimbing;
    private bool inLadderZone;
    private bool hasPowerUp;
    private float powerUpTimer;
    
    // Jump timing
    private float jumpBufferCounter;
    private float coyoteCounter;
    private float lastGroundedTime;
    
    // Movement
    private float currentSpeed;
    private bool facingRight = true;
    
    // Respawn
    private Vector3 spawnPosition;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spawnPosition = transform.position;
    }
    
    void Update()
    {
        CheckGrounded();
        HandleJumpBufferAndCoyote();
        HandleClimbing();
        UpdatePowerUp();
        UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        if (isClimbing)
        {
            HandleLadderMovement();
        }
        else
        {
            HandleGroundMovement();
            HandleJump();
        }
    }
    
    #region Input Callbacks

    // SendMessages behavior: Unity calls these by name with InputValue
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpPressed = true;
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpPressed = false;

            // Variable jump height - cut jump short if button released
            if (rb != null && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
        }
    }

    #endregion
    
    #region Movement
    
    void HandleGroundMovement()
    {
        float targetSpeed = moveInput.x * moveSpeed;
        float speedDiff = targetSpeed - currentSpeed;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        
        currentSpeed += movement;
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
        
        // Flip sprite
        if (moveInput.x > 0 && !facingRight)
            Flip();
        else if (moveInput.x < 0 && facingRight)
            Flip();
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    #endregion
    
    #region Jump
    
    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
    }
    
    void HandleJumpBufferAndCoyote()
    {
        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;
    }
    
    void HandleJump()
    {
        // Jump if we pressed jump recently and are grounded (or just left ground)
        if (jumpBufferCounter > 0 && coyoteCounter > 0 && !isClimbing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }
    }
    
    #endregion
    
    #region Ladder Climbing
    
    void HandleClimbing()
    {
        // Enter climbing if in ladder zone and moving vertically
        if (inLadderZone && Mathf.Abs(moveInput.y) > 0.1f && !isClimbing)
        {
            isClimbing = true;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            // Phase through platforms while on the ladder
            rb.excludeLayers = groundLayer;
        }
        // Exit climbing if no longer in zone or jumped
        else if (isClimbing && (!inLadderZone || jumpPressed))
        {
            ExitLadder();
        }
    }
    
    void HandleLadderMovement()
    {
        // Smooth ladder climbing
        float targetSpeed = moveInput.y * climbSpeed;
        float currentClimbSpeed = rb.linearVelocity.y;
        float speedDiff = targetSpeed - currentClimbSpeed;
        float movement = speedDiff * ladderAcceleration * Time.fixedDeltaTime;
        
        rb.linearVelocity = new Vector2(0f, currentClimbSpeed + movement);
        
        // Allow small horizontal movement on ladder
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            transform.position += new Vector3(moveInput.x * 0.5f * Time.fixedDeltaTime, 0f, 0f);
        }
    }
    
    void ExitLadder()
    {
        isClimbing = false;
        rb.gravityScale = 1f;
        rb.excludeLayers = 0; // Restore platform collision

        if (jumpPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;
        }
    }
    
    public void EnterLadderZone()
    {
        inLadderZone = true;
    }
    
    public void ExitLadderZone()
    {
        inLadderZone = false;
        if (isClimbing)
        {
            ExitLadder();
        }
    }
    
    #endregion
    
    #region Power-Up
    
    public void ActivateWrench()
    {
        hasPowerUp = true;
        powerUpTimer = wrenchDuration;
    }
    
    void UpdatePowerUp()
    {
        if (hasPowerUp)
        {
            powerUpTimer -= Time.deltaTime;
            if (powerUpTimer <= 0)
            {
                hasPowerUp = false;
            }
        }
    }
    
    public bool HasPowerUp()
    {
        return hasPowerUp;
    }
    
    #endregion
    
    #region Animations
    
    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(currentSpeed));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsClimbing", isClimbing);
            animator.SetFloat("VelocityY", rb.linearVelocity.y);
            animator.SetBool("HasPowerUp", hasPowerUp);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    public void Respawn(Vector3 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
        currentSpeed = 0f;
        isClimbing = false;
        rb.gravityScale = 1f;
        rb.excludeLayers = 0;
    }
    
    public void SetSpawnPosition(Vector3 position)
    {
        spawnPosition = position;
    }
    
    public Vector3 GetSpawnPosition()
    {
        return spawnPosition;
    }
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
    
    #endregion
}

