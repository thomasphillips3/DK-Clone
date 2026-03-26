using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Rebuilds all 8 room scenes with proper visuals:
/// - Dark warm background (no skybox)
/// - Recording studio model in each room (rotated per room for variety)
/// - Proper warm/cool lighting with beat-reactive main light
/// - Floor collider, dust particles, fog
/// - InGameCanvas with NowPlayingHUD and PauseMenu
/// Run from Tools > Build All Room Scenes.
/// </summary>
public static class RoomSceneBuilder
{
    static readonly (string scene, string trackAsset, Vector3 playerPos, float modelRotY, Color warmColor, Color coolColor)[] rooms = new[]
    {
        ("Room_01_LiveRoom",         "Assets/Data/Track_01_LiveRoom.asset",         new Vector3(0, 1.5f, -3f),  0f,   new Color(0.9f, 0.7f, 0.3f), new Color(0.5f, 0.5f, 0.7f)),
        ("Room_02_ControlRoom",      "Assets/Data/Track_02_ControlRoom.asset",      new Vector3(0, 1.5f, -3f),  0f,   new Color(0.4f, 0.6f, 0.9f), new Color(0.7f, 0.8f, 1f)),
        ("Room_03_VocalBooth",       "Assets/Data/Track_03_VocalBooth.asset",       new Vector3(0, 1.5f, -2f),  90f,  new Color(0.8f, 0.5f, 0.6f), new Color(0.6f, 0.4f, 0.5f)),
        ("Room_04_EquipmentCloset",  "Assets/Data/Track_04_EquipmentCloset.asset",  new Vector3(2, 1.5f, -2f),  180f, new Color(0.4f, 0.7f, 0.4f), new Color(0.3f, 0.5f, 0.3f)),
        ("Room_05_TapeMachineRoom",  "Assets/Data/Track_05_TapeMachineRoom.asset",  new Vector3(-2, 1.5f, -3f), 270f, new Color(0.8f, 0.6f, 0.3f), new Color(0.5f, 0.4f, 0.2f)),
        ("Room_06_Lounge",           "Assets/Data/Track_06_Lounge.asset",           new Vector3(0, 1.5f, -4f),  45f,  new Color(0.7f, 0.3f, 0.5f), new Color(0.4f, 0.2f, 0.4f)),
        ("Room_07_EchoChamber",      "Assets/Data/Track_07_EchoChamber.asset",      new Vector3(0, 1.5f, -3f),  135f, new Color(0.5f, 0.5f, 0.6f), new Color(0.3f, 0.3f, 0.5f)),
        ("Room_08_Rooftop",          "Assets/Data/Track_08_Rooftop.asset",          new Vector3(0, 1.5f, -2f),  0f,   new Color(0.3f, 0.4f, 0.6f), new Color(0.15f, 0.2f, 0.35f)),
    };

