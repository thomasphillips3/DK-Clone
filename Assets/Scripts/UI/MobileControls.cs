using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.OnScreen;

/// <summary>
/// Injects on-screen controls at runtime on Android/iOS.
/// Runner levels (Level01+) get a single Jump button.
/// DK levels get full D-pad + Jump.
/// </summary>
public class MobileControls : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInject()
    {
#if !UNITY_ANDROID && !UNITY_IOS
        return;
#endif
        string scene = SceneManager.GetActiveScene().name;
        if (scene == "Boot" || scene == "MixtapeMap") return;

        if (FindAnyObjectByType<MobileControls>() != null) return;

        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[MobileControls] No Canvas found -- skipping.");
            return;
        }

        var go = new GameObject("MobileControls", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var mc = go.AddComponent<MobileControls>();

        // Runner levels only need Jump
        if (scene.StartsWith("Level"))
            mc.BuildRunnerControls();
        else
            mc.BuildDKControls();
    }

    void BuildRunnerControls()
    {
        var rect = GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Full-screen invisible tap zone — tap anywhere to jump
        var invisible = new Color(0f, 0f, 0f, 0f);
        CreateButton("BtnJump", "<Keyboard>/space",
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2000f, 2000f), invisible);

        Debug.Log("[MobileControls] Runner controls created (jump only).");
    }

    void BuildDKControls()
    {
        var rect = GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0.75f;

        var blue = new Color(0.18f, 0.65f, 1f, 0.85f);
        CreateButton("BtnLeft",  "<Keyboard>/a",     Vector2.zero,        new Vector2( 60f, 185f), new Vector2(110f, 110f), blue);
        CreateButton("BtnRight", "<Keyboard>/d",     Vector2.zero,        new Vector2(310f, 185f), new Vector2(110f, 110f), blue);
        CreateButton("BtnUp",    "<Keyboard>/w",     Vector2.zero,        new Vector2(185f, 310f), new Vector2(110f, 110f), blue);
        CreateButton("BtnDown",  "<Keyboard>/s",     Vector2.zero,        new Vector2(185f,  60f), new Vector2(110f, 110f), blue);

        var orange = new Color(1f, 0.50f, 0.10f, 0.85f);
        CreateButton("BtnJump", "<Keyboard>/space", new Vector2(1f, 0f), new Vector2(-185f, 185f), new Vector2(150f, 150f), orange);

        Debug.Log("[MobileControls] DK controls created.");
    }

    void CreateButton(string name, string controlPath,
        Vector2 anchor, Vector2 anchoredPos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;

        var btn = go.AddComponent<OnScreenButton>();
        btn.controlPath = controlPath;
    }
}
