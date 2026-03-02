using UnityEngine;

public class BeatScheduler : MonoBehaviour
{
    public AudioSource source;
    public BeatMap beatMap;
    public float startDelay = 2f; // grace period before music starts

    public System.Action<BeatEvent> OnEvent;

    int idx;
    float delayTimer;
    bool started;

    void Start()
    {
        idx = 0;
        delayTimer = startDelay;

        // Ensure AudioSource has the clip from BeatMap
        if (source && beatMap && beatMap.clip)
        {
            source.clip = beatMap.clip;
        }
    }

    void Update()
    {
        // Grace period before starting
        if (!started)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer > 0f) return;

            started = true;
            if (source && !source.isPlaying)
                source.Play();
        }

        if (!source || !source.isPlaying || beatMap == null) return;

        float t = source.time;

        // Catch up if frames skip
        while (idx < beatMap.events.Count && beatMap.events[idx].time <= t)
        {
            OnEvent?.Invoke(beatMap.events[idx]);
            idx++;
        }
    }

    public void ResetSchedule()
    {
        idx = 0;
    }
}