    [MenuItem("Tools/Build All Room Scenes")]
    public static void BuildAllRooms()
    {
        CreateRoomMaterials();

        var studioModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Equipment/RecordingStudio.dae");
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab");

        for (int i = 0; i < rooms.Length; i++)
        {
            var room = rooms[i];
            string scenePath = $"Assets/Scenes/{room.scene}.unity";
            Debug.Log($"[RoomBuilder] Building {room.scene}...");

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Clear everything
            foreach (var go in scene.GetRootGameObjects())
                Object.DestroyImmediate(go);

            // Render settings
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.04f, 0.035f, 0.025f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.06f;
            RenderSettings.fogColor = new Color(0.03f, 0.025f, 0.02f);

            // Player prefab
            if (playerPrefab != null)
            {
                var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
                player.transform.position = room.playerPos;

                var playerCam = player.GetComponentInChildren<Camera>();
                if (playerCam != null)
                {
                    playerCam.gameObject.tag = "MainCamera";
                    playerCam.clearFlags = CameraClearFlags.SolidColor;
                    playerCam.backgroundColor = new Color(0.04f, 0.03f, 0.02f);
                    playerCam.nearClipPlane = 0.1f;
                    playerCam.farClipPlane = 50f;
                    playerCam.fieldOfView = 65f;
                }
            }

            // Studio model
            if (studioModel != null)
            {
                var studio = (GameObject)PrefabUtility.InstantiatePrefab(studioModel, scene);
                studio.name = "RecordingStudio";
                studio.transform.position = Vector3.zero;
                studio.transform.rotation = Quaternion.Euler(0, room.modelRotY, 0);
                studio.isStatic = true;

                foreach (var mf in studio.GetComponentsInChildren<MeshFilter>())
                {
                    if (mf.GetComponent<Collider>() == null)
                        mf.gameObject.AddComponent<MeshCollider>();
                }
            }

            // Floor
            var floorGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floorGO.name = "Floor";
            floorGO.transform.position = Vector3.zero;
            floorGO.transform.localScale = new Vector3(5, 1, 5);
            floorGO.isStatic = true;
            var floorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Rooms/Floor_Dark.mat");
            if (floorMat != null)
                floorGO.GetComponent<Renderer>().sharedMaterial = floorMat;

            // Warm point light
            var warmGO = new GameObject("WarmLight");
            var wl = warmGO.AddComponent<Light>();
            wl.type = LightType.Point;
            wl.color = room.warmColor;
            wl.intensity = 2.5f;
            wl.range = 15f;
            wl.shadows = LightShadows.Soft;
            warmGO.transform.position = new Vector3(0, 3.5f, 0);
            warmGO.AddComponent<BeatReactiveLight>();

            // Cool fill
            var coolGO = new GameObject("FillLight");
            var cl = coolGO.AddComponent<Light>();
            cl.type = LightType.Point;
            cl.color = room.coolColor;
            cl.intensity = 0.8f;
            cl.range = 12f;
            coolGO.transform.position = new Vector3(-3, 2.5f, 2);

            // Accent spot
            var spotGO = new GameObject("AccentSpot");
            var sl = spotGO.AddComponent<Light>();
            sl.type = LightType.Spot;
            sl.color = new Color(1f, 0.9f, 0.7f);
            sl.intensity = 3f;
            sl.range = 8f;
            sl.spotAngle = 60f;
            spotGO.transform.position = new Vector3(1, 3, -1);
            spotGO.transform.rotation = Quaternion.Euler(60, -20, 0);

            // Dust
            var dustGO = new GameObject("DustParticles");
            var ps = dustGO.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(1f, 0.9f, 0.7f, 0.08f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.05f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.005f, 0.03f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 12f);
            main.maxParticles = 300;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission;
            em.rateOverTime = 40f;
            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Box;
            sh.scale = new Vector3(8, 4, 8);
            dustGO.transform.position = new Vector3(0, 2, 0);
            var psr = dustGO.GetComponent<ParticleSystemRenderer>();
            psr.material = new Material(Shader.Find("Particles/Standard Unlit"));
            psr.material.SetColor("_Color", new Color(1f, 0.9f, 0.7f, 0.08f));
            dustGO.AddComponent<DustParticleController>();

            // RoomController
            var rcGO = new GameObject("RoomController");
            var rm = rcGO.AddComponent<RoomManager>();
            var trackData = AssetDatabase.LoadAssetAtPath<TrackData>(room.trackAsset);
            var so = new SerializedObject(rm);
            SetRef(so, "trackData", trackData);
            var idxProp = so.FindProperty("trackIndex");
            if (idxProp != null) idxProp.intValue = i;
            so.ApplyModifiedPropertiesWithoutUndo();

            // InGameCanvas
            BuildInGameCanvas();

            // EventSystem
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[RoomBuilder] {room.scene} done");
        }

        Debug.Log("[RoomBuilder] All 8 rooms built!");
    }

