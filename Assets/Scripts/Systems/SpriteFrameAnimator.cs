using UnityEngine;

/// <summary>
/// Lightweight frame-based sprite animator. Cycles through sprite frames at given FPS.
/// Use 1 frame for static; multiple frames for run cycles, etc. No Animator needed.
/// </summary>
public class SpriteFrameAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 8f;

    private SpriteRenderer sr;
    private float timer;
    private int index;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (frames != null && frames.Length > 0 && sr != null)
            sr.sprite = frames[0];
    }

    void Update()
    {
        if (frames == null || frames.Length <= 1 || sr == null || fps <= 0f) return;

        timer += Time.deltaTime;
        float frameTime = 1f / fps;
        while (timer >= frameTime)
        {
            timer -= frameTime;
            index = (index + 1) % frames.Length;
            sr.sprite = frames[index];
        }
    }

    public void SetFrames(Sprite[] f)
    {
        frames = f;
        index = 0;
        timer = 0f;
        if (frames != null && frames.Length > 0 && sr != null)
            sr.sprite = frames[0];
    }
}
