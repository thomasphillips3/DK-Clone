
# Feb 28 Changes — Time Off 3 Pivot (Rhythm-Reactive Endless Runner)

This update pivots Level 1 away from the Donkey Kong clone and into a **rhythm-reactive endless runner** (Chrome T-Rex vibe) where the **song is the timer** and gameplay reacts to beat events.

---

## Goal (What we’re building)

- **Endless runner** for the duration of the track.
- Patchy runs; player taps to **jump** (optional swipe down later).
- Obstacles spawn in **sync** with the track via **BeatMap events**.
- Score = survival time + obstacle clears (simple start).
- When the song ends → results screen → load next track/minigame.

**Key idea:** drive all timing off `AudioSource.time` for stable sync.

---

## High-level Architecture

### Scene Objects (Level01)
Create/keep these:

- `Main Camera` (add CameraPulse)
- `Music` (AudioSource + BeatScheduler)
- `GameManager` (RunnerGameManager + RunnerSpeed)
- `Spawner` (ObstacleSpawner)
- `Ground` (BoxCollider2D)
- `Patchy` (SpriteRenderer + Rigidbody2D + Collider2D + PatchyRunnerController)
- `Canvas` (Score UI + End panel)

### Scripts (drop into `Assets/Scripts/`)
- `BeatMap.cs` (ScriptableObject)
- `BeatScheduler.cs` (fires events based on `AudioSource.time`)
- `RunnerGameManager.cs` (handles beat events → spawns + speed + camera bump)
- `RunnerSpeed.cs` (global speed multiplier)
- `ObstacleSpawner.cs`
- `ObstacleMover.cs`
- `PatchyRunnerController.cs`
- `CameraPulse.cs`
- `ScoreManager.cs` (simple scoring)
- `TrackEndController.cs` (end-of-song → results + next)
- **Editor tool:** `BeatMapGeneratorWindow.cs` (auto-generate beats from BPM + offset)

---

## Folder Structure (recommended)

```text
Assets/
  Audio/
    Track_01_while_it_counts.wav
  Art/
    Level1/
      bg_workbench.png
    Patchy/
      patchy_idle.png
  Prefabs/
    Obstacles/
  Scenes/
    Level01.unity
  Data/
    BeatMaps/
      Track01_BeatMap.asset
  Scripts/
    (all .cs files below)
  Editor/
    BeatMapGeneratorWindow.cs
```

---

## Implementation Steps (Do these in order)

### 1) Convert the scene to runner layout
- Delete DK clone geometry you don’t need (ladders/platform stacks).
- Create a single `Ground` across bottom:
  - `BoxCollider2D` (isTrigger OFF)
- Place `Patchy` on ground:
  - `Rigidbody2D` (Gravity Scale ~ 3–5)
  - `CapsuleCollider2D` (or BoxCollider2D)
  - Add `PatchyRunnerController` and assign `groundCheck` child transform + ground layer mask.

### 2) Create obstacles
- Make 2–4 simple obstacle prefabs:
  - `ResistorRoll` (basic)
  - `CapacitorDrop` (basic)
  - `TapeReel` (accent)
- Each obstacle prefab needs:
  - `SpriteRenderer`
  - `Collider2D` (isTrigger OFF)
  - `ObstacleMover` script

Put them in `Assets/Prefabs/Obstacles/`.

### 3) Add Audio + Beat Scheduler
- Create `Music` GameObject:
  - `AudioSource` (clip = Track_01_while_it_counts.wav, Play On Awake ON, Loop OFF)
  - `BeatScheduler` (assign AudioSource + BeatMap asset)

### 4) Generate BeatMap from BPM
- Use editor window (below) to generate beats:
  - `Tools → TimeOff3 → BeatMap Generator`
  - Select audio clip, set BPM, set offset, duration, and generate.
- Optional: add `Section` events manually in inspector later.

### 5) Wire GameManager + Spawner
- `GameManager`:
  - Add `RunnerSpeed` and `RunnerGameManager`
  - Assign `BeatScheduler`, `ObstacleSpawner`, `RunnerSpeed`, `CameraPulse`
- `Spawner`:
  - Add `ObstacleSpawner`
  - Assign `RunnerSpeed`, `spawnPoint`, and obstacle arrays.

### 6) Add Score + End-of-track flow
- Add `ScoreManager` and `TrackEndController`
- Show results when the track ends and optionally auto-load next scene.

---

## CODE – Copy these files exactly

