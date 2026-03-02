using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public RunnerSpeed speed;
    public Transform spawnPoint;

    public GameObject[] basicPrefabs;
    public GameObject[] accentPrefabs;

    int pattern = 0;

    public void SetPattern(int p) => pattern = p;

    public void SpawnBasic()
    {
        SpawnFrom(basicPrefabs);

        // Occasionally double-spawn on higher patterns
        if (pattern >= 3 && Random.value < 0.10f)
            SpawnFrom(basicPrefabs);
    }

    public void SpawnAccent()
    {
        SpawnFrom(accentPrefabs);
    }

    void SpawnFrom(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0 || !spawnPoint) return;

        var prefab = prefabs[Random.Range(0, prefabs.Length)];
        var go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        var mover = go.GetComponent<ObstacleMover>();
        if (mover) mover.speed = speed;
    }
}
