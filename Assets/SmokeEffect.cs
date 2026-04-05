using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SmokeEffect : MonoBehaviour
{
    [Range(0f, 1f)]
    public float maxSmokeAlpha       = 0.75f;
    public float smokeDuration       = 120f;
    public float pulseAmount         = 0.04f;
    public float pulseSpeed          = 0.4f;
    public float fadeToBlackDuration = 1.5f;

    private Image     _overlay;
    private Coroutine _smokeCoroutine;
    private bool      _active;

    /// <summary>Fires when smoke duration runs out (player consumed by smoke).</summary>
    public event Action OnSmokeTimeout;

    void Awake()
    {
        BuildOverlay();
    }

    void BuildOverlay()
    {
        // White 1x1 sprite so Image actually renders
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Canvas behind captions
        GameObject cGO = new GameObject("SmokeCanvas");
        Canvas canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;
        cGO.AddComponent<CanvasScaler>();
        cGO.AddComponent<GraphicRaycaster>();

        // Fullscreen overlay image
        GameObject iGO = new GameObject("SmokeOverlay");
        iGO.transform.SetParent(cGO.transform, false);
        _overlay               = iGO.AddComponent<Image>();
        _overlay.sprite        = white;
        _overlay.color         = new Color(0.05f, 0.05f, 0.05f, 0f);
        _overlay.raycastTarget = false;
        RectTransform rt = iGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void StartSmoke()
    {
        if (_active) return;
        _active = true;
        _smokeCoroutine = StartCoroutine(SmokeRoutine());
    }

    public void StopSmoke()
    {
        _active = false;
        if (_smokeCoroutine != null) StopCoroutine(_smokeCoroutine);
    }

    public IEnumerator FadeToBlack()
    {
        StopSmoke();
        if (_overlay == null) yield break;

        float start = _overlay.color.a;
        float t = 0f;
        while (t < fadeToBlackDuration)
        {
            t += Time.deltaTime;
            Color c = _overlay.color;
            c.a = Mathf.Lerp(start, 1f, t / fadeToBlackDuration);
            _overlay.color = c;
            yield return null;
        }
        Color final = _overlay.color;
        final.a = 1f;
        _overlay.color = final;
    }

    IEnumerator SmokeRoutine()
    {
        float elapsed = 0f;
        while (_active && elapsed < smokeDuration)
        {
            elapsed += Time.deltaTime;
            float baseAlpha = Mathf.Lerp(0f, maxSmokeAlpha, elapsed / smokeDuration);
            float pulse     = Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) * pulseAmount;
            Color c         = _overlay.color;
            c.a             = Mathf.Clamp01(baseAlpha + pulse);
            _overlay.color  = c;
            yield return null;
        }

        // If we exited because time ran out (not because StopSmoke was called)
        if (_active)
        {
            _active = false;
            OnSmokeTimeout?.Invoke();
        }
    }
}