> Create these files under `Assets/Scripts/` (except the Editor tool).

---

### `BeatMap.cs`
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public enum BeatEventType { Beat, Kick, Snare, Hat, Section }

[Serializable]
public class BeatEvent
{
    public float time;
    public BeatEventType type;
    public int value; // optional (e.g., section id/intensity)
}

[CreateAssetMenu(menuName="TimeOff3/BeatMap")]
public class BeatMap : ScriptableObject
{
    public AudioClip clip;
    public List<BeatEvent> events = new();
}
```

---

### `BeatScheduler.cs`
```csharp
using UnityEngine;

public class BeatScheduler : MonoBehaviour
{
    public AudioSource source;
    public BeatMap beatMap;

    public System.Action<BeatEvent> OnEvent;

    int idx;

    void Start()
    {
        idx = 0;

        if (source && source.clip == null && beatMap && beatMap.clip)
            source.clip = beatMap.clip;

        if (source && !source.isPlaying)
            source.Play();
    }

    void Update()
    {
        if (!source || !source.isPlaying || beatMap == null) return;

        float t = source.time;

        // Catch up if frames skip
        while (idx < beatMap.events.Count && beatMap.events[idx].time <= t)
        {
            OnEvent?.Invoke(beatMap.events[idx]);
            idx++;
        }
    }

    public void ResetSchedule()
    {
        idx = 0;
    }
}
```

---

### `RunnerSpeed.cs`
```csharp
using UnityEngine;

public class RunnerSpeed : MonoBehaviour
{
    public float baseSpeed = 6f;
    public float multiplier = 1f;

    public float Current => baseSpeed * multiplier;

    public void Nudge(float amount)
        => multiplier = Mathf.Clamp(multiplier + amount, 0.8f, 2.0f);

    public void SetMultiplier(float m)
        => multiplier = Mathf.Clamp(m, 0.8f, 2.5f);
}
```

---

### `ObstacleMover.cs`
```csharp
using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    public RunnerSpeed speed;
    public float destroyX = -20f;

    void Update()
    {
        if (!speed) return;

        transform.position += Vector3.left * speed.Current * Time.deltaTime;

        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }
}
```

---

### `ObstacleSpawner.cs`
```csharp
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public RunnerSpeed speed;
    public Transform spawnPoint;

    public GameObject[] basicPrefabs;
    public GameObject[] accentPrefabs;

    int pattern = 0;

    public void SetPattern(int p) => pattern = p;

    public void SpawnBasic()
    {
        SpawnFrom(basicPrefabs);

        // Occasionally double-spawn on higher patterns
        if (pattern >= 2 && Random.value < 0.25f)
            SpawnFrom(basicPrefabs);
    }

    public void SpawnAccent()
    {
        SpawnFrom(accentPrefabs);
    }

    void SpawnFrom(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0 || !spawnPoint) return;

        var prefab = prefabs[Random.Range(0, prefabs.Length)];
        var go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        var mover = go.GetComponent<ObstacleMover>();
        if (mover) mover.speed = speed;
    }
}
```

---

### `CameraPulse.cs`
```csharp
using UnityEngine;

public class CameraPulse : MonoBehaviour
{
    Vector3 basePos;
    float strength;

    void Start()
    {
        basePos = transform.position;
    }

    public void Bump(float s)
    {
        strength = Mathf.Clamp(strength + s, 0f, 0.5f);
    }

    void Update()
    {
        strength = Mathf.Lerp(strength, 0f, Time.deltaTime * 6f);
        transform.position = basePos + (Vector3)(Random.insideUnitCircle * strength * 0.1f);
    }
}
```

---

### `PatchyRunnerController.cs`
```csharp
using UnityEngine;

public class PatchyRunnerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public float jumpForce = 12f;

    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundRadius = 0.15f;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Tap/click to jump
        if (Input.GetMouseButtonDown(0))
        {
            if (IsGrounded())
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask) != null;
    }
}
```

---

### `ScoreManager.cs`
```csharp
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int score;
    public float secondsAlive;

    public Text scoreText; // optional (legacy UI). For TMP, swap type.

    public void Add(int amount)
    {
        score += amount;
        UpdateUI();
    }

    void Update()
    {
        secondsAlive += Time.deltaTime;
        // 1 point per second alive
        score = Mathf.Max(score, Mathf.FloorToInt(secondsAlive));
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = score.ToString();
    }
}
```

---

### `RunnerGameManager.cs`
```csharp
using UnityEngine;

