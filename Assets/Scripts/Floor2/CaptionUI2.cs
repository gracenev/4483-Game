using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Same as CaptionUI but for Floor 2 - kept separate so each floor
// has its own canvas and they don't interfere with each other.
public class CaptionUI2 : MonoBehaviour
{
    public float fadeInDuration  = 0.3f;
    public float fadeOutDuration = 0.6f;

    private TMP_Text    _text;
    private CanvasGroup _group;
    private Coroutine   _current;

    void Awake()
    {
        BuildCanvas();
    }

    void BuildCanvas()
    {
        // Need a plain white sprite so the Image component actually renders
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Canvas that always renders on top of the game world
        GameObject cGO = new GameObject("CaptionCanvas2");
        Canvas canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<GraphicRaycaster>();

        // Semi-transparent dark panel at the bottom of the screen
        GameObject panelGO = new GameObject("CaptionPanel2");
        panelGO.transform.SetParent(cGO.transform, false);
        Image panelImg = panelGO.AddComponent<Image>();
        panelImg.sprite        = white;
        panelImg.color         = new Color(0f, 0f, 0f, 0.65f);
        panelImg.raycastTarget = false;
        RectTransform pr = panelGO.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0f, 0f);
        pr.anchorMax = new Vector2(1f, 0.18f);
        pr.offsetMin = Vector2.zero;
        pr.offsetMax = Vector2.zero;

        // CanvasGroup lets me fade the whole panel in and out with one alpha value
        _group = panelGO.AddComponent<CanvasGroup>();
        _group.alpha          = 0f;
        _group.blocksRaycasts = false;

        // The text that actually displays the caption
        GameObject tGO = new GameObject("CaptionText2");
        tGO.transform.SetParent(panelGO.transform, false);
        _text               = tGO.AddComponent<TextMeshProUGUI>();
        _text.fontSize      = 32;
        _text.color         = Color.white;
        _text.alignment     = TextAlignmentOptions.Center;
        _text.fontStyle     = FontStyles.Bold;
        _text.text          = "";
        _text.raycastTarget = false;
        RectTransform tr = tGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(60f, 12f);
        tr.offsetMax = new Vector2(-60f, -12f);
    }

    public void Show(string text, float duration)
    {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(ShowRoutine(text, duration));
    }

    public void Hide()
    {
        if (_current != null) StopCoroutine(_current);
        if (_group != null) _group.alpha = 0f;
        if (_text  != null) _text.text   = "";
    }

    IEnumerator ShowRoutine(string text, float duration)
    {
        _text.text = text;
        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(duration);
        yield return Fade(1f, 0f, fadeOutDuration);
        _text.text = "";
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            _group.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        _group.alpha = to;
    }
}