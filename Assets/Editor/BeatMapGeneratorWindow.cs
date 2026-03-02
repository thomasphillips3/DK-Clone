using UnityEditor;
using UnityEngine;

public class BeatMapGeneratorWindow : EditorWindow
{
    AudioClip clip;
    BeatMap target;
    float bpm = 90f;
    float offsetSeconds = 0f;
    float maxSeconds = 0f; // 0 = use clip length
    BeatEventType type = BeatEventType.Beat;

    [MenuItem("Tools/TimeOff3/BeatMap Generator")]
    public static void ShowWindow()
    {
        GetWindow<BeatMapGeneratorWindow>("BeatMap Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Generate Beat Events from BPM", EditorStyles.boldLabel);

        clip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", clip, typeof(AudioClip), false);
        target = (BeatMap)EditorGUILayout.ObjectField("Target BeatMap", target, typeof(BeatMap), false);

        bpm = EditorGUILayout.FloatField("BPM", bpm);
        offsetSeconds = EditorGUILayout.FloatField("Offset (sec)", offsetSeconds);
        maxSeconds = EditorGUILayout.FloatField("Max Seconds (0=clip)", maxSeconds);
        type = (BeatEventType)EditorGUILayout.EnumPopup("Event Type", type);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Generate"))
        {
            if (!target)
            {
                EditorUtility.DisplayDialog("Missing BeatMap", "Create/select a BeatMap asset as target.", "OK");
                return;
            }
            if (!clip && !target.clip)
            {
                EditorUtility.DisplayDialog("Missing Clip", "Assign an AudioClip (or set it on the BeatMap).", "OK");
                return;
            }

            if (!clip) clip = target.clip;
            target.clip = clip;

            Undo.RecordObject(target, "Generate BeatMap");
            target.events.Clear();

            float secondsPerBeat = 60f / Mathf.Max(1f, bpm);
            float duration = (maxSeconds > 0f) ? maxSeconds : clip.length;

            float t = Mathf.Max(0f, offsetSeconds);
            while (t <= duration)
            {
                target.events.Add(new BeatEvent { time = t, type = type, value = 0 });
                t += secondsPerBeat;
            }

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Done", $"Generated {target.events.Count} events.", "OK");
        }
    }
}
