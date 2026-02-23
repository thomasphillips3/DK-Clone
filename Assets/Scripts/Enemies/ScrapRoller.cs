using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class ScrapRoller : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float rollSpeed = 3f;
    [SerializeField] private float torqueMultiplier = 10f;
    [SerializeField] private bool startMovingRight = true;
    
    [Header("Collision")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.6f;
    
    [Header("Lifetime")]
    [SerializeField] private float maxLifetime = 30f;
    [SerializeField] private bool destroyOnFall = true;
    [SerializeField] private float fallDestroyY = -20f;
    
    // Components
    private Rigidbody2D rb;
    private CircleCollider2D col;
    
    // State
    private float movementDirection;
    private float lifetime;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        
        movementDirection = startMovingRight ? 1f : -1f;
    }
    
    void Start()
    {
        // Apply initial velocity
        rb.linearVelocity = new Vector2(movementDirection * rollSpeed, rb.linearVelocity.y);
    }
    
    void Update()
    {
        // Track lifetime
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }
        
        // Destroy if fallen too far
        if (destroyOnFall && transform.position.y < fallDestroyY)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void FixedUpdate()
    {
        // Maintain constant horizontal speed
        Vector2 velocity = rb.linearVelocity;
        velocity.x = movementDirection * rollSpeed;
        rb.linearVelocity = velocity;
        
        // Apply rolling torque for visual effect
        float targetAngularVelocity = -movementDirection * rollSpeed * torqueMultiplier;
        rb.angularVelocity = targetAngularVelocity;
        
        // Check for walls and reverse direction
        CheckWallCollision();
    }
    
    void CheckWallCollision()
    {
        // Raycast in movement direction to detect walls
        Vector2 checkDirection = movementDirection > 0 ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, checkDirection, wallCheckDistance, wallLayer);
        
        if (hit.collider != null)
        {
            ReverseDirection();
        }
    }
    
    void ReverseDirection()
    {
        movementDirection *= -1f;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Reverse on platform edge or wall collision
        if (collision.gameObject.layer == LayerMask.NameToLayer("Platform") ||
            collision.gameObject.layer == LayerMask.NameToLayer("OneWay"))
        {
            // Check if we hit a wall (contact normal is mostly horizontal)
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.7f)
                {
                    ReverseDirection();
                    break;
                }
            }
        }
    }
    
    public void SetDirection(bool moveRight)
    {
        movementDirection = moveRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(movementDirection * rollSpeed, rb.linearVelocity.y);
    }
    
    public void SetSpeed(float speed)
    {
        rollSpeed = speed;
        rb.linearVelocity = new Vector2(movementDirection * rollSpeed, rb.linearVelocity.y);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 direction = movementDirection > 0 ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(transform.position, direction * wallCheckDistance);
    }
}

