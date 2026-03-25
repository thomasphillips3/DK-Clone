using UnityEngine;
using System.Collections;

/// <summary>
/// Procedural clutter placement within a room. Grid-divides the floor, uses SeedUtility
/// to deterministically decide what goes in each cell. Runs as a coroutine to avoid spikes.
/// </summary>
public class RoomDetailPlacer : MonoBehaviour
{
    [Header("Room Bounds")]
    [SerializeField] private Vector3 roomMin = new Vector3(-5, 0, -5);
    [SerializeField] private Vector3 roomMax = new Vector3(5, 0, 5);
    [SerializeField] private float cellSize = 1f;

    [Header("Clutter Prefabs")]
    [SerializeField] private GameObject[] clutterPrefabs;
    [SerializeField] private float clutterChance = 0.15f;

    [Header("Procedural Settings")]
    [SerializeField] private int roomSeed = 42;
    [SerializeField] private int itemsPerFrame = 5;

    [Header("Placement")]
    [SerializeField] private LayerMask surfaceLayer;
    [SerializeField] private float maxRayDistance = 5f;

    void Start()
    {
        // Pull seed from room's TrackData if available
        var rm = GetComponentInParent<RoomManager>();
        if (rm != null && rm.TrackData != null)
            roomSeed = rm.TrackData.roomSeed;

        StartCoroutine(PlaceDetails());
    }

    IEnumerator PlaceDetails()
    {
        if (clutterPrefabs == null || clutterPrefabs.Length == 0)
            yield break;

        int placedThisFrame = 0;

        for (float x = roomMin.x; x < roomMax.x; x += cellSize)
        {
            for (float z = roomMin.z; z < roomMax.z; z += cellSize)
            {
                Vector3 cellCenter = new Vector3(
                    x + cellSize * 0.5f,
                    roomMax.y + 1f,
                    z + cellSize * 0.5f
                );

                if (!SeedUtility.ChanceAt(cellCenter, roomSeed, clutterChance))
                    continue;

                // Raycast down to find surface
                if (Physics.Raycast(cellCenter, Vector3.down, out RaycastHit hit, maxRayDistance, surfaceLayer))
                {
                    int prefabIndex = SeedUtility.IntAt(hit.point, roomSeed + 1, clutterPrefabs.Length);
                    float yRotation = SeedUtility.ValueAt(hit.point, roomSeed + 2) * 360f;

                    Instantiate(
                        clutterPrefabs[prefabIndex],
                        hit.point,
                        Quaternion.Euler(0f, yRotation, 0f),
                        transform
                    );

                    placedThisFrame++;
                    if (placedThisFrame >= itemsPerFrame)
                    {
                        placedThisFrame = 0;
                        yield return null;
                    }
                }
            }
        }
    }
}
