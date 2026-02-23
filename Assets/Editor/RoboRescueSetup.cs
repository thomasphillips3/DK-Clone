using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// <summary>
/// One-shot setup script for Robo Rescue.
/// Run "Robo Rescue / Build Everything" from the menu bar once to populate
/// prefabs and the Level01 scene completely.
/// </summary>
public static class RoboRescueSetup
{
    // ── Layer indices (match TagManager.asset) ─────────────────────────────
    const int L_PLAYER      = 6;
    const int L_PLATFORM    = 7;
    const int L_LADDER      = 8;
    const int L_HAZARD      = 9;
    const int L_COLLECTIBLE = 10;
    const int L_ENEMY       = 11;
    const int L_ONEWAY      = 12;

    static int GroundMask => (1 << L_PLATFORM) | (1 << L_ONEWAY); // 4224
    static int WallMask   => (1 << L_PLATFORM) | (1 << L_ONEWAY);

    // ── Paths ──────────────────────────────────────────────────────────────
    const string ART      = "Assets/Art/robot_assets";
    const string TILES    = "Assets/Art/Tilemaps";
    const string PREFABS  = "Assets/Prefabs";

    // ─────────────────────────────────────────────────────────────────────
    // MAIN ENTRY POINTS
    // ─────────────────────────────────────────────────────────────────────

