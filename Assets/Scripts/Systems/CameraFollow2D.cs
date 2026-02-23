using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool autoFindPlayer = true;
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector2 offset = Vector2.zero;
    
    [Header("Dead Zone")]
    [SerializeField] private bool useDeadZone = true;
    [SerializeField] private Vector2 deadZoneSize = new Vector2(2f, 1f);
    
    [Header("Bounds")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(10f, 15f);
    
    [Header("Camera Settings")]
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 2f;
    
    // Internal state
    private Vector3 currentVelocity;
    private float currentLookAhead;
    private Camera cam;
    private Vector3 deadZoneCenter;
    
    void Awake()
    {
        cam = GetComponent<Camera>();
    }
    
    void Start()
    {
        if (autoFindPlayer && target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        if (target != null)
        {
            deadZoneCenter = target.position;
        }
    }
    
    void LateUpdate()
    {
        if (target == null)
            return;
        
        Vector3 desiredPosition = CalculateDesiredPosition();
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);
        
        // Keep Z position for camera
        smoothedPosition.z = transform.position.z;
        
        // Apply bounds
        if (useBounds)
        {
            smoothedPosition = ApplyBounds(smoothedPosition);
        }
        
        transform.position = smoothedPosition;
    }
    
    Vector3 CalculateDesiredPosition()
    {
        Vector3 targetPos = target.position + (Vector3)offset;
        Vector3 currentPos = transform.position;
        
        // Calculate look ahead based on target velocity
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null && lookAheadDistance > 0f)
        {
            float targetLookAhead = Mathf.Sign(targetRb.linearVelocity.x) * lookAheadDistance;
            currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSpeed * Time.deltaTime);
            targetPos.x += currentLookAhead;
        }
        
        // Dead zone logic
        if (useDeadZone)
        {
            Vector2 delta = (Vector2)targetPos - (Vector2)deadZoneCenter;
            
            // Only move if target is outside dead zone
            if (followX && Mathf.Abs(delta.x) > deadZoneSize.x / 2f)
            {
                deadZoneCenter.x = targetPos.x - Mathf.Sign(delta.x) * deadZoneSize.x / 2f;
            }
            
            if (followY && Mathf.Abs(delta.y) > deadZoneSize.y / 2f)
            {
                deadZoneCenter.y = targetPos.y - Mathf.Sign(delta.y) * deadZoneSize.y / 2f;
            }
            
            return deadZoneCenter;
        }
        else
        {
            // Direct follow
            Vector3 result = currentPos;
            
            if (followX)
                result.x = targetPos.x;
            
            if (followY)
                result.y = targetPos.y;
            
            return result;
        }
    }
    
    Vector3 ApplyBounds(Vector3 position)
    {
        // Calculate camera bounds based on orthographic size
        float cameraHeight = cam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * cam.aspect;
        
        float minX = minBounds.x + cameraWidth / 2f;
        float maxX = maxBounds.x - cameraWidth / 2f;
        float minY = minBounds.y + cameraHeight / 2f;
        float maxY = maxBounds.y - cameraHeight / 2f;
        
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        
        return position;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            deadZoneCenter = target.position;
        }
    }
    
    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
    }
    
    public void SnapToTarget()
    {
        if (target != null)
        {
            Vector3 targetPos = target.position + (Vector3)offset;
            targetPos.z = transform.position.z;
            transform.position = targetPos;
            deadZoneCenter = targetPos;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (useDeadZone)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = Application.isPlaying ? deadZoneCenter : transform.position;
            Gizmos.DrawWireCube(center, deadZoneSize);
        }
        
        if (useBounds)
        {
            Gizmos.color = Color.red;
            Vector3 boundsCenter = new Vector3(
                (minBounds.x + maxBounds.x) / 2f,
                (minBounds.y + maxBounds.y) / 2f,
                transform.position.z
            );
            Vector3 boundsSize = new Vector3(
                maxBounds.x - minBounds.x,
                maxBounds.y - minBounds.y,
                0f
            );
            Gizmos.DrawWireCube(boundsCenter, boundsSize);
        }
    }
}

