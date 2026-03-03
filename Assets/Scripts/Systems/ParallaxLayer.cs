using UnityEngine;

/// <summary>
/// Infinite horizontal parallax scrolling layer. Scrolls at a fraction of runner speed
/// to create depth (0.1 = far, 0.3 = mid, 0.6 = near). Uses 3 tiles for seamless wrap.
/// Automatically scales tiles to fill the camera viewport height.
/// Parent this to the Main Camera so it follows; tiles scroll in camera-local space.
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] [Range(0f, 1f)] private float scrollSpeedMultiplier = 0.3f;
    [SerializeField] private int sortingOrder = -150;
    [SerializeField] private float verticalOffset = 0f;

    public void SetSprite(Sprite s) => sprite = s;
    public void SetScrollSpeed(float f) => scrollSpeedMultiplier = f;
    public void SetSortingOrder(int o) => sortingOrder = o;
    public void SetVerticalOffset(float y) => verticalOffset = y;

    private RunnerSpeed runnerSpeed;
    private Transform[] tiles;
    private float tileWidth;
    private float offsetX;

    void Start()
    {
        runnerSpeed = FindFirstObjectByType<RunnerSpeed>();
        if (runnerSpeed == null) return;

        if (sprite == null)
        {
            sprite = GetComponentInChildren<SpriteRenderer>()?.sprite;
            if (sprite == null) return;
        }

        if (sprite.bounds.size.x <= 0f) return;

        // Parent to camera so we scroll in view space
        var cam = Camera.main;
        if (cam != null && transform.parent != cam.transform)
        {
            transform.SetParent(cam.transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        CreateTiles();
        offsetX = 0f;
    }

    void CreateTiles()
    {
        var cam = Camera.main;
        float viewportHeight = cam != null ? cam.orthographicSize * 2f : 10f;
        float spriteHeight = sprite.bounds.size.y;
        float scale = spriteHeight > 0f ? viewportHeight / spriteHeight : 1f;

        // Scale uniformly so width scales proportionally
        tileWidth = sprite.bounds.size.x * scale;

        tiles = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            var go = new GameObject($"ParallaxTile_{i}");
            go.transform.SetParent(transform);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            tiles[i] = go.transform;
        }
    }

    void Update()
    {
        if (runnerSpeed == null || tiles == null || tiles.Length == 0) return;

        float speed = runnerSpeed.Current * scrollSpeedMultiplier * Time.deltaTime;
        offsetX -= speed;

        if (tileWidth <= 0f) return;

        while (offsetX < -tileWidth)
            offsetX += tileWidth;
        while (offsetX > 0f)
            offsetX -= tileWidth;

        for (int i = 0; i < tiles.Length; i++)
        {
            float baseX = offsetX + (i - 1) * tileWidth;
            tiles[i].localPosition = new Vector3(baseX, verticalOffset, 10f + sortingOrder * 0.01f);
        }
    }
}
