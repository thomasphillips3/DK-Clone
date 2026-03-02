using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance { get; private set; }

    [Header("VHS Wipe Material (optional)")]
    [SerializeField] private Material vhsWipeMaterial;

    private Image overlayImage;
    private bool isTransitioning;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        CreateOverlay();
    }

    void CreateOverlay()
    {
        var canvasGO = new GameObject("_TransitionCanvas");
        canvasGO.transform.SetParent(transform);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasGO.AddComponent<CanvasScaler>();

        var imgGO = new GameObject("_Overlay");
        imgGO.transform.SetParent(canvasGO.transform, false);

        overlayImage = imgGO.AddComponent<Image>();
        var rt = overlayImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        if (vhsWipeMaterial != null)
            overlayImage.material = new Material(vhsWipeMaterial);
        else
            overlayImage.color = Color.black;

        SetProgress(0f);
        overlayImage.gameObject.SetActive(false);
    }

    public void TransitionToScene(string sceneName)
    {
        if (!isTransitioning)
            StartCoroutine(TransitionCoroutine(sceneName));
    }

    IEnumerator TransitionCoroutine(string sceneName)
    {
        isTransitioning = true;
        overlayImage.gameObject.SetActive(true);

        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetProgress(Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }
        SetProgress(1f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op != null) yield return op;

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetProgress(Mathf.SmoothStep(1f, 0f, elapsed / duration));
            yield return null;
        }
        SetProgress(0f);

        overlayImage.gameObject.SetActive(false);
        isTransitioning = false;
    }

    void SetProgress(float value)
    {
        if (overlayImage == null) return;
        if (vhsWipeMaterial != null && overlayImage.material != null)
            overlayImage.material.SetFloat("_Progress", value);
        else
        {
            Color c = Color.black;
            c.a = value;
            overlayImage.color = c;
        }
    }
}
