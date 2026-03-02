using UnityEngine;

public class CassetteAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform reel1;
    [SerializeField] private RectTransform reel2;

    void Update()
    {
        bool playing = AudioSyncManager.instance?.IsPlaying ?? false;

        if (!playing)
        {
            float idleDelta = 15f * Time.deltaTime;
            if (reel1 != null) reel1.Rotate(0f, 0f, -idleDelta);
            if (reel2 != null) reel2.Rotate(0f, 0f, -idleDelta);
            return;
        }

        TrackLevelData track = MixtapeManager.instance?.GetActiveTrack();
        float bpm = track?.bpm ?? 120f;
        float degreesPerSecond = (bpm / 60f) * 360f;
        float delta = degreesPerSecond * Time.deltaTime;

        if (reel1 != null) reel1.Rotate(0f, 0f, -delta);
        if (reel2 != null) reel2.Rotate(0f, 0f, -delta);
    }
}
