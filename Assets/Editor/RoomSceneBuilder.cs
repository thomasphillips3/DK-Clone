using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Batch-builds all 8 room scenes with the standard template:
/// Player prefab, RoomController, lighting, floor, atmosphere.
/// </summary>
public static class RoomSceneBuilder
{
    private struct RoomDef
    {
        public string sceneName;
        public string trackAssetPath;
        public int trackIndex;
        public string roomLabel;
        public Color primaryColor;
        public Color secondaryColor;
        public Color accentColor;
        public float roomWidth;
        public float roomDepth;
        public float roomHeight;
    }

    private static readonly RoomDef[] rooms = new RoomDef[]
    {
        new RoomDef {
            sceneName = "Room_01_LiveRoom", trackAssetPath = "Assets/Data/Track_01_LiveRoom.asset",
            trackIndex = 0, roomLabel = "Live Room",
            primaryColor = new Color(0.9f, 0.7f, 0.3f), secondaryColor = new Color(0.3f, 0.2f, 0.15f),
            accentColor = new Color(1f, 0.85f, 0.4f),
            roomWidth = 12, roomDepth = 10, roomHeight = 4
        },
        new RoomDef {
            sceneName = "Room_02_ControlRoom", trackAssetPath = "Assets/Data/Track_02_ControlRoom.asset",
            trackIndex = 1, roomLabel = "Control Room",
            primaryColor = new Color(0.2f, 0.5f, 0.7f), secondaryColor = new Color(0.15f, 0.2f, 0.3f),
            accentColor = new Color(0.4f, 0.8f, 1f),
            roomWidth = 10, roomDepth = 8, roomHeight = 3.5f
        },
        new RoomDef {
            sceneName = "Room_03_VocalBooth", trackAssetPath = "Assets/Data/Track_03_VocalBooth.asset",
            trackIndex = 2, roomLabel = "Vocal Booth",
            primaryColor = new Color(0.6f, 0.4f, 0.5f), secondaryColor = new Color(0.25f, 0.15f, 0.2f),
            accentColor = new Color(0.9f, 0.6f, 0.7f),
            roomWidth = 4, roomDepth = 4, roomHeight = 3
        },
        new RoomDef {
            sceneName = "Room_04_EquipmentCloset", trackAssetPath = "Assets/Data/Track_04_EquipmentCloset.asset",
            trackIndex = 3, roomLabel = "Equipment Closet",
            primaryColor = new Color(0.3f, 0.6f, 0.3f), secondaryColor = new Color(0.1f, 0.2f, 0.1f),
            accentColor = new Color(0.5f, 1f, 0.5f),
            roomWidth = 5, roomDepth = 3, roomHeight = 3
        },
        new RoomDef {
            sceneName = "Room_05_TapeMachineRoom", trackAssetPath = "Assets/Data/Track_05_TapeMachineRoom.asset",
            trackIndex = 4, roomLabel = "Tape Machine Room",
            primaryColor = new Color(0.7f, 0.5f, 0.3f), secondaryColor = new Color(0.3f, 0.2f, 0.1f),
            accentColor = new Color(1f, 0.7f, 0.4f),
            roomWidth = 8, roomDepth = 6, roomHeight = 3.5f
        },
        new RoomDef {
            sceneName = "Room_06_Lounge", trackAssetPath = "Assets/Data/Track_06_Lounge.asset",
            trackIndex = 5, roomLabel = "Lounge",
            primaryColor = new Color(0.6f, 0.3f, 0.5f), secondaryColor = new Color(0.2f, 0.1f, 0.2f),
            accentColor = new Color(0.8f, 0.4f, 0.7f),
            roomWidth = 10, roomDepth = 8, roomHeight = 3.5f
        },
        new RoomDef {
            sceneName = "Room_07_EchoChamber", trackAssetPath = "Assets/Data/Track_07_EchoChamber.asset",
            trackIndex = 6, roomLabel = "Echo Chamber",
            primaryColor = new Color(0.4f, 0.4f, 0.5f), secondaryColor = new Color(0.15f, 0.15f, 0.2f),
            accentColor = new Color(0.6f, 0.6f, 0.8f),
            roomWidth = 8, roomDepth = 12, roomHeight = 6
        },
        new RoomDef {
            sceneName = "Room_08_Rooftop", trackAssetPath = "Assets/Data/Track_08_Rooftop.asset",
            trackIndex = 7, roomLabel = "Rooftop",
            primaryColor = new Color(0.2f, 0.3f, 0.5f), secondaryColor = new Color(0.05f, 0.08f, 0.15f),
            accentColor = new Color(0.9f, 0.7f, 0.3f),
            roomWidth = 20, roomDepth = 15, roomHeight = 0 // outdoor, no ceiling
        }
    };

    [MenuItem("Tools/Build All Room Scenes")]
    public static void BuildAllRooms()
    {
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/Player.prefab");
        if (playerPrefab == null)
        {
            Debug.LogError("[RoomBuilder] Player.prefab not found!");
            return;
        }

        foreach (var room in rooms)
        {
            BuildRoom(room, playerPrefab);
        }

        Debug.Log("[RoomBuilder] All 8 rooms built successfully.");
    }

