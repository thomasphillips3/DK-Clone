using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MenuSceneSetup
{
    [MenuItem("Tools/Setup Menu Scene")]
    public static void Setup()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity", OpenSceneMode.Single);

        // Clear existing objects
        foreach (var go in scene.GetRootGameObjects())
            Object.DestroyImmediate(go);

        // -- Camera --
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.03f, 0.02f, 1f);
        camGO.AddComponent<AudioListener>();

        // -- EventSystem --
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // -- Canvas --
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // -- Background Panel --
        var bgImage = CreateUIElement<Image>("Background", canvasGO.transform);
        bgImage.color = new Color(0.05f, 0.03f, 0.02f, 1f);
        StretchFull(bgImage.gameObject);

        // -- Album Title --
        var titleGO = CreateTextElement("AlbumTitle", canvasGO.transform,
            "time off 3", 72, new Color(1f, 0.91f, 0.82f),
            new Vector2(0, 380));
        titleGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // -- Artist Name --
        var artistGO = CreateTextElement("ArtistName", canvasGO.transform,
            "Bombest Music", 32, new Color(0.7f, 0.6f, 0.5f),
            new Vector2(0, 310));

        // -- Track List Parent (Vertical Layout) --
        var trackListGO = new GameObject("TrackList");
        trackListGO.transform.SetParent(canvasGO.transform, false);
        var trackListRT = trackListGO.AddComponent<RectTransform>();
        trackListRT.anchoredPosition = new Vector2(0, -20);
        trackListRT.sizeDelta = new Vector2(600, 500);
        var vlg = trackListGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // -- Track Button Prefab (create in scene, then save as prefab) --
        var trackBtnGO = new GameObject("TrackButton");
        var btnRT = trackBtnGO.AddComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(600, 50);
        var btnImage = trackBtnGO.AddComponent<Image>();
        btnImage.color = new Color(0.12f, 0.08f, 0.06f, 0.8f);
        var btn = trackBtnGO.AddComponent<Button>();
        var btnColors = btn.colors;
        btnColors.normalColor = new Color(0.12f, 0.08f, 0.06f, 0.8f);
        btnColors.highlightedColor = new Color(0.25f, 0.18f, 0.12f, 1f);
        btnColors.pressedColor = new Color(0.35f, 0.25f, 0.15f, 1f);
        btn.colors = btnColors;

        var btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(trackBtnGO.transform, false);
        var btnText = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnText.text = "Track Name";
        btnText.fontSize = 28;
        btnText.color = new Color(1f, 0.91f, 0.82f);
        btnText.alignment = TextAlignmentOptions.MidlineLeft;
        var btnTextRT = btnTextGO.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = new Vector2(20, 0);
        btnTextRT.offsetMax = new Vector2(-20, 0);

        // Save track button as prefab
        string prefabDir = "Assets/Prefabs/UI";
        if (!AssetDatabase.IsValidFolder(prefabDir))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        string btnPrefabPath = prefabDir + "/TrackButton.prefab";
        PrefabUtility.SaveAsPrefabAsset(trackBtnGO, btnPrefabPath);
        Object.DestroyImmediate(trackBtnGO);
        Debug.Log($"[MenuSceneSetup] TrackButton prefab saved to {btnPrefabPath}");

        // -- Navigation Mode Buttons --
        var modeParentGO = new GameObject("ModeButtons");
        modeParentGO.transform.SetParent(canvasGO.transform, false);
        var modeRT = modeParentGO.AddComponent<RectTransform>();
        modeRT.anchoredPosition = new Vector2(0, -340);
        modeRT.sizeDelta = new Vector2(700, 60);
        var hlg = modeParentGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        var menuBtn = CreateModeButton("MenuModeButton", modeParentGO.transform, "MENU", new Color(0.6f, 0.4f, 0.3f));
        var connBtn = CreateModeButton("ConnectedModeButton", modeParentGO.transform, "CONNECTED", new Color(0.4f, 0.5f, 0.6f));
        var seqBtn = CreateModeButton("SequentialModeButton", modeParentGO.transform, "SEQUENTIAL", new Color(0.5f, 0.6f, 0.4f));

        // -- Credits Text --
        CreateTextElement("Credits", canvasGO.transform,
            "Design & Engineering: JR  |  Music & Sound: Bombest Music  |  Creative Direction: tomdabomb",
            16, new Color(0.4f, 0.35f, 0.3f),
            new Vector2(0, -480));

        // -- Attach MenuController and wire references --
        var menuCtrl = canvasGO.AddComponent<MenuController>();
        var so = new SerializedObject(menuCtrl);

        var albumConfigAsset = AssetDatabase.LoadAssetAtPath<AlbumConfig>("Assets/Data/AlbumConfig.asset");
        so.FindProperty("albumConfig").objectReferenceValue = albumConfigAsset;
        so.FindProperty("trackListParent").objectReferenceValue = trackListRT;

        var btnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(btnPrefabPath);
        so.FindProperty("trackButtonPrefab").objectReferenceValue = btnPrefab;
        so.FindProperty("albumTitleText").objectReferenceValue = titleGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("artistNameText").objectReferenceValue = artistGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("menuModeButton").objectReferenceValue = menuBtn.GetComponent<Button>();
        so.FindProperty("connectedModeButton").objectReferenceValue = connBtn.GetComponent<Button>();
        so.FindProperty("sequentialModeButton").objectReferenceValue = seqBtn.GetComponent<Button>();

        so.ApplyModifiedProperties();
        Debug.Log("[MenuSceneSetup] MenuController wired.");

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[MenuSceneSetup] Menu scene saved.");
    }

    static GameObject CreateTextElement(string name, Transform parent, string text, float fontSize, Color color, Vector2 position)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(800, fontSize + 20);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    static T CreateUIElement<T>(string name, Transform parent) where T : Component
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go.AddComponent<T>();
    }

    static void StretchFull(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static GameObject CreateModeButton(string name, Transform parent, string label, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 50);
        var img = go.AddComponent<Image>();
        img.color = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 0.8f);
        var btn = go.AddComponent<Button>();
        var btnColors = btn.colors;
        btnColors.normalColor = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 0.8f);
        btnColors.highlightedColor = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);
        btnColors.pressedColor = color;
        btn.colors = btnColors;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.color = new Color(1f, 0.91f, 0.82f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        return go;
    }
}
