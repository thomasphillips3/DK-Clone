using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates atmosphere prefabs (dust particles) and in-game UI Canvas prefab,
/// then deploys them to all 8 room scenes.
/// </summary>
public static class AtmosphereAndUISetup
{
    [MenuItem("Tools/Setup Atmosphere And UI")]
    public static void Setup()
    {
        CreateDustPrefab();
        CreateInGameUICanvasPrefab();
        DeployToAllRooms();
    }

    static void CreateDustPrefab()
    {
        string dir = "Assets/Prefabs/Atmosphere";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Atmosphere");

        var dustGO = new GameObject("DustParticles");
        var ps = dustGO.AddComponent<ParticleSystem>();

        // Configure particle system for ambient dust
        var main = ps.main;
        main.startLifetime = 12f;
        main.startSpeed = 0.02f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.005f, 0.025f);
        main.startColor = new Color(0.9f, 0.85f, 0.75f, 0.15f);
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.001f; // slight upward drift

        var emission = ps.emission;
        emission.rateOverTime = 10f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(8, 3, 6); // room-sized volume
        shape.position = new Vector3(0, 1.5f, 0);

        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.01f, 0.01f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.005f, 0.005f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.01f, 0.01f);

        var renderer = dustGO.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        // Add DustParticleController
        dustGO.AddComponent<DustParticleController>();

        PrefabUtility.SaveAsPrefabAsset(dustGO, dir + "/DustParticles.prefab");
        Object.DestroyImmediate(dustGO);
        Debug.Log("[AtmosphereAndUI] DustParticles prefab created.");
    }

    static void CreateInGameUICanvasPrefab()
    {
        string dir = "Assets/Prefabs/UI";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        // Canvas root
        var canvasGO = new GameObject("InGameCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- NowPlayingHUD ---
        var hudGO = new GameObject("NowPlayingHUD");
        hudGO.transform.SetParent(canvasGO.transform, false);
        var hudRT = hudGO.AddComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0, 1);
        hudRT.anchorMax = new Vector2(0, 1);
        hudRT.pivot = new Vector2(0, 1);
        hudRT.anchoredPosition = new Vector2(40, -30);
        hudRT.sizeDelta = new Vector2(500, 80);
        var hudCG = hudGO.AddComponent<CanvasGroup>();
        hudCG.alpha = 0; // starts hidden, fades in on track change

        var trackTitleGO = CreateText("TrackTitle", hudGO.transform, "", 32,
            new Color(1, 0.91f, 0.82f), TextAlignmentOptions.BottomLeft,
            new Vector2(0, 0), new Vector2(500, 45));
        var artistTextGO = CreateText("ArtistText", hudGO.transform, "", 18,
            new Color(0.7f, 0.6f, 0.5f), TextAlignmentOptions.TopLeft,
            new Vector2(0, -45), new Vector2(500, 25));

        // Wire NowPlayingHUD component
        var hudComp = hudGO.AddComponent<NowPlayingHUD>();
        var hudSO = new SerializedObject(hudComp);
        hudSO.FindProperty("trackTitleText").objectReferenceValue = trackTitleGO.GetComponent<TextMeshProUGUI>();
        hudSO.FindProperty("artistText").objectReferenceValue = artistTextGO.GetComponent<TextMeshProUGUI>();
        hudSO.ApplyModifiedProperties();

        // --- InteractionPrompt ---
        var promptGO = new GameObject("InteractionPrompt");
        promptGO.transform.SetParent(canvasGO.transform, false);
        var promptRT = promptGO.AddComponent<RectTransform>();
        promptRT.anchorMin = new Vector2(0.5f, 0);
        promptRT.anchorMax = new Vector2(0.5f, 0);
        promptRT.pivot = new Vector2(0.5f, 0);
        promptRT.anchoredPosition = new Vector2(0, 60);
        promptRT.sizeDelta = new Vector2(400, 40);
        var promptCG = promptGO.AddComponent<CanvasGroup>();
        promptCG.alpha = 0;

        var promptTextGO = CreateText("PromptText", promptGO.transform, "[E] Interact", 22,
            new Color(1, 0.91f, 0.82f, 0.9f), TextAlignmentOptions.Center,
            Vector2.zero, new Vector2(400, 40));

        var promptComp = promptGO.AddComponent<InteractionPrompt>();
        var promptSO = new SerializedObject(promptComp);
        promptSO.FindProperty("promptText").objectReferenceValue = promptTextGO.GetComponent<TextMeshProUGUI>();
        promptSO.ApplyModifiedProperties();

        // --- PauseMenu (initially inactive) ---
        var pauseGO = new GameObject("PauseMenu");
        pauseGO.transform.SetParent(canvasGO.transform, false);
        var pauseRT = pauseGO.AddComponent<RectTransform>();
        pauseRT.anchorMin = Vector2.zero;
        pauseRT.anchorMax = Vector2.one;
        pauseRT.offsetMin = Vector2.zero;
        pauseRT.offsetMax = Vector2.zero;

        var pauseBG = pauseGO.AddComponent<Image>();
        pauseBG.color = new Color(0.03f, 0.02f, 0.01f, 0.85f);

        // Pause title
        CreateText("PauseTitle", pauseGO.transform, "PAUSED", 48,
            new Color(1, 0.91f, 0.82f), TextAlignmentOptions.Center,
            new Vector2(0, 100), new Vector2(400, 60));

        // Resume button
        var resumeBtn = CreateButton("ResumeButton", pauseGO.transform, "RESUME",
            new Vector2(0, 0), new Color(0.4f, 0.5f, 0.3f));

        // Menu button
        var menuBtn = CreateButton("MenuButton", pauseGO.transform, "BACK TO MENU",
            new Vector2(0, -70), new Color(0.5f, 0.3f, 0.3f));

        // Wire PauseMenu component
        var pauseComp = pauseGO.AddComponent<PauseMenu>();
        var pauseSO = new SerializedObject(pauseComp);
        pauseSO.FindProperty("resumeButton").objectReferenceValue = resumeBtn.GetComponent<Button>();
        pauseSO.FindProperty("menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        pauseSO.ApplyModifiedProperties();

        pauseGO.SetActive(false); // hidden by default

        // --- Crosshair dot ---
        var dotGO = new GameObject("Crosshair");
        dotGO.transform.SetParent(canvasGO.transform, false);
        var dotRT = dotGO.AddComponent<RectTransform>();
        dotRT.anchoredPosition = Vector2.zero;
        dotRT.sizeDelta = new Vector2(4, 4);
        var dotImg = dotGO.AddComponent<Image>();
        dotImg.color = new Color(1, 1, 1, 0.4f);

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(canvasGO, dir + "/InGameCanvas.prefab");
        Object.DestroyImmediate(canvasGO);
        Debug.Log("[AtmosphereAndUI] InGameCanvas prefab created.");
    }

    static void DeployToAllRooms()
    {
        var dustPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Atmosphere/DustParticles.prefab");
        var uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/InGameCanvas.prefab");

        string[] roomScenes = {
            "Room_01_LiveRoom", "Room_02_ControlRoom", "Room_03_VocalBooth",
            "Room_04_EquipmentCloset", "Room_05_TapeMachineRoom", "Room_06_Lounge",
            "Room_07_EchoChamber", "Room_08_Rooftop"
        };

        foreach (string roomName in roomScenes)
        {
            string scenePath = $"Assets/Scenes/{roomName}.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Check if dust already exists
            bool hasDust = false;
            bool hasUI = false;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "DustParticles") hasDust = true;
                if (go.name == "InGameCanvas") hasUI = true;
            }

            if (!hasDust && dustPrefab != null)
            {
                var dust = (GameObject)PrefabUtility.InstantiatePrefab(dustPrefab, scene);
                dust.transform.position = Vector3.zero;
            }

            if (!hasUI && uiPrefab != null)
            {
                var ui = (GameObject)PrefabUtility.InstantiatePrefab(uiPrefab, scene);
            }

            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[AtmosphereAndUI] Deployed to {roomName}");
        }
    }

    static GameObject CreateText(string name, Transform parent, string text, float fontSize,
        Color color, TextAlignmentOptions alignment, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        return go;
    }

    static GameObject CreateButton(string name, Transform parent, string label,
        Vector2 position, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(300, 50);

        var img = go.AddComponent<Image>();
        img.color = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 0.9f);

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);
        colors.pressedColor = color;
        btn.colors = colors;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.color = new Color(1, 0.91f, 0.82f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        return go;
    }
}
