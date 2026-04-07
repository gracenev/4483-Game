using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Controls the smoke overlay that gradually darkens the screen over time.
// When the timer runs out it fires an event so the level manager knows the player didn't make it.
public class SmokeEffect : MonoBehaviour
{
    [Range(0f, 1f)]
    public float maxSmokeAlpha       = 0.75f;  // how dark the screen gets at its worst
    public float smokeDuration       = 120f;   // 2 minutes to find the key and escape
    public float pulseAmount         = 0.04f;  // subtle breathing pulse on the overlay
    public float pulseSpeed          = 0.4f;
    public float fadeToBlackDuration = 1.5f;

    private Image     _overlay;
    private Coroutine _smokeCoroutine;
    private bool      _active;

    // Other scripts can listen to this to know when the smoke timer ran out
    public event Action OnSmokeTimeout;

    void Awake()
    {
        BuildOverlay();
    }

    void BuildOverlay()
    {
        // Need a plain white sprite so the Image actually shows up
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Put this canvas behind the captions so text still shows through
        GameObject cGO = new GameObject("SmokeCanvas");
        Canvas canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;
        cGO.AddComponent<CanvasScaler>();
        cGO.AddComponent<GraphicRaycaster>();

        // Dark fullscreen image that covers everything
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

    // Fades the screen fully to black - called when the level ends or the player fails
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

            // Gradually get darker over the full duration
            float baseAlpha = Mathf.Lerp(0f, maxSmokeAlpha, elapsed / smokeDuration);

            // Add a small breathing pulse on top to make it feel more alive
            float pulse     = Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) * pulseAmount;
            Color c         = _overlay.color;
            c.a             = Mathf.Clamp01(baseAlpha + pulse);
            _overlay.color  = c;
            yield return null;
        }

        // Only fire the timeout event if the timer actually ran out naturally
        if (_active)
        {
            _active = false;
            OnSmokeTimeout?.Invoke();
        }
    }
}