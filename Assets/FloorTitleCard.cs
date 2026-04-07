using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Displays a centered floor title card when the scene loads.
/// Attach to any persistent GameObject in the scene.
/// Leave titleText blank to auto-use the scene name.
/// </summary>
public class FloorTitleCard : MonoBehaviour
{
    [Tooltip("Text to display. Leave blank to use the scene name.")]
    public string titleText;

    [Tooltip("How long the title stays fully visible.")]
    public float holdDuration = 3f;

    public float fadeInDuration  = 0.5f;
    public float fadeOutDuration = 0.8f;

    private CanvasGroup _group;

    void Start()
    {
        string display = string.IsNullOrEmpty(titleText)
            ? SceneManager.GetActiveScene().name
            : titleText;

        BuildCanvas(display);
        StartCoroutine(ShowRoutine());
    }

    void BuildCanvas(string display)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Canvas
        GameObject cGO = new GameObject("FloorTitleCanvas");
        Canvas canvas  = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;   // above all other UI
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<GraphicRaycaster>();

        // Dark center band
        GameObject band = new GameObject("TitleBand");
        band.transform.SetParent(cGO.transform, false);
        Image bandImg      = band.AddComponent<Image>();
        bandImg.sprite     = white;
        bandImg.color      = new Color(0f, 0f, 0f, 0.72f);
        bandImg.raycastTarget = false;
        RectTransform brt  = band.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 0.4f);
        brt.anchorMax = new Vector2(1f, 0.6f);
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;

        _group               = band.AddComponent<CanvasGroup>();
        _group.alpha         = 0f;
        _group.blocksRaycasts = false;

        // Title text
        GameObject tGO = new GameObject("TitleText");
        tGO.transform.SetParent(band.transform, false);
        TMP_Text tmp      = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text          = display;
        tmp.fontSize      = 72;
        tmp.color         = Color.white;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.raycastTarget = false;
        RectTransform trt = tGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(80f, 0f);
        trt.offsetMax = new Vector2(-80f, 0f);
    }

    IEnumerator ShowRoutine()
    {
        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(holdDuration);
        yield return Fade(1f, 0f, fadeOutDuration);
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
