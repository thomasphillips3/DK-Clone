using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates the Boot splash screen and interactable prefabs.
/// </summary>
public static class SplashAndInteractablesSetup
{
    [MenuItem("Tools/Setup Splash Screen")]
    public static void SetupSplash()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity", OpenSceneMode.Single);

        // Check if splash already exists
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == "SplashCanvas") return;
        }

        // Camera for splash
        var camGO = new GameObject("SplashCamera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.03f, 0.02f, 1f);
        cam.tag = "MainCamera";

        // Splash Canvas
        var canvasGO = new GameObject("SplashCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Background
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.05f, 0.03f, 0.02f, 1f);

        // Album title
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(canvasGO.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchoredPosition = new Vector2(0, 20);
        titleRT.sizeDelta = new Vector2(800, 100);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "time off 3";
        titleTMP.fontSize = 80;
        titleTMP.color = new Color(1f, 0.91f, 0.82f, 0f); // starts invisible, fades in
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.Bold;

        // Artist subtitle
        var artistGO = new GameObject("Artist");
        artistGO.transform.SetParent(canvasGO.transform, false);
        var artistRT = artistGO.AddComponent<RectTransform>();
        artistRT.anchoredPosition = new Vector2(0, -50);
        artistRT.sizeDelta = new Vector2(600, 40);
        var artistTMP = artistGO.AddComponent<TextMeshProUGUI>();
        artistTMP.text = "Bombest Music";
        artistTMP.fontSize = 28;
        artistTMP.color = new Color(0.7f, 0.6f, 0.5f, 0f);
        artistTMP.alignment = TextAlignmentOptions.Center;

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SplashSetup] Boot splash screen created.");
    }

    [MenuItem("Tools/Create Interactable Prefabs")]
    public static void CreateInteractablePrefabs()
    {
        string dir = "Assets/Prefabs/Interactables";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Interactables");

        CreateCablePlugPrefab(dir);
        CreatePowerSwitchPrefab(dir);
        CreateFaderSlidePrefab(dir);
        CreateRecordPlayerPrefab(dir);
        CreateTapeThreaderPrefab(dir);

        Debug.Log("[InteractableSetup] All 5 interactable prefabs created.");
    }

    static void CreateCablePlugPrefab(string dir)
    {
        var root = new GameObject("CablePlug");
        root.layer = 14; // Interactable

        // Cable body
        var cable = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cable.name = "CableEnd";
        cable.transform.SetParent(root.transform, false);
        cable.transform.localScale = new Vector3(0.02f, 0.3f, 0.02f);
        cable.transform.localPosition = new Vector3(0, 0.15f, 0);
        SetColor(cable, new Color(0.1f, 0.1f, 0.1f));

        // Plug target jack
        var jack = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        jack.name = "PlugTarget";
        jack.transform.SetParent(root.transform, false);
        jack.transform.localScale = new Vector3(0.04f, 0.05f, 0.04f);
        jack.transform.localPosition = new Vector3(0.3f, 0.5f, 0);
        SetColor(jack, new Color(0.3f, 0.3f, 0.3f));

        // VU meter light
        var vuLight = new GameObject("VUMeterLight");
        vuLight.transform.SetParent(root.transform, false);
        vuLight.transform.localPosition = new Vector3(0.3f, 0.6f, 0);
        var light = vuLight.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Color.green;
        light.intensity = 0;
        light.range = 0.5f;

        // Add collider for interaction
        var col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(0.5f, 0.8f, 0.3f);
        col.center = new Vector3(0.15f, 0.3f, 0);

        // Wire CablePlug component
        var plug = root.AddComponent<CablePlug>();
        var so = new SerializedObject(plug);
        so.FindProperty("cableEnd").objectReferenceValue = cable.transform;
        so.FindProperty("plugTarget").objectReferenceValue = jack.transform;
        so.FindProperty("vuMeterLight").objectReferenceValue = vuLight;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, dir + "/CablePlug.prefab");
        Object.DestroyImmediate(root);
    }

    static void CreatePowerSwitchPrefab(string dir)
    {
        var root = new GameObject("PowerSwitch");
        root.layer = 14;

        // Switch body (wall plate)
        var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.name = "SwitchPlate";
        plate.transform.SetParent(root.transform, false);
        plate.transform.localScale = new Vector3(0.08f, 0.12f, 0.02f);
        SetColor(plate, new Color(0.85f, 0.82f, 0.75f));

        // Toggle switch
        var toggle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        toggle.name = "SwitchToggle";
        toggle.transform.SetParent(root.transform, false);
        toggle.transform.localScale = new Vector3(0.02f, 0.04f, 0.03f);
        toggle.transform.localPosition = new Vector3(0, 0, 0.015f);
        SetColor(toggle, new Color(0.2f, 0.2f, 0.2f));

        // Indicator
        var indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.name = "Indicator";
        indicator.transform.SetParent(root.transform, false);
        indicator.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        indicator.transform.localPosition = new Vector3(0, 0.04f, 0.015f);
        SetColor(indicator, new Color(0.3f, 0, 0)); // off = dim red

        // Target light (what the switch controls)
        var targetLight = new GameObject("TargetLight");
        targetLight.transform.SetParent(root.transform, false);
        targetLight.transform.localPosition = new Vector3(0, 1, 0);
        var lt = targetLight.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.intensity = 0;
        lt.range = 5;

        var col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(0.12f, 0.16f, 0.06f);

        var sw = root.AddComponent<PowerSwitch>();
        var so = new SerializedObject(sw);
        so.FindProperty("indicatorRenderer").objectReferenceValue = indicator.GetComponent<Renderer>();
        so.FindProperty("targetLight").objectReferenceValue = targetLight;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, dir + "/PowerSwitch.prefab");
        Object.DestroyImmediate(root);
    }

    static void CreateFaderSlidePrefab(string dir)
    {
        var root = new GameObject("FaderSlide");
        root.layer = 14;

        // Fader track
        var track = GameObject.CreatePrimitive(PrimitiveType.Cube);
        track.name = "FaderTrack";
        track.transform.SetParent(root.transform, false);
        track.transform.localScale = new Vector3(0.02f, 0.15f, 0.01f);
        SetColor(track, new Color(0.15f, 0.15f, 0.15f));

        // Fader knob
        var knob = GameObject.CreatePrimitive(PrimitiveType.Cube);
        knob.name = "FaderKnob";
        knob.transform.SetParent(root.transform, false);
        knob.transform.localScale = new Vector3(0.03f, 0.02f, 0.02f);
        knob.transform.localPosition = new Vector3(0, -0.05f, 0.01f);
        SetColor(knob, new Color(0.8f, 0.2f, 0.2f)); // red knob

        // Channel meter
        var meter = new GameObject("ChannelMeter");
        meter.transform.SetParent(root.transform, false);
        meter.transform.localPosition = new Vector3(0.04f, 0, 0);
        var meterLight = meter.AddComponent<Light>();
        meterLight.type = LightType.Point;
        meterLight.color = Color.green;
        meterLight.intensity = 0;
        meterLight.range = 0.3f;

        var col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(0.06f, 0.2f, 0.04f);

        var fader = root.AddComponent<FaderSlide>();
        var so = new SerializedObject(fader);
        so.FindProperty("faderKnob").objectReferenceValue = knob.transform;
        so.FindProperty("channelMeter").objectReferenceValue = meter;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, dir + "/FaderSlide.prefab");
        Object.DestroyImmediate(root);
    }

    static void CreateRecordPlayerPrefab(string dir)
    {
        var root = new GameObject("RecordPlayer");
        root.layer = 14;

        // Turntable base
        var baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseObj.name = "TurntableBase";
        baseObj.transform.SetParent(root.transform, false);
        baseObj.transform.localScale = new Vector3(0.5f, 0.08f, 0.4f);
        SetColor(baseObj, new Color(0.3f, 0.15f, 0.08f)); // walnut wood

        // Platter
        var platter = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        platter.name = "Platter";
        platter.transform.SetParent(root.transform, false);
        platter.transform.localScale = new Vector3(0.35f, 0.01f, 0.35f);
        platter.transform.localPosition = new Vector3(-0.05f, 0.05f, 0);
        SetColor(platter, new Color(0.1f, 0.1f, 0.1f)); // black

        // Tone arm
        var arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arm.name = "ToneArm";
        arm.transform.SetParent(root.transform, false);
        arm.transform.localScale = new Vector3(0.008f, 0.12f, 0.008f);
        arm.transform.localPosition = new Vector3(0.2f, 0.1f, 0);
        arm.transform.localRotation = Quaternion.Euler(0, 0, -30);
        SetColor(arm, new Color(0.7f, 0.7f, 0.7f)); // silver

        var col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(0.55f, 0.2f, 0.45f);
        col.center = new Vector3(0, 0.05f, 0);

        var rp = root.AddComponent<RecordPlayerInteractable>();
        var so = new SerializedObject(rp);
        so.FindProperty("platter").objectReferenceValue = platter.transform;
        so.FindProperty("toneArm").objectReferenceValue = arm.transform;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, dir + "/RecordPlayer.prefab");
        Object.DestroyImmediate(root);
    }

    static void CreateTapeThreaderPrefab(string dir)
    {
        var root = new GameObject("TapeThreader");
        root.layer = 14;

        // Machine body
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "MachineBody";
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = new Vector3(0.5f, 0.6f, 0.25f);
        body.transform.localPosition = new Vector3(0, 0.3f, 0);
        SetColor(body, new Color(0.6f, 0.55f, 0.45f)); // beige

        // Supply reel
        var supply = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        supply.name = "SupplyReel";
        supply.transform.SetParent(root.transform, false);
        supply.transform.localScale = new Vector3(0.18f, 0.02f, 0.18f);
        supply.transform.localPosition = new Vector3(-0.12f, 0.5f, 0.13f);
        supply.transform.localRotation = Quaternion.Euler(90, 0, 0);
        SetColor(supply, new Color(0.2f, 0.2f, 0.2f));

        // Takeup reel
        var takeup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        takeup.name = "TakeupReel";
        takeup.transform.SetParent(root.transform, false);
        takeup.transform.localScale = new Vector3(0.18f, 0.02f, 0.18f);
        takeup.transform.localPosition = new Vector3(0.12f, 0.5f, 0.13f);
        takeup.transform.localRotation = Quaternion.Euler(90, 0, 0);
        SetColor(takeup, new Color(0.2f, 0.2f, 0.2f));

        var col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(0.55f, 0.65f, 0.3f);
        col.center = new Vector3(0, 0.3f, 0);

        var tt = root.AddComponent<TapeThreader>();
        var so = new SerializedObject(tt);
        so.FindProperty("supplyReel").objectReferenceValue = supply.transform;
        so.FindProperty("takeupReel").objectReferenceValue = takeup.transform;
        so.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(root, dir + "/TapeThreader.prefab");
        Object.DestroyImmediate(root);
    }

    static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Smoothness", 0.3f);
            renderer.sharedMaterial = mat;
        }
    }
}