    static void CreateRoomMaterials()
    {
        string dir = "Assets/Materials/Rooms";
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/Materials", "Rooms");

        if (AssetDatabase.LoadAssetAtPath<Material>($"{dir}/Floor_Dark.mat") == null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.08f, 0.06f, 0.04f);
            mat.SetFloat("_Smoothness", 0.3f);
            mat.SetFloat("_Metallic", 0.1f);
            AssetDatabase.CreateAsset(mat, $"{dir}/Floor_Dark.mat");
        }
        AssetDatabase.SaveAssets();
    }

    static void BuildInGameCanvas()
    {
        var canvasGO = new GameObject("InGameCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // NowPlaying HUD
        var hudGO = new GameObject("NowPlayingHUD");
        hudGO.transform.SetParent(canvasGO.transform, false);
        var hudCG = hudGO.AddComponent<CanvasGroup>();
        hudCG.alpha = 0f;
        var hudRT = hudGO.GetComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0, 0.88f);
        hudRT.anchorMax = new Vector2(0.65f, 1f);
        hudRT.offsetMin = new Vector2(40, 0);
        hudRT.offsetMax = new Vector2(0, -40);

        var titleTMP = CreateTMPChild(hudGO.transform, "TrackTitle", "Track", 32,
            new Color(1f, 0.92f, 0.78f), FontStyles.Bold,
            new Vector2(0, 0.5f), new Vector2(1, 1));

        var artistTMP = CreateTMPChild(hudGO.transform, "ArtistName", "Artist", 20,
            new Color(0.85f, 0.65f, 0.4f, 0.8f), FontStyles.Normal,
            new Vector2(0, 0.15f), new Vector2(1, 0.5f));

        var timeTMP = CreateTMPChild(hudGO.transform, "TimeText", "0:00 / 0:00", 16,
            new Color(0.7f, 0.6f, 0.5f, 0.7f), FontStyles.Normal,
            new Vector2(0, 0), new Vector2(1, 0.2f));

        var hud = hudGO.AddComponent<NowPlayingHUD>();
        var hudSO = new SerializedObject(hud);
        SetRef(hudSO, "trackTitleText", titleTMP);
        SetRef(hudSO, "artistText", artistTMP);
        SetRef(hudSO, "timeText", timeTMP);
        SetRef(hudSO, "canvasGroup", hudCG);
        hudSO.ApplyModifiedPropertiesWithoutUndo();

        // Pause hint
        CreateTMPChild(canvasGO.transform, "PauseHint", "tap to pause", 18,
            new Color(0.6f, 0.5f, 0.4f, 0.35f), FontStyles.Normal,
            new Vector2(0.3f, 0.01f), new Vector2(0.7f, 0.04f),
            TextAlignmentOptions.Center);

        // Pause panel (hidden)
        var pauseGO = new GameObject("PausePanel");
        pauseGO.transform.SetParent(canvasGO.transform, false);
        var pauseBG = pauseGO.AddComponent<Image>();
        pauseBG.color = new Color(0.03f, 0.02f, 0.015f, 0.9f);
        var pauseRT = pauseGO.GetComponent<RectTransform>();
        pauseRT.anchorMin = new Vector2(0.1f, 0.25f);
        pauseRT.anchorMax = new Vector2(0.9f, 0.75f);
        pauseRT.offsetMin = pauseRT.offsetMax = Vector2.zero;

        CreateTMPChild(pauseGO.transform, "PausedText", "PAUSED", 48,
            new Color(1f, 0.92f, 0.78f), FontStyles.Bold,
            new Vector2(0, 0.7f), new Vector2(1, 0.95f),
            TextAlignmentOptions.Center);

        var resumeBtn = CreateButton(pauseGO.transform, "ResumeButton", "RESUME",
            new Vector2(0.15f, 0.38f), new Vector2(0.85f, 0.58f));
        var backBtn = CreateButton(pauseGO.transform, "BackToMenuButton", "BACK TO MENU",
            new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.32f));

        var pm = pauseGO.AddComponent<PauseMenu>();
        var pmSO = new SerializedObject(pm);
        SetRef(pmSO, "resumeButton", resumeBtn);
        SetRef(pmSO, "backToMenuButton", backBtn);
        pmSO.ApplyModifiedPropertiesWithoutUndo();

        pauseGO.SetActive(false);

        // PauseTrigger on the canvas (always active)
        var pt = canvasGO.AddComponent<PauseTrigger>();
        var ptSO = new SerializedObject(pt);
        SetRef(ptSO, "pausePanel", pauseGO);
        ptSO.ApplyModifiedPropertiesWithoutUndo();
    }

    static TextMeshProUGUI CreateTMPChild(Transform parent, string name, string text, float size,
        Color color, FontStyles style, Vector2 anchorMin, Vector2 anchorMax,
        TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = align;
        var rt = tmp.rectTransform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.15f, 0.1f, 0.9f);
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.15f, 0.1f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.22f, 0.12f, 1f);
        colors.pressedColor = new Color(0.5f, 0.35f, 0.15f, 1f);
        colors.fadeDuration = 0.1f;
        btn.colors = colors;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        CreateTMPChild(go.transform, "Text", label, 28,
            new Color(1f, 0.92f, 0.78f), FontStyles.Bold,
            Vector2.zero, Vector2.one, TextAlignmentOptions.Center);

        return btn;
    }

    static void SetRef(SerializedObject so, string propName, Object value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null)
            prop.objectReferenceValue = value;
    }
}
