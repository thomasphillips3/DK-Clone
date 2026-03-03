using UnityEngine;

/// <summary>
/// Animates Patchy in the runner level: run sprite when grounded, jump sprite when airborne.
/// Requires PatchyRunnerController for grounded state.
/// </summary>
public class PatchyRunnerAnimator : MonoBehaviour
{
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite jumpSprite;
    [SerializeField] private Sprite[] runFrames;
    [SerializeField] private float framesPerSecond = 10f;

    private SpriteRenderer sr;
    private PatchyRunnerController runner;
    private float frameTimer;
    private int currentFrame;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        runner = GetComponent<PatchyRunnerController>();
    }

    void Start()
    {
        if (idleSprite != null && sr != null)
            sr.sprite = idleSprite;
    }

    void Update()
    {
        if (sr == null) return;

        bool grounded = runner != null && runner.IsGrounded();

        if (grounded && runFrames != null && runFrames.Length > 0)
        {
            // Cycle through run frames
            frameTimer += Time.deltaTime;
            float frameDuration = 1f / framesPerSecond;
            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                currentFrame = (currentFrame + 1) % runFrames.Length;
            }

            if (runFrames[currentFrame] != null)
                sr.sprite = runFrames[currentFrame];
        }
        else if (!grounded)
        {
            // Use jump sprite when airborne, fall back to idle
            Sprite airSprite = jumpSprite != null ? jumpSprite : idleSprite;
            if (airSprite != null)
                sr.sprite = airSprite;
            // Reset animation to first frame for next ground contact
            currentFrame = 0;
            frameTimer = 0f;
        }
    }

    public void SetSprites(Sprite idle, Sprite[] run, Sprite jump = null)
    {
        idleSprite = idle;
        runFrames = run;
        jumpSprite = jump;
        currentFrame = 0;
        frameTimer = 0f;
        if (idleSprite != null && sr != null)
            sr.sprite = idleSprite;
    }
}