public class RunnerGameManager : MonoBehaviour
{
    public BeatScheduler scheduler;
    public ObstacleSpawner spawner;
    public RunnerSpeed speed;
    public CameraPulse cameraPulse;
    public ScoreManager score;

    void Awake()
    {
        if (scheduler != null)
            scheduler.OnEvent += HandleBeatEvent;
    }

    void HandleBeatEvent(BeatEvent e)
    {
        switch (e.type)
        {
            case BeatEventType.Beat:
            case BeatEventType.Kick:
                spawner.SpawnBasic();
                speed.Nudge(0.02f);
                score?.Add(1);
                break;

            case BeatEventType.Snare:
                cameraPulse?.Bump(0.15f);
                spawner.SpawnAccent();
                score?.Add(3);
                break;

            case BeatEventType.Section:
                // e.value can represent section intensity 0..N
                speed.SetMultiplier(1f + (0.1f * e.value));
                spawner.SetPattern(e.value);
                break;
        }
    }
}
```

---

### `TrackEndController.cs`
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrackEndController : MonoBehaviour
{
    public AudioSource source;
    public GameObject endPanel; // assign a UI panel
    public float autoNextDelay = 2.5f;
    public string nextSceneName = ""; // optional

    bool ended;

    void Update()
    {
        if (ended) return;
        if (!source || source.clip == null) return;

        if (!source.isPlaying && source.time > 0.1f)
        {
            EndRun();
        }
        else if (source.time >= source.clip.length - 0.02f)
        {
            EndRun();
        }
    }

    void EndRun()
    {
        ended = true;

        if (endPanel) endPanel.SetActive(true);

        // Optional auto-load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
            Invoke(nameof(LoadNext), autoNextDelay);
    }

    void LoadNext()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
```

---

## EDITOR TOOL – BeatMap Generator (Auto-generate beats)

> Create this file under `Assets/Editor/BeatMapGeneratorWindow.cs`

### `BeatMapGeneratorWindow.cs`
```csharp
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
```

---

## Wiring Checklist (Inspector)

### `Patchy`
- Rigidbody2D (Gravity Scale 3–5)
- Collider2D
- PatchyRunnerController:
  - `rb` assigned
  - `groundCheck` assigned (create child transform at feet)
  - `groundMask` set to “Ground” layer

### `Music`
- AudioSource:
  - clip = Track_01_while_it_counts.wav
  - Play On Awake ON, Loop OFF
- BeatScheduler:
  - source = AudioSource
  - beatMap = Track01_BeatMap.asset

### `GameManager`
- RunnerSpeed (baseSpeed 6)
- RunnerGameManager:
  - scheduler = Music/BeatScheduler
  - spawner = Spawner/ObstacleSpawner
  - speed = RunnerSpeed
  - cameraPulse = Main Camera/CameraPulse
  - score = Canvas/ScoreManager

### `Spawner`
- ObstacleSpawner:
  - speed = GameManager/RunnerSpeed
  - spawnPoint = child transform at right side of camera view (x ~ +10)
  - basicPrefabs = resistor/capacitor
  - accentPrefabs = tape reel/etc.

### `Main Camera`
- CameraPulse attached

### `End Panel`
- A UI panel (disabled by default)
- TrackEndController:
  - source = Music/AudioSource
  - endPanel = your panel
  - nextSceneName optional

---

## Notes for Claude Code (what to do next)
1) Implement collision handling for death (player hits obstacle → freeze & show end panel).
2) Add difficulty shaping: section events, density changes, speed curves.
3) Replace placeholder obstacles with Level 1 “workbench” themed sprites.
4) Add per-track skins: each track uses same runner core but different background + obstacles + palette.

---

## Optional: Collision Death (add to Patchy)
If you want instant fail on collision with obstacles, add this to Patchy:

```csharp
using UnityEngine;

public class PatchyDeath : MonoBehaviour
{
    public AudioSource source;
    public GameObject endPanel;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Obstacle"))
        {
            if (source) source.Pause();
            if (endPanel) endPanel.SetActive(true);
            Time.timeScale = 0f; // optional freeze
        }
    }
}
```

Tag obstacles with `Obstacle` and ensure colliders are not triggers.

---

## Done ✅
This pack is sufficient to:
- Generate beat events from BPM
- Schedule them during playback
- Spawn obstacles and modulate speed
- Score the run
- End when the track ends