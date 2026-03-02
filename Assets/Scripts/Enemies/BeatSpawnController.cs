using UnityEngine;

public class BeatSpawnController : MonoBehaviour
{
    [SerializeField] private RollerSpawner spawner;
    [SerializeField] private int spawnEveryNBeats = 2;

    private int beatCount;

    void Awake()
    {
        if (spawner == null) spawner = GetComponent<RollerSpawner>();
    }

    void Start()
    {
        SubscribeToAudio();
    }

    void SubscribeToAudio()
    {
        if (AudioSyncManager.instance == null)
        {
            Invoke(nameof(SubscribeToAudio), 0.1f);
            return;
        }
        AudioSyncManager.instance.OnBeat += HandleBeat;
        AudioSyncManager.instance.OnDrop += HandleDrop;
    }

    void OnDestroy()
    {
        if (AudioSyncManager.instance != null)
        {
            AudioSyncManager.instance.OnBeat -= HandleBeat;
            AudioSyncManager.instance.OnDrop -= HandleDrop;
        }
    }

    void HandleBeat()
    {
        beatCount++;
        if (beatCount >= spawnEveryNBeats)
        {
            beatCount = 0;
            spawner?.SpawnOneNow();
        }
    }

    void HandleDrop()
    {
        beatCount = 0;
        spawner?.SpawnOneNow();
    }
}
