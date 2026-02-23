using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool instantKill = false;
    [SerializeField] private bool destroyEnemies = true;
    
    private BoxCollider2D col;
    
    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Handle player
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                if (instantKill)
                {
                    health.InstantKill();
                }
                else
                {
                    health.FallDeath();
                }
            }
        }
        // Destroy enemies that fall
        else if (destroyEnemies && other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Destroy(other.gameObject);
        }
    }
    
    void OnDrawGizmos()
    {
        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        if (boxCol != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.offset, boxCol.size);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCol.offset, boxCol.size);
        }
    }
}

