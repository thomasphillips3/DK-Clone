using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-50)]
public class BeatReactSystem : MonoBehaviour
{
    [SerializeField] private float shakeStrength = 0.15f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float dropShakeMultiplier = 2.5f;
    [SerializeField] private Color beatFlashColor = new Color(1f, 1f, 1f, 0.15f);
    [SerializeField] private ParticleSystem dropParticles;

    private Camera mainCam;
    private Vector3 camOriginalLocalPos;
    private MaterialPropertyBlock propBlock;
    private List<Renderer> beatRenderers = new List<Renderer>();

    void Awake()
    {
        mainCam = Camera.main;
        propBlock = new MaterialPropertyBlock();
        SubscribeToAudio();
    }

    void SubscribeToAudio()
    {
        if (AudioSyncManager.instance == null)
        {
            Invoke(nameof(SubscribeToAudio), 0.1f);
            return;
        }
        AudioSyncManager.instance.OnBeat += HandleBeat;
        AudioSyncManager.instance.OnDrop += HandleDrop;
    }

    void OnDestroy()
    {
        if (AudioSyncManager.instance != null)
        {
            AudioSyncManager.instance.OnBeat -= HandleBeat;
            AudioSyncManager.instance.OnDrop -= HandleDrop;
        }
    }

    public void RegisterRenderer(Renderer r)
    {
        if (r != null && !beatRenderers.Contains(r)) beatRenderers.Add(r);
    }

    void HandleBeat()
    {
        FlashRenderers(beatFlashColor);
        StartCoroutine(ScreenShake(shakeStrength, shakeDuration));
    }

    void HandleDrop()
    {
        FlashRenderers(beatFlashColor * 2f);
        StartCoroutine(ScreenShake(shakeStrength * dropShakeMultiplier, shakeDuration * 1.5f));
        if (dropParticles != null) dropParticles.Play();
        HapticManager.Beat();
    }

    void FlashRenderers(Color flashColor)
    {
        foreach (var r in beatRenderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", flashColor);
            r.SetPropertyBlock(propBlock);
        }
        StartCoroutine(ResetRendererColors());
    }

    IEnumerator ResetRendererColors()
    {
        yield return new WaitForSeconds(0.05f);
        foreach (var r in beatRenderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", Color.white);
            r.SetPropertyBlock(propBlock);
        }
    }

    IEnumerator ScreenShake(float strength, float duration)
    {
        if (mainCam == null) yield break;
        camOriginalLocalPos = mainCam.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float damped = Mathf.SmoothStep(strength, 0f, elapsed / duration);
            mainCam.transform.localPosition = camOriginalLocalPos + (Vector3)Random.insideUnitCircle * damped;
            yield return null;
        }
        mainCam.transform.localPosition = camOriginalLocalPos;
    }
}