    [MenuItem("Robo Rescue/BUILD EVERYTHING")]
    static void BuildEverything()
    {
        EnsureFolders();
        FixPlayerPrefab();
        CreateScrapRollerPrefab();
        CreateRollerSpawnerPrefab();
        CreateLadderZonePrefab();
        CreateKillZonePrefab();
        CreateGoalPrefab();
        CreateWrenchPickupPrefab();
        CreateBatteryPickupPrefab();
        CreateMicrochipPickupPrefab();
        SetupLevel01Scene();
        RemoveDuplicateComponents();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RoboRescue] ✅ All prefabs created and Level01 fully configured!");
    }

    [MenuItem("Robo Rescue/Fix Duplicate Components")]
    static void RemoveDuplicateComponents()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player == null) { Debug.LogWarning("[RoboRescue] Player not found."); return; }

        RemoveDupes<PlayerController>(player);
        RemoveDupes<PlayerHealth>(player);

        // Re-link groundCheck after deduplication
        var gc = player.transform.Find("GroundCheck");
        var pc = player.GetComponent<PlayerController>();
        if (pc != null && gc != null)
        {
            var so = new SerializedObject(pc);
            so.FindProperty("groundCheck").objectReferenceValue = gc;
            so.FindProperty("groundLayer").intValue = GroundMask;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(pc);
        }

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[RoboRescue] Duplicate components removed and scene saved.");
    }

    static void RemoveDupes<T>(GameObject go) where T : Component
    {
        var all = go.GetComponents<T>();
        for (int i = 1; i < all.Length; i++) // keep [0], destroy rest
        {
            Object.DestroyImmediate(all[i]);
            Debug.Log($"[RoboRescue] Removed duplicate {typeof(T).Name}");
        }
    }

    [MenuItem("Robo Rescue/Paint Tilemap Only")]
    static void PaintTilemapMenu()
    {
        SetupTilemap();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[RoboRescue] Tilemap painted and scene saved.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // FOLDERS
    // ─────────────────────────────────────────────────────────────────────

    static void EnsureFolders()
    {
        string[] folders = {
            PREFABS + "/Player",
            PREFABS + "/Enemies",
            PREFABS + "/Props",
            PREFABS + "/Collectibles",
            PREFABS + "/UI",
        };
        foreach (var f in folders)
        {
            var parts = f.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // PLAYER PREFAB
    // ─────────────────────────────────────────────────────────────────────

    static void FixPlayerPrefab()
    {
        string path = PREFABS + "/Player/Player.prefab";
        var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefabAsset == null)
        {
            Debug.LogWarning("[RoboRescue] Player.prefab not found at " + path);
            return;
        }

        // Open prefab for editing
        string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
        var root = PrefabUtility.LoadPrefabContents(prefabPath);

        // Fix Rigidbody2D
        var rb = root.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.gravityScale = 1f;
        }

        // Fix CapsuleCollider2D
        var cap = root.GetComponent<CapsuleCollider2D>();
        if (cap != null)
        {
            cap.size = new Vector2(0.56f, 0.91f);
            cap.direction = CapsuleDirection2D.Vertical;
        }

        // Ensure SpriteRenderer has the idle sprite
        var sr = root.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite == null)
        {
            var spr = LoadSprite("player_idle_1.png");
            if (spr != null) sr.sprite = spr;
        }

        // Tag + layer
        root.tag = "Player";
        root.layer = L_PLAYER;

        // Add PlayerController if missing
        if (root.GetComponent<PlayerController>() == null)
        {
            var pc = root.AddComponent<PlayerController>();
            var soPc = new SerializedObject(pc);
            soPc.FindProperty("groundLayer").intValue = GroundMask;
            soPc.FindProperty("moveSpeed").floatValue = 5f;
            soPc.FindProperty("jumpForce").floatValue = 15f;
            soPc.FindProperty("climbSpeed").floatValue = 3f;
            soPc.ApplyModifiedProperties();
        }
        else
        {
            var pc = root.GetComponent<PlayerController>();
            var soPc = new SerializedObject(pc);
            soPc.FindProperty("groundLayer").intValue = GroundMask;
            soPc.ApplyModifiedProperties();
        }

        // Add PlayerHealth if missing
        if (root.GetComponent<PlayerHealth>() == null)
            root.AddComponent<PlayerHealth>();

        // Add PlayerInput if missing
        var pi = root.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (pi == null)
        {
            pi = root.AddComponent<UnityEngine.InputSystem.PlayerInput>();
            var actions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
                "Assets/InputSystem_Actions.inputactions");
            if (actions != null)
            {
                var soPi = new SerializedObject(pi);
                soPi.FindProperty("m_Actions").objectReferenceValue = actions;
                soPi.FindProperty("m_DefaultActionMap").stringValue = "Player";
                soPi.ApplyModifiedProperties();
            }
        }

        // Add GroundCheck child
        Transform groundCheck = root.transform.Find("GroundCheck");
        if (groundCheck == null)
        {
            var gcObj = new GameObject("GroundCheck");
            gcObj.transform.SetParent(root.transform, false);
            gcObj.transform.localPosition = new Vector3(0f, -0.46f, 0f);
            groundCheck = gcObj.transform;
        }

        // Link groundCheck to PlayerController
        var playerCtrl = root.GetComponent<PlayerController>();
        if (playerCtrl != null)
        {
            var so = new SerializedObject(playerCtrl);
            so.FindProperty("groundCheck").objectReferenceValue = groundCheck;
            so.FindProperty("groundLayer").intValue = GroundMask;
            so.ApplyModifiedProperties();
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("[RoboRescue] Player prefab updated.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // ENEMY PREFABS
    // ─────────────────────────────────────────────────────────────────────

    static void CreateScrapRollerPrefab()
    {
        string path = PREFABS + "/Enemies/ScrapRoller.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("[RoboRescue] ScrapRoller prefab already exists, skipping.");
            return;
        }

        var go = new GameObject("ScrapRoller");
        go.layer = L_ENEMY;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("enemy_scrap_roller.png");

        var rb = go.AddComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 1f;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        var roller = go.AddComponent<ScrapRoller>();
        var so = new SerializedObject(roller);
        so.FindProperty("rollSpeed").floatValue = 3f;
        so.FindProperty("wallLayer").intValue = WallMask;
        so.FindProperty("maxLifetime").floatValue = 30f;
        so.ApplyModifiedProperties();

        SavePrefab(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RoboRescue] ScrapRoller prefab created.");
    }

    static void CreateRollerSpawnerPrefab()
    {
        string path = PREFABS + "/Enemies/RollerSpawner.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("[RoboRescue] RollerSpawner prefab already exists, skipping.");
            return;
        }

        var rollerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Enemies/ScrapRoller.prefab");

        var go = new GameObject("RollerSpawner");
        var spawner = go.AddComponent<RollerSpawner>();
        var so = new SerializedObject(spawner);
        so.FindProperty("rollerPrefab").objectReferenceValue = rollerPrefab;
        so.FindProperty("spawnInterval").floatValue = 3f;
        so.FindProperty("initialDelay").floatValue = 2f;
        so.FindProperty("maxActiveRollers").intValue = 5;
        so.FindProperty("randomizeInterval").boolValue = true;
        so.FindProperty("intervalVariation").floatValue = 1f;
        so.ApplyModifiedProperties();

        SavePrefab(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RoboRescue] RollerSpawner prefab created.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // PROP PREFABS
    // ─────────────────────────────────────────────────────────────────────

    static void CreateLadderZonePrefab()
    {
        string path = PREFABS + "/Props/LadderZone.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("LadderZone");
        go.layer = L_LADDER;

        // Ladder visual (optional sprite)
        var srTop = new GameObject("LadderVisual");
        srTop.transform.SetParent(go.transform, false);
        var sr = srTop.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("ladder_middle.png");
        sr.sortingOrder = -1;
        srTop.transform.localScale = new Vector3(1f, 4f, 1f); // stretch vertically

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 4f);

        go.AddComponent<LadderZone>();

        SavePrefab(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RoboRescue] LadderZone prefab created.");
    }

    static void CreateKillZonePrefab()
    {
        string path = PREFABS + "/Props/KillZone.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("KillZone");

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(30f, 2f);

        var kz = go.AddComponent<KillZone>();
        var so = new SerializedObject(kz);
        so.FindProperty("instantKill").boolValue = true;
        so.FindProperty("destroyEnemies").boolValue = true;
        so.ApplyModifiedProperties();

        SavePrefab(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RoboRescue] KillZone prefab created.");
    }

    static void CreateGoalPrefab()
    {
        string path = PREFABS + "/Props/Goal.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("Goal");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("small_robot_friend.png");
        sr.sortingOrder = 1;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.8f;

        var goal = go.AddComponent<Goal>();
        var so = new SerializedObject(goal);
        so.FindProperty("completionScore").intValue = 1000;
        so.FindProperty("timeBonus").intValue = 500;
        so.FindProperty("completionDelay").floatValue = 2f;
        so.FindProperty("loadNextScene").boolValue = false; // reload same scene for now
        so.ApplyModifiedProperties();

        SavePrefab(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RoboRescue] Goal prefab created.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // COLLECTIBLE PREFABS
    // ─────────────────────────────────────────────────────────────────────

    static void CreateWrenchPickupPrefab()
    {
        string path = PREFABS + "/Collectibles/WrenchPickup.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("WrenchPickup");
        go.layer = L_COLLECTIBLE;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("wrench.png");
        sr.sortingOrder = 1;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        var pickup = go.AddComponent<WrenchPickup>();
        var so = new SerializedObject(pickup);
        so.FindProperty("scoreValue").intValue = 50;
        so.FindProperty("bobSpeed").floatValue = 1.5f;
        so.FindProperty("bobHeight").floatValue = 0.2f;
        so.ApplyModifiedProperties();

        SavePrefab(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RoboRescue] WrenchPickup prefab created.");
    }

    static void CreateBatteryPickupPrefab()
    {
        string path = PREFABS + "/Collectibles/BatteryPickup.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("BatteryPickup");
        go.layer = L_COLLECTIBLE;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("battery.png");
        sr.sortingOrder = 1;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        var pickup = go.AddComponent<BatteryPickup>();
        var so = new SerializedObject(pickup);
        so.FindProperty("scoreValue").intValue = 100;
        so.ApplyModifiedProperties();

        SavePrefab(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RoboRescue] BatteryPickup prefab created.");
    }

    static void CreateMicrochipPickupPrefab()
    {
        string path = PREFABS + "/Collectibles/MicrochipPickup.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

        var go = new GameObject("MicrochipPickup");
        go.layer = L_COLLECTIBLE;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = LoadSprite("microchip.png");
        sr.sortingOrder = 1;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        go.AddComponent<MicrochipPickup>();

        SavePrefab(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[RoboRescue] MicrochipPickup prefab created.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // LEVEL01 SCENE SETUP
    // ─────────────────────────────────────────────────────────────────────

    static void SetupLevel01Scene()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.name.Contains("Level01"))
        {
            Debug.LogWarning("[RoboRescue] Level01 is not the active scene! Open it first.");
            return;
        }

        // ── Tilemap setup ──────────────────────────────────────────────
        SetupTilemap();

        // ── Fix existing Player instance ───────────────────────────────
        FixPlayerInScene();

        // ── GameManager ────────────────────────────────────────────────
        SetupGameManager();

        // ── Camera ─────────────────────────────────────────────────────
        SetupCamera();

        // ── HUD Canvas ─────────────────────────────────────────────────
        SetupHUDCanvas();

        // ── Ladders ───────────────────────────────────────────────────
        PlaceLadders();

        // ── Goal & KillZone ───────────────────────────────────────────
        PlaceGoalAndKillZone();

        // ── RollerSpawner ─────────────────────────────────────────────
        PlaceRollerSpawner();

        // ── Collectibles ──────────────────────────────────────────────
        PlaceCollectibles();

        // ── Directional Light ─────────────────────────────────────────
        EnsureDirectionalLight();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[RoboRescue] Level01 scene fully configured!");
    }

    // ─────────────────────────────────────────────────────────────────────
    // TILEMAP
    // ─────────────────────────────────────────────────────────────────────

    static void SetupTilemap()
    {
        var grid = GameObject.Find("Grid");
        if (grid == null) { Debug.LogError("[RoboRescue] Grid not found in scene!"); return; }

        // GroundTilemap
        var groundTilemapGO = grid.transform.Find("GroundTilemap")?.gameObject
                           ?? grid.transform.Find("Tilemap")?.gameObject;
        if (groundTilemapGO == null)
        {
            groundTilemapGO = new GameObject("GroundTilemap");
            groundTilemapGO.transform.SetParent(grid.transform, false);
            groundTilemapGO.AddComponent<Tilemap>();
            groundTilemapGO.AddComponent<TilemapRenderer>();
        }
        groundTilemapGO.name = "GroundTilemap";
        groundTilemapGO.layer = L_PLATFORM;

        var tm = groundTilemapGO.GetComponent<Tilemap>();

        // Add colliders if missing
        if (groundTilemapGO.GetComponent<TilemapCollider2D>() == null)
        {
            var tc = groundTilemapGO.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
        }
        else
        {
            groundTilemapGO.GetComponent<TilemapCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
        }

        if (groundTilemapGO.GetComponent<Rigidbody2D>() == null)
        {
            var rb = groundTilemapGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (groundTilemapGO.GetComponent<CompositeCollider2D>() == null)
            groundTilemapGO.AddComponent<CompositeCollider2D>();

        // Paint tiles
        var tile = AssetDatabase.LoadAssetAtPath<TileBase>(TILES + "/platform_flat_0.asset");
        if (tile == null)
        {
            Debug.LogWarning("[RoboRescue] platform_flat_0.asset not found, creating placeholder tiles.");
            tile = CreatePlaceholderTile();
        }

        tm.ClearAllTiles();
        PaintRow(tm, tile, -11, 11,  0);  // Ground floor
        PaintRow(tm, tile,  -9,  3,  4);  // Floor 1 (left-heavy)
        PaintRow(tm, tile,  -3,  9,  8);  // Floor 2 (right-heavy)
        PaintRow(tm, tile,  -9,  3, 12);  // Floor 3 (left-heavy)
        PaintRow(tm, tile,  -5,  5, 16);  // Top floor (goal platform)

        EditorUtility.SetDirty(groundTilemapGO);

        // OneWay tilemap (optional – creates it but leaves it empty for manual use)
        var owGO = grid.transform.Find("OneWayTilemap")?.gameObject;
        if (owGO == null)
        {
            owGO = new GameObject("OneWayTilemap");
            owGO.transform.SetParent(grid.transform, false);
            owGO.layer = L_ONEWAY;
            owGO.AddComponent<Tilemap>();
            owGO.AddComponent<TilemapRenderer>();
            var tc2 = owGO.AddComponent<TilemapCollider2D>();
            tc2.compositeOperation = Collider2D.CompositeOperation.Merge;
            var rb2 = owGO.AddComponent<Rigidbody2D>();
            rb2.bodyType = RigidbodyType2D.Static;
            var cc = owGO.AddComponent<CompositeCollider2D>();
            cc.geometryType = CompositeCollider2D.GeometryType.Polygons;
            var eff = owGO.AddComponent<PlatformEffector2D>();
            eff.useOneWay = true;
            eff.rotationalOffset = 0f;
        }

        Debug.Log("[RoboRescue] Tilemap configured.");
    }

    static void PaintRow(Tilemap tm, TileBase tile, int xMin, int xMax, int y)
    {
        for (int x = xMin; x <= xMax; x++)
            tm.SetTile(new Vector3Int(x, y, 0), tile);
    }

    static TileBase CreatePlaceholderTile()
    {
        var t = ScriptableObject.CreateInstance<Tile>();
        t.color = new Color(0.4f, 0.3f, 0.2f);
        AssetDatabase.CreateAsset(t, TILES + "/placeholder_tile.asset");
        return t;
    }

    // ─────────────────────────────────────────────────────────────────────
    // FIX PLAYER IN SCENE
    // ─────────────────────────────────────────────────────────────────────

    static void FixPlayerInScene()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("[RoboRescue] Player not found in scene. Placing from prefab.");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Player/Player.prefab");
            if (prefab == null) { Debug.LogError("[RoboRescue] Player.prefab missing!"); return; }
            player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.transform.position = new Vector3(-6f, 1f, 0f);
        }
        else
        {
            player.transform.position = new Vector3(-6f, 1f, 0f);
        }

        player.tag = "Player";
        player.layer = L_PLAYER;

        // Ensure GroundCheck child exists
        var gc = player.transform.Find("GroundCheck");
        if (gc == null)
        {
            var gcGO = new GameObject("GroundCheck");
            gcGO.transform.SetParent(player.transform, false);
            gcGO.transform.localPosition = new Vector3(0f, -0.46f, 0f);
            gc = gcGO.transform;
        }

        // Fix PlayerController
        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            var so = new SerializedObject(pc);
            so.FindProperty("groundLayer").intValue = GroundMask;
            so.FindProperty("groundCheck").objectReferenceValue = gc;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(pc);
        }

        // Fix Rigidbody2D constraints
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            EditorUtility.SetDirty(rb);
        }

        Debug.Log("[RoboRescue] Player in scene fixed.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // GAME MANAGER
    // ─────────────────────────────────────────────────────────────────────

    static void SetupGameManager()
    {
        var existing = Object.FindFirstObjectByType<GameManager>();
        if (existing != null)
        {
            Debug.Log("[RoboRescue] GameManager already exists.");
            return;
        }

        var go = new GameObject("GameManager");
        var gm = go.AddComponent<GameManager>();
        var so = new SerializedObject(gm);
        so.FindProperty("startingLives").intValue = 3;
        so.FindProperty("levelTimeLimit").floatValue = 300f;
        so.FindProperty("useTimer").boolValue = true;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(go);
        Debug.Log("[RoboRescue] GameManager created.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // CAMERA
    // ─────────────────────────────────────────────────────────────────────

    static void SetupCamera()
    {
        var camGO = GameObject.Find("Main Camera");
        if (camGO == null)
        {
            camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }

        camGO.transform.position = new Vector3(0f, 8f, -10f);

        // Camera settings for 2D arcade
        var cam = camGO.GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 9f;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.15f);
        }

        // CameraFollow2D
        var follow = camGO.GetComponent<CameraFollow2D>();
        if (follow == null) follow = camGO.AddComponent<CameraFollow2D>();

        var so = new SerializedObject(follow);
        so.FindProperty("autoFindPlayer").boolValue = true;
        so.FindProperty("smoothSpeed").floatValue = 5f;
        so.FindProperty("useDeadZone").boolValue = true;
        so.FindProperty("deadZoneSize").vector2Value = new Vector2(1.5f, 1f);
        so.FindProperty("useBounds").boolValue = true;
        so.FindProperty("minBounds").vector2Value = new Vector2(-12f, -2f);
        so.FindProperty("maxBounds").vector2Value = new Vector2(12f, 18f);
        so.FindProperty("lookAheadDistance").floatValue = 2f;
        so.FindProperty("followX").boolValue = true;
        so.FindProperty("followY").boolValue = true;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(follow);
        Debug.Log("[RoboRescue] Camera configured.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // HUD CANVAS
    // ─────────────────────────────────────────────────────────────────────

    static void SetupHUDCanvas()
    {
        if (Object.FindFirstObjectByType<HUDController>() != null)
        {
            Debug.Log("[RoboRescue] HUD already exists.");
            return;
        }

        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // HUD Panel
        var hudGO = new GameObject("HUD");
        hudGO.transform.SetParent(canvasGO.transform, false);
        var hudRect = hudGO.AddComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;

        // Score text
        var scoreGO = CreateTMPText(hudGO.transform, "ScoreText",
            "SCORE: 000000", 14, TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -10f));

        // Lives text
        var livesGO = CreateTMPText(hudGO.transform, "LivesText",
            "LIVES: 3", 14, TextAnchor.UpperCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f));

        // Timer text
        var timerGO = CreateTMPText(hudGO.transform, "TimerText",
            "TIME: 300", 14, TextAnchor.UpperRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -10f));

        // Power-up indicator
        var powerGO = new GameObject("PowerUpIndicator");
        powerGO.transform.SetParent(hudGO.transform, false);
        var powerRect = powerGO.AddComponent<RectTransform>();
        powerRect.anchorMin = new Vector2(0f, 0f);
        powerRect.anchorMax = new Vector2(0f, 0f);
        powerRect.pivot = new Vector2(0f, 0f);
        powerRect.anchoredPosition = new Vector2(20f, 20f);
        powerRect.sizeDelta = new Vector2(60f, 60f);
        var powerImg = powerGO.AddComponent<Image>();
        powerImg.color = new Color(1f, 0.8f, 0f, 1f);
        powerGO.SetActive(false);

        // HUDController
        var hud = hudGO.AddComponent<HUDController>();
        var so = new SerializedObject(hud);
        so.FindProperty("scoreText").objectReferenceValue = scoreGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("livesText").objectReferenceValue = livesGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("timerText").objectReferenceValue = timerGO.GetComponent<TextMeshProUGUI>();
        so.FindProperty("powerUpIndicator").objectReferenceValue = powerGO;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(hud);

        Debug.Log("[RoboRescue] HUD Canvas created.");
    }

    static GameObject CreateTMPText(Transform parent, string name, string text,
        float fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(300f, 40f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        return go;
    }

    // ─────────────────────────────────────────────────────────────────────
    // LADDERS
    // ─────────────────────────────────────────────────────────────────────

    [MenuItem("Robo Rescue/Fix Ladders")]
    static void FixLaddersMenu()
    {
        PlaceLadders();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[RoboRescue] Ladders fixed and scene saved.");
    }

    static void PlaceLadders()
    {
        // Destroy any existing Ladders container so we can rebuild cleanly
        var existing = GameObject.Find("Ladders");
        if (existing != null) Object.DestroyImmediate(existing);

        var ladderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Props/LadderZone.prefab");
        if (ladderPrefab == null) { Debug.LogError("[RoboRescue] LadderZone prefab missing!"); return; }

        var container = new GameObject("Ladders");

        // Platform world-space top surfaces (tiles at cell y paint to world y..y+1):
        //   Floor 0 top = y=1, Floor 1 top = y=5, Floor 2 top = y=9,
        //   Floor 3 top = y=13, Top top = y=17
        // Platform world-space x extents (cell x paints to world x..x+1):
        //   Floor 1: x=-9..4  Floor 2: x=-3..10  Floor 3: x=-9..4  Top: x=-5..6
        // Ladder x must lie within the x-overlap of the two platforms it connects.
        //   Floor1-Floor2 overlap: x=-3..4   → use x=-2 or x=2
        //   Floor2-Floor3 overlap: x=-3..4   → use x=2 or x=-2
        //   Floor3-Top overlap:    x=-5..4   → use x=-2
        // Ladder height spans from lower floor top to 0.5 above upper floor top.
        var ladders = new (float x, float yBot, float h)[]
        {
            ( 2f,  1f, 4.5f),  // Floor 0 → Floor 1 (right side, x=2 in floor1 x-range)
            (-2f,  5f, 4.5f),  // Floor 1 → Floor 2 (left side,  x=-2 in overlap -3..4)
            ( 2f,  9f, 4.5f),  // Floor 2 → Floor 3 (right side, x=2  in overlap -3..4)
            (-2f, 13f, 4.5f),  // Floor 3 → Top     (left side,  x=-2 in overlap -5..4)
        };

        foreach (var (x, yBot, h) in ladders)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(ladderPrefab);
            inst.transform.SetParent(container.transform, false);
            inst.transform.position = new Vector3(x, yBot + h * 0.5f, 0f);

            var col = inst.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(0.8f, h);

            // Stretch the visual child
            var vis = inst.transform.Find("LadderVisual");
            if (vis != null) vis.localScale = new Vector3(1f, h, 1f);
        }

        Debug.Log("[RoboRescue] Ladders placed.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // GOAL & KILLZONE
    // ─────────────────────────────────────────────────────────────────────

    static void PlaceGoalAndKillZone()
    {
        // Goal
        if (Object.FindFirstObjectByType<Goal>() == null)
        {
            var goalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Props/Goal.prefab");
            if (goalPrefab != null)
            {
                var g = (GameObject)PrefabUtility.InstantiatePrefab(goalPrefab);
                g.transform.position = new Vector3(0f, 17.5f, 0f);
            }
        }

        // KillZone
        if (Object.FindFirstObjectByType<KillZone>() == null)
        {
            var kzPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Props/KillZone.prefab");
            if (kzPrefab != null)
            {
                var k = (GameObject)PrefabUtility.InstantiatePrefab(kzPrefab);
                k.transform.position = new Vector3(0f, -2f, 0f);
            }
        }

        Debug.Log("[RoboRescue] Goal and KillZone placed.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // ROLLER SPAWNER
    // ─────────────────────────────────────────────────────────────────────

    static void PlaceRollerSpawner()
    {
        if (Object.FindFirstObjectByType<RollerSpawner>() != null)
        {
            Debug.Log("[RoboRescue] RollerSpawner already in scene.");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Enemies/RollerSpawner.prefab");
        if (prefab == null) { Debug.LogError("[RoboRescue] RollerSpawner prefab missing!"); return; }

        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        inst.transform.position = new Vector3(4f, 17.5f, 0f);
        Debug.Log("[RoboRescue] RollerSpawner placed at top of level.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // COLLECTIBLES
    // ─────────────────────────────────────────────────────────────────────

    static void PlaceCollectibles()
    {
        if (GameObject.Find("Collectibles") != null)
        {
            Debug.Log("[RoboRescue] Collectibles container already exists.");
            return;
        }

        var container = new GameObject("Collectibles");

        var wrenchPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Collectibles/WrenchPickup.prefab");
        var battPrefab    = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Collectibles/BatteryPickup.prefab");
        var chipPrefab    = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS + "/Collectibles/MicrochipPickup.prefab");

        // Wrenches on mid-platforms
        Vector3[] wrenchPos = {
            new Vector3( 0f,  5f, 0f),
            new Vector3(-1f,  9f, 0f),
            new Vector3( 2f, 13f, 0f),
        };
        foreach (var pos in wrenchPos)
        {
            if (wrenchPrefab == null) break;
            var w = (GameObject)PrefabUtility.InstantiatePrefab(wrenchPrefab);
            w.transform.SetParent(container.transform, false);
            w.transform.position = pos;
        }

        // Battery (extra life) - hidden on floor 2
        if (battPrefab != null)
        {
            var b = (GameObject)PrefabUtility.InstantiatePrefab(battPrefab);
            b.transform.SetParent(container.transform, false);
            b.transform.position = new Vector3(6f, 9f, 0f);
        }

        // Microchips scattered for bonus points
        Vector3[] chipPos = {
            new Vector3(-5f,  5f, 0f),
            new Vector3( 5f, 13f, 0f),
        };
        foreach (var pos in chipPos)
        {
            if (chipPrefab == null) break;
            var c = (GameObject)PrefabUtility.InstantiatePrefab(chipPrefab);
            c.transform.SetParent(container.transform, false);
            c.transform.position = pos;
        }

        Debug.Log("[RoboRescue] Collectibles placed.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // DIRECTIONAL LIGHT
    // ─────────────────────────────────────────────────────────────────────

    static void EnsureDirectionalLight()
    {
        if (Object.FindFirstObjectByType<Light>() != null) return;
        var go = new GameObject("Directional Light");
        go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.color = Color.white;
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────

    static Sprite LoadSprite(string filename)
    {
        // Try single-sprite load first
        var spr = AssetDatabase.LoadAssetAtPath<Sprite>(ART + "/" + filename);
        if (spr != null) return spr;

        // Try loading all sub-assets (sliced sprites)
        var all = AssetDatabase.LoadAllAssetsAtPath(ART + "/" + filename);
        foreach (var a in all)
            if (a is Sprite s) return s;

        Debug.LogWarning("[RoboRescue] Sprite not found: " + filename);
        return null;
    }

    static void SavePrefab(GameObject go, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(go, path);
    }
}
