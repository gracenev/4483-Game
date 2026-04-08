using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shows the floor number on a black screen briefly when a scene loads, then fades out.
// Attaches to any GameObject in the scene and sets the Floor Name in the Inspector.
public class FloorTitle : MonoBehaviour
{
    [Tooltip("The text that shows up e.g. 'Floor 9' or 'Floor 7'")]
    public string floorName = "Floor 9";

    public float holdDuration = 1.5f;   // how long it stays on screen
    public float fadeDuration = 0.8f;   // how long the fade out takes

    private Image       _bg;
    private TMP_Text    _text;
    private CanvasGroup _group;

    void Awake()
    {
        BuildUI();
        StartCoroutine(ShowTitle());
    }

    void BuildUI()
    {
        // White sprite so the Image renders properly
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Canvas that sits on top of everything else in the scene
        GameObject cGO  = new GameObject("FloorTitleCanvas");
        Canvas canvas   = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<GraphicRaycaster>();

        // Black fullscreen background
        GameObject bgGO = new GameObject("TitleBG");
        bgGO.transform.SetParent(cGO.transform, false);
        _bg               = bgGO.AddComponent<Image>();
        _bg.sprite        = white;
        _bg.color         = new Color(0f, 0f, 0f, 1f);
        _bg.raycastTarget = false;
        var bgrt = bgGO.GetComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
        bgrt.offsetMin = Vector2.zero; bgrt.offsetMax = Vector2.zero;

        // CanvasGroup so the whole thing can fade at once
        _group                = bgGO.AddComponent<CanvasGroup>();
        _group.alpha          = 1f;
        _group.blocksRaycasts = false;

        // The floor name text centred on the black screen
        GameObject tGO = new GameObject("TitleText");
        tGO.transform.SetParent(bgGO.transform, false);
        _text               = tGO.AddComponent<TextMeshProUGUI>();
        _text.text          = floorName;
        _text.fontSize      = 72;
        _text.color         = Color.white;
        _text.alignment     = TextAlignmentOptions.Center;
        _text.fontStyle     = FontStyles.Bold;
        _text.raycastTarget = false;
        var trt = tGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
    }

    IEnumerator ShowTitle()
    {
        // Hold for a moment so the player can read it
        yield return new WaitForSeconds(holdDuration);

        // Fade out and reveal the level
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _group.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        _group.alpha = 0f;
    }
}