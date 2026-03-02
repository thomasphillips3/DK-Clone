using UnityEngine;

public class RunnerGameManager : MonoBehaviour
{
    public BeatScheduler scheduler;
    public ObstacleSpawner spawner;
    public RunnerSpeed speed;
    public CameraPulse cameraPulse;
    public ScoreManager score;

    void Awake()
    {
        if (scheduler != null)
            scheduler.OnEvent += HandleBeatEvent;
    }

    void HandleBeatEvent(BeatEvent e)
    {
        switch (e.type)
        {
            case BeatEventType.Beat:
            case BeatEventType.Kick:
                // Only spawn on ~40% of beats to keep it playable
                if (Random.value < 0.4f)
                    spawner.SpawnBasic();
                speed.Nudge(0.005f);
                score?.Add(1);
                break;

            case BeatEventType.Snare:
                cameraPulse?.Bump(0.15f);
                if (Random.value < 0.6f)
                    spawner.SpawnAccent();
                score?.Add(3);
                break;

            case BeatEventType.Section:
                speed.SetMultiplier(1f + (0.05f * e.value));
                spawner.SetPattern(e.value);
                break;
        }
    }
}
