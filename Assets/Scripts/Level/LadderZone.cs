using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LadderZone : MonoBehaviour
{
    private BoxCollider2D col;
    
    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        
        // Ensure this is on the Ladder layer
        gameObject.layer = LayerMask.NameToLayer("Ladder");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.EnterLadderZone();
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ExitLadderZone();
            }
        }
    }
    
    void OnDrawGizmos()
    {
        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        if (boxCol != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.offset, boxCol.size);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(boxCol.offset, boxCol.size);
        }
    }
}

