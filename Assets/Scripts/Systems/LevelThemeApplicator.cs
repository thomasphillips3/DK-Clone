using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelThemeApplicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private TrackLevelData fallbackTrack;

    void Start()
    {
        TrackLevelData track = MixtapeManager.instance != null
            ? MixtapeManager.instance.GetActiveTrack()
            : null;
        if (track == null)
            track = fallbackTrack;
        if (track == null) return;

        // Apply visual theme
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = track.primaryColor * 0.12f;
            Camera.main.orthographicSize = 5f;
        }

        SetupParallaxLayers(track);
        ApplyGroundTiles();
        ApplyPatchySprite();

        ApplyColorToTag("Platform", track.primaryColor);
        ApplyColorToTag("Ladder", track.accentColor);

        // Load and start audio for this level (only when MixtapeManager exists)
        if (AudioSyncManager.instance != null && MixtapeManager.instance != null)
        {
            float resumeTime = 0f;
            int idx = MixtapeManager.instance.GetActiveTrackIndex();
            TrackSaveData save = MixtapeManager.instance.GetSaveData(idx);
            // Resume from last position if not at start
            resumeTime = (save.lastTimestamp > 1f) ? save.lastTimestamp : 0f;

            AudioSyncManager.instance.LoadTrack(track, resumeTime);
            AudioSyncManager.instance.Play();
        }
    }

    void SetupParallaxLayers(TrackLevelData track)
    {
        var cam = Camera.main;
        if (cam == null) return;

        // Solid background behind everything
        CreateSolidBackground(track);

        // Atmospheric gradient layer behind the equipment
        CreateGradientBackground(track);

        var farSprite = LoadSprite("Level01/Parallax_Far");
        var midSprite = LoadSprite("Level01/Parallax_Mid");
        var nearSprite = LoadSprite("Level01/Parallax_Near");
        if (farSprite == null && midSprite == null && nearSprite == null) return;

        if (farSprite != null)
            CreateParallaxLayer("ParallaxFar", farSprite, 0.05f, -200);
        if (midSprite != null)
            CreateParallaxLayer("ParallaxMid", midSprite, 0.15f, -180);
        if (nearSprite != null)
            CreateParallaxLayer("ParallaxNear", nearSprite, 0.35f, -160);
    }

    void CreateSolidBackground(TrackLevelData track)
    {
        if (track == null) return;

        var tex = Texture2D.whiteTexture;
        if (tex == null) return;
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
        if (sprite == null) return;

        var go = new GameObject("ParallaxSolidBackground");
        go.transform.SetParent(Camera.main.transform);
        go.transform.localPosition = new Vector3(0f, 0f, 50f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = new Vector3(200f, 200f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = track.primaryColor * 0.10f;
        sr.sortingOrder = -300;
    }

    void CreateGradientBackground(TrackLevelData track)
    {
        if (track == null) return;

        // Create a vertical gradient texture for atmosphere
        int w = 2, h = 64;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color topColor = track.secondaryColor * 0.18f;
        Color bottomColor = track.primaryColor * 0.22f;
        topColor.a = 1f;
        bottomColor.a = 1f;

        for (int y = 0; y < h; y++)
        {
            float t = (float)y / (h - 1);
            Color c = Color.Lerp(bottomColor, topColor, t);
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, c);
        }
        tex.Apply();

        var cam = Camera.main;
        float viewH = cam.orthographicSize * 2f;
        float viewW = viewH * cam.aspect;
        var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), h / viewH);

        var go = new GameObject("GradientBackground");
        go.transform.SetParent(cam.transform);
        go.transform.localPosition = new Vector3(0f, 0f, 40f);
        go.transform.localRotation = Quaternion.identity;
        // Scale width to cover viewport
        float spriteW = sprite.bounds.size.x;
        float scaleX = spriteW > 0 ? (viewW * 1.5f) / spriteW : 1f;
        go.transform.localScale = new Vector3(scaleX, 1f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -220;
    }

    void CreateParallaxLayer(string name, Sprite sprite, float speed, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(Camera.main.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        var layer = go.AddComponent<ParallaxLayer>();
        layer.SetSprite(sprite);
        layer.SetScrollSpeed(speed);
        layer.SetSortingOrder(order);
    }

    void ApplyGroundTiles()
    {
        var ground = GameObject.Find("Ground");
        if (ground == null) return;

        var floorSprite = LoadSprite("Level01/Ground_StudioFloor");
        if (floorSprite == null) return;

        float tileWidth = floorSprite.bounds.size.x;
        if (tileWidth <= 0f) return;

        float groundY = ground.transform.position.y;
        int count = Mathf.CeilToInt(100f / tileWidth) + 1;
        float startX = -(count - 1) * tileWidth * 0.5f;

        var parent = new GameObject("GroundTiles");
        parent.transform.position = new Vector3(0f, groundY, 0f);

        // Disable Ground's SpriteRenderer immediately to avoid any visible frame
        var mainSr = ground.GetComponent<SpriteRenderer>();
        if (mainSr != null) mainSr.enabled = false;

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("GroundTile");
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = new Vector3(startX + i * tileWidth, 0f, 0f);
            go.transform.localScale = Vector3.one;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = floorSprite;
            sr.sortingOrder = -50;
        }
    }

    void ApplyPatchySprite()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var runnerController = player.GetComponent<PatchyRunnerController>();
        if (runnerController == null) return; // Only apply for runner levels (Level01)

        var idleSprite = LoadSprite("Level01/Patchy_Idle", 289f);
        var run1 = LoadSprite("Level01/Patchy_Run_1", 289f);
        var run2 = LoadSprite("Level01/Patchy_Run_2", 289f);
        var run3 = LoadSprite("Level01/Patchy_Run_3", 289f);
        var run4 = LoadSprite("Level01/Patchy_Run_4", 289f);
        var jumpSprite = LoadSprite("Level01/Patchy_Jump", 289f);

        if (idleSprite == null)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning("[LevelThemeApplicator] Patchy_Idle failed to load - keeping prefab sprite.");
#endif
            return;
        }

        var sr = player.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = idleSprite;

        Sprite[] runFrames = null;
        if (run1 != null && run2 != null && run3 != null && run4 != null)
            runFrames = new Sprite[] { run1, run2, run3, run4 };
        else if (run1 != null)
            runFrames = new Sprite[] { run1 };

        var animator = player.GetComponent<PatchyRunnerAnimator>();
        if (animator == null) animator = player.AddComponent<PatchyRunnerAnimator>();
        animator.SetSprites(idleSprite, runFrames, jumpSprite);
    }

    Sprite LoadSprite(string path, float fallbackPixelsPerUnit = 100f)
    {
        var s = Resources.Load<Sprite>(path);
        if (s != null) return s;
        var tex = Resources.Load<Texture2D>(path);
        if (tex != null)
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), fallbackPixelsPerUnit);
        return null;
    }

    void ApplyColorToTag(string tag, Color color)
    {
        try
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_Color", color);
            foreach (var obj in objects)
            {
                Renderer r = obj.GetComponent<Renderer>();
                if (r != null) r.SetPropertyBlock(block);
            }
        }
        catch { /* tag not registered — safe to ignore */ }
    }
}
