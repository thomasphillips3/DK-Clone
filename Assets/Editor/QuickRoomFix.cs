using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Quick fix: adds RecordingStudio model to all rooms, removes old Walls,
/// fixes camera backgrounds and render settings.
/// Run from Tools > Quick Fix All Rooms.
/// </summary>
public static class QuickRoomFix
{
    static readonly (string scene, float rotY)[] rooms = new[]
    {
        ("Room_01_LiveRoom", 0f),
        ("Room_02_ControlRoom", 0f),
        ("Room_03_VocalBooth", 90f),
        ("Room_04_EquipmentCloset", 180f),
        ("Room_05_TapeMachineRoom", 270f),
        ("Room_06_Lounge", 45f),
        ("Room_07_EchoChamber", 135f),
        ("Room_08_Rooftop", 0f),
    };

    [MenuItem("Tools/Quick Fix All Rooms")]
    public static void FixAllRooms()
    {
        var studioModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Equipment/RecordingStudio.dae");
        if (studioModel == null)
        {
            Debug.LogError("[QuickFix] RecordingStudio.dae not found!");
            return;
        }

        foreach (var room in rooms)
        {
            string path = $"Assets/Scenes/{room.scene}.unity";
            Debug.Log($"[QuickFix] Fixing {room.scene}...");

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // Remove old Walls if present
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "Walls" || go.name == "Ceiling")
                    Object.DestroyImmediate(go);
            }

            // Check if RecordingStudio already exists
            bool hasStudio = false;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "RecordingStudio")
                {
                    hasStudio = true;
                    // Fix rotation
                    go.transform.rotation = Quaternion.Euler(0, room.rotY, 0);
                    // Add colliders if missing
                    foreach (var mf in go.GetComponentsInChildren<MeshFilter>())
                    {
                        if (mf.GetComponent<Collider>() == null)
                            mf.gameObject.AddComponent<MeshCollider>();
                    }
                    break;
                }
            }

            if (!hasStudio)
            {
                // Instantiate model - use Instantiate since PrefabUtility may fail with .dae
                var instance = Object.Instantiate(studioModel);
                instance.name = "RecordingStudio";
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.Euler(0, room.rotY, 0);
                instance.isStatic = true;
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(instance, scene);

                // Add colliders
                foreach (var mf in instance.GetComponentsInChildren<MeshFilter>())
                {
                    if (mf.GetComponent<Collider>() == null)
                        mf.gameObject.AddComponent<MeshCollider>();
                }
            }

            // Fix all cameras in scene
            foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.04f, 0.03f, 0.02f);
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 50f;
                cam.fieldOfView = 65f;
            }

            // Render settings
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.06f, 0.05f, 0.035f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.05f;
            RenderSettings.fogColor = new Color(0.03f, 0.025f, 0.02f);

            // Ensure dust particles exist
            bool hasDust = false;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "DustParticles") { hasDust = true; break; }
            }
            if (!hasDust)
            {
                var dustGO = new GameObject("DustParticles");
                var ps = dustGO.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startColor = new Color(1f, 0.9f, 0.7f, 0.1f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.04f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(0.005f, 0.02f);
                main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 14f);
                main.maxParticles = 250;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                var em = ps.emission;
                em.rateOverTime = 30f;
                var sh = ps.shape;
                sh.shapeType = ParticleSystemShapeType.Box;
                sh.scale = new Vector3(8, 4, 8);
                dustGO.transform.position = new Vector3(0, 2, 0);

                var psr = dustGO.GetComponent<ParticleSystemRenderer>();
                var dustMat = new Material(Shader.Find("Particles/Standard Unlit"));
                dustMat.SetColor("_Color", new Color(1f, 0.9f, 0.7f, 0.1f));
                psr.material = dustMat;
            }

            // Ensure InGameCanvas exists
            bool hasCanvas = false;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "InGameCanvas") { hasCanvas = true; break; }
            }
            if (!hasCanvas)
            {
                var canvasGO = new GameObject("InGameCanvas");
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[QuickFix] {room.scene} done");
        }

        Debug.Log("[QuickFix] All 8 rooms fixed!");
    }
}