    static void BuildRoom(RoomDef room, GameObject playerPrefab)
    {
        string scenePath = $"Assets/Scenes/{room.sceneName}.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Clear existing objects (except those we want to keep for Room_02)
        if (room.sceneName != "Room_02_ControlRoom")
        {
            foreach (var go in scene.GetRootGameObjects())
                Object.DestroyImmediate(go);
        }
        else
        {
            // Room_02 already has content — just ensure it has the template objects
            // Check if Player exists, if not add it
            bool hasPlayer = false;
            bool hasRoomController = false;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "Player") hasPlayer = true;
                if (go.name == "RoomController") hasRoomController = true;
            }
            if (hasPlayer && hasRoomController)
            {
                // Room_02 is already set up, just save and skip
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[RoomBuilder] {room.sceneName} already configured, skipping.");
                return;
            }
        }

        float hw = room.roomWidth / 2f;
        float hd = room.roomDepth / 2f;

        // -- Player --
        var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
        player.transform.position = new Vector3(0, 0.1f, -hd + 1.5f);
        SceneManager.MoveGameObjectToScene(player, scene);

        // -- Room Controller --
        var rcGO = new GameObject("RoomController");
        SceneManager.MoveGameObjectToScene(rcGO, scene);
        var rm = rcGO.AddComponent<RoomManager>();
        var trackData = AssetDatabase.LoadAssetAtPath<TrackData>(room.trackAssetPath);
        if (trackData != null)
        {
            var so = new SerializedObject(rm);
            so.FindProperty("trackData").objectReferenceValue = trackData;
            so.FindProperty("trackIndex").intValue = room.trackIndex;
            so.ApplyModifiedProperties();
        }

        // -- Floor --
        var floorGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floorGO.name = "Floor";
        floorGO.transform.position = new Vector3(0, -0.05f, 0);
        floorGO.transform.localScale = new Vector3(room.roomWidth, 0.1f, room.roomDepth);
        floorGO.isStatic = true;
        floorGO.layer = 15; // Surface layer
        SceneManager.MoveGameObjectToScene(floorGO, scene);
        var floorRenderer = floorGO.GetComponent<Renderer>();
        if (floorRenderer != null)
            floorRenderer.sharedMaterial = CreateTempMaterial("Floor_" + room.sceneName, room.secondaryColor * 0.5f);

        // -- Walls (4 walls) --
        var wallsParent = new GameObject("Walls");
        wallsParent.isStatic = true;
        SceneManager.MoveGameObjectToScene(wallsParent, scene);

        // Back wall
        CreateWall("Wall_Back", wallsParent.transform,
            new Vector3(0, room.roomHeight / 2f, hd),
            new Vector3(room.roomWidth, room.roomHeight, 0.2f),
            room.secondaryColor * 0.4f);
        // Front wall
        CreateWall("Wall_Front", wallsParent.transform,
            new Vector3(0, room.roomHeight / 2f, -hd),
            new Vector3(room.roomWidth, room.roomHeight, 0.2f),
            room.secondaryColor * 0.4f);
        // Left wall
        CreateWall("Wall_Left", wallsParent.transform,
            new Vector3(-hw, room.roomHeight / 2f, 0),
            new Vector3(0.2f, room.roomHeight, room.roomDepth),
            room.secondaryColor * 0.35f);
        // Right wall
        CreateWall("Wall_Right", wallsParent.transform,
            new Vector3(hw, room.roomHeight / 2f, 0),
            new Vector3(0.2f, room.roomHeight, room.roomDepth),
            room.secondaryColor * 0.35f);

        // Ceiling (skip for Rooftop)
        if (room.roomHeight > 0)
        {
            CreateWall("Ceiling", wallsParent.transform,
                new Vector3(0, room.roomHeight, 0),
                new Vector3(room.roomWidth, 0.1f, room.roomDepth),
                room.secondaryColor * 0.3f);
        }

        // -- Lighting --
        // Main warm point light
        var mainLightGO = new GameObject("MainLight");
        SceneManager.MoveGameObjectToScene(mainLightGO, scene);
        mainLightGO.transform.position = new Vector3(0, room.roomHeight > 0 ? room.roomHeight - 0.5f : 5f, 0);
        var mainLight = mainLightGO.AddComponent<Light>();
        mainLight.type = LightType.Point;
        mainLight.color = Color.Lerp(room.primaryColor, new Color(1, 0.9f, 0.7f), 0.5f);
        mainLight.intensity = 2f;
        mainLight.range = Mathf.Max(room.roomWidth, room.roomDepth) * 1.2f;
        mainLightGO.AddComponent<BeatReactiveLight>();

        // Fill light
        var fillLightGO = new GameObject("FillLight");
        SceneManager.MoveGameObjectToScene(fillLightGO, scene);
        fillLightGO.transform.position = new Vector3(-hw * 0.6f, room.roomHeight * 0.7f, -hd * 0.5f);
        var fillLight = fillLightGO.AddComponent<Light>();
        fillLight.type = LightType.Point;
        fillLight.color = room.accentColor * 0.5f;
        fillLight.intensity = 0.6f;
        fillLight.range = Mathf.Max(room.roomWidth, room.roomDepth) * 0.8f;

        // -- Ambient Settings --
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = room.secondaryColor * 0.15f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.03f;
        RenderSettings.fogColor = room.secondaryColor * 0.2f;

        // Skybox for Rooftop, solid color for indoor rooms
        if (room.sceneName == "Room_08_Rooftop")
        {
            // Use default skybox for outdoor
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.1f, 0.15f, 0.3f);
            RenderSettings.ambientEquatorColor = new Color(0.3f, 0.2f, 0.15f);
            RenderSettings.ambientGroundColor = new Color(0.05f, 0.05f, 0.05f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.08f, 0.1f, 0.18f);
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[RoomBuilder] {room.sceneName} ({room.roomLabel}) built and saved.");
    }

    static void CreateWall(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.localPosition = position;
        wall.transform.localScale = scale;
        wall.isStatic = true;
        var renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = CreateTempMaterial(name, color);
    }

    static Material CreateTempMaterial(string name, Color color)
    {
        // Use standard shader with the given color
        var mat = new Material(Shader.Find("Standard"));
        mat.name = name + "_Mat";
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.2f);
        return mat;
    }
}
