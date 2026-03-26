using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Fixes the Menu scene: CanvasScaler for portrait mobile, EventSystem for touch,
/// button press states, touch target sizes, atmosphere, and splash screen.
/// Run from Tools > Fix Menu Scene.
/// </summary>
public static class MenuSceneFixer
{
    [MenuItem("Tools/Fix Menu Scene")]
    public static void FixMenuScene()
    {
        // Load the Menu scene
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity", OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();

        Canvas canvas = null;
        EventSystem eventSystem = null;
        Camera mainCam = null;

        foreach (var root in roots)
        {
            if (root.GetComponent<Canvas>() != null) canvas = root.GetComponent<Canvas>();
            if (root.GetComponent<EventSystem>() != null) eventSystem = root.GetComponent<EventSystem>();
            if (root.GetComponent<Camera>() != null) mainCam = root.GetComponent<Camera>();
        }

        // ──────────────────────────────────────────────────
        // 1. Fix CanvasScaler — portrait mobile (1080x1920)
        // ──────────────────────────────────────────────────
        if (canvas != null)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                Debug.Log("[MenuFixer] CanvasScaler fixed: 1080x1920 portrait, match 0.5");
            }
        }

        // ──────────────────────────────────────────────────
        // 2. Fix EventSystem — ensure StandaloneInputModule for reliable touch
        // ──────────────────────────────────────────────────
        if (eventSystem != null)
        {
            // InputSystemUIInputModule should work with touch, but let's ensure
            // it's properly configured. The key issue: Unity's Input System UI module
            // handles Touchscreen by default when the pointer action uses auto-routing.
            var isim = eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (isim != null)
            {
                // Force pointer behavior to "All Pointers At Once" so touch works alongside mouse
                isim.pointerBehavior = UnityEngine.InputSystem.UI.UIPointerBehavior.AllPointersAsIs;
                Debug.Log("[MenuFixer] InputSystemUIInputModule pointer behavior set to AllPointersAsIs");
            }
        }

        // ──────────────────────────────────────────────────
        // 3. Fix Camera background to warm near-black
        // ──────────────────────────────────────────────────
        if (mainCam != null)
        {
            mainCam.backgroundColor = new Color(0.04f, 0.03f, 0.02f, 1f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
        }

        // ──────────────────────────────────────────────────
        // 4. Fix all Button components — add press color transitions, min touch target
        // ──────────────────────────────────────────────────
        var allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            // Set color block for press feedback
            var colors = btn.colors;
            colors.normalColor = new Color(0.15f, 0.12f, 0.08f, 0.9f);
            colors.highlightedColor = new Color(0.25f, 0.2f, 0.12f, 1f);
            colors.pressedColor = new Color(0.5f, 0.35f, 0.15f, 1f);
            colors.selectedColor = new Color(0.3f, 0.25f, 0.15f, 1f);
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            // Ensure minimum touch target height of 48dp (roughly 48 units at our reference)
            var rt = btn.GetComponent<RectTransform>();
            if (rt != null && rt.sizeDelta.y < 52f)
            {
                var size = rt.sizeDelta;
                size.y = 52f;
                rt.sizeDelta = size;
            }
        }
        Debug.Log($"[MenuFixer] Fixed {allButtons.Length} buttons with press states and touch targets");

        // ──────────────────────────────────────────────────
        // 5. Add dust particle atmosphere to menu background
        // ──────────────────────────────────────────────────
        if (GameObject.Find("MenuDust") == null)
        {
            var dustGO = new GameObject("MenuDust");
            dustGO.transform.position = new Vector3(0, 0, 5);
            var ps = dustGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(1f, 0.9f, 0.7f, 0.15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.05f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f);
            main.maxParticles = 200;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 30f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(10, 6, 2);

            var renderer = dustGO.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.SetColor("_Color", new Color(1f, 0.9f, 0.7f, 0.15f));

            Debug.Log("[MenuFixer] Added dust particle atmosphere");
        }

        // ──────────────────────────────────────────────────
        // 6. Fix credits text — larger, more visible
        // ──────────────────────────────────────────────────
        var allTMP = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var tmp in allTMP)
        {
            if (tmp.text.Contains("Design & Engineering"))
            {
                tmp.fontSize = 14f;
                tmp.alpha = 0.5f;
            }
        }

        // ──────────────────────────────────────────────────
        // Save
        // ──────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[MenuFixer] Menu scene saved with all fixes applied");
    }
}
