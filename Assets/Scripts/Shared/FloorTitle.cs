using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shows the floor number and name on a black screen briefly when a scene loads, then fades out.
// Also shows a short subtitle describing what the player needs to do on this floor.
public class FloorTitle : MonoBehaviour
{
    [Tooltip("e.g. 'Floor 6' - the display number shown to the player")]
    public string displayFloorNumber = "Floor 6";

    [Tooltip("e.g. 'Smoke & Evacuation'")]
    public string floorName = "Smoke & Evacuation";

    [Tooltip("One short sentence describing the objective")]
    [TextArea(2, 3)]
    public string floorDescription = "Find the elevator key and escape before the smoke overwhelms you.";

    public float holdDuration = 7f;
    public float fadeDuration = 0.8f;

    private CanvasGroup _group;

    void Awake()
    {
        BuildUI();
        StartCoroutine(ShowTitle());
    }

    void BuildUI()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Canvas on top of everything
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
        Image bg          = bgGO.AddComponent<Image>();
        bg.sprite         = white;
        bg.color          = Color.black;
        bg.raycastTarget  = false;
        var bgrt = bgGO.GetComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
        bgrt.offsetMin = Vector2.zero; bgrt.offsetMax = Vector2.zero;

        // CanvasGroup to fade whole thing at once
        _group                = bgGO.AddComponent<CanvasGroup>();
        _group.alpha          = 1f;
        _group.blocksRaycasts = false;

        // Floor number - small label above the name
        GameObject numGO = new GameObject("FloorNumber");
        numGO.transform.SetParent(bgGO.transform, false);
        TMP_Text num     = numGO.AddComponent<TextMeshProUGUI>();
        num.text         = displayFloorNumber.ToUpper();
        num.fontSize     = 22;
        num.color        = new Color(0.55f, 0.55f, 0.55f);
        num.alignment    = TextAlignmentOptions.Center;
        num.characterSpacing = 6f;
        num.raycastTarget = false;
        var nrt = numGO.GetComponent<RectTransform>();
        nrt.anchorMin = new Vector2(0f, 0.58f);
        nrt.anchorMax = new Vector2(1f, 0.64f);
        nrt.offsetMin = Vector2.zero; nrt.offsetMax = Vector2.zero;

        // Floor name - large bold centred
        GameObject nameGO = new GameObject("FloorName");
        nameGO.transform.SetParent(bgGO.transform, false);
        TMP_Text name     = nameGO.AddComponent<TextMeshProUGUI>();
        name.text         = floorName.ToUpper();
        name.fontSize     = 64;
        name.fontStyle    = FontStyles.Bold;
        name.color        = Color.white;
        name.alignment    = TextAlignmentOptions.Center;
        name.characterSpacing = 4f;
        name.raycastTarget = false;
        var namert = nameGO.GetComponent<RectTransform>();
        namert.anchorMin = new Vector2(0f, 0.44f);
        namert.anchorMax = new Vector2(1f, 0.60f);
        namert.offsetMin = Vector2.zero; namert.offsetMax = Vector2.zero;

        // Thin divider line
        GameObject lineGO = new GameObject("Divider");
        lineGO.transform.SetParent(bgGO.transform, false);
        Image line        = lineGO.AddComponent<Image>();
        line.sprite       = white;
        line.color        = new Color(0.35f, 0.35f, 0.35f);
        line.raycastTarget = false;
        var lrt = lineGO.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.3f, 0.435f);
        lrt.anchorMax = new Vector2(0.7f, 0.438f);
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

        // Description - subtle grey text below name
        GameObject descGO = new GameObject("FloorDesc");
        descGO.transform.SetParent(bgGO.transform, false);
        TMP_Text desc     = descGO.AddComponent<TextMeshProUGUI>();
        desc.text         = floorDescription;
        desc.fontSize     = 22;
        desc.color        = new Color(0.6f, 0.6f, 0.6f);
        desc.alignment    = TextAlignmentOptions.Center;
        desc.raycastTarget = false;
        var drt = descGO.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0.15f, 0.33f);
        drt.anchorMax = new Vector2(0.85f, 0.43f);
        drt.offsetMin = Vector2.zero; drt.offsetMax = Vector2.zero;
    }

    IEnumerator ShowTitle()
    {
        yield return new WaitForSeconds(holdDuration);

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