using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Manages the start menu and the opening crawl sequence.
// Scene order: StartMenu scene -> Floor 9 scene
public class StartMenuManager : MonoBehaviour
{
    [Header("First Scene to Load")]
    public string firstSceneName = "Floor 9";

    [Header("Fade")]
    public float fadeSpeed = 1.2f;

    // Built at runtime
    private Image       _bg;
    private CanvasGroup _menuGroup;
    private CanvasGroup _crawlGroup;
    private CanvasGroup _fadeGroup;
    private TMP_Text    _crawlText;
    private RectTransform _crawlRT;

    private bool _inputEnabled = false;
    private bool _transitioning = false;

    void Start()
    {
        BuildUI();
        StartCoroutine(MenuEntrance());
    }

    void Update()
    {
        if (_inputEnabled && !_transitioning && Input.GetKeyDown(KeyCode.Space))
        {
            _transitioning = true;
            StartCoroutine(StartSequence());
        }
    }

    // ── Menu fades in ──────────────────────────────────────────────────────
    IEnumerator MenuEntrance()
    {
        // Start fully black
        _fadeGroup.alpha = 1f;

        yield return new WaitForSeconds(0.5f);

        // Fade in from black
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            _fadeGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        _fadeGroup.alpha = 0f;

        _inputEnabled = true;
    }

    // ── Space pressed → fade out menu → show crawl ────────────────────────
    IEnumerator StartSequence()
    {
        // Fade menu out
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            _menuGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        _menuGroup.alpha = 0f;
        _menuGroup.gameObject.SetActive(false);

        // Show crawl
        _crawlGroup.alpha = 1f;
        _crawlGroup.gameObject.SetActive(true);

        yield return StartCoroutine(RunCrawl());
    }

    // ── Star Wars style upward crawl ──────────────────────────────────────
    IEnumerator RunCrawl()
    {
        // Start the text below the screen
        float startY  = -Screen.height * 0.5f;
        float endY    = Screen.height * 1.8f;
        float duration = 50f;
        float elapsed  = 0f;

        _crawlRT.anchoredPosition = new Vector2(0f, startY);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Allow skipping with space
            if (Input.GetKeyDown(KeyCode.Space))
                break;

            float progress = elapsed / duration;
            _crawlRT.anchoredPosition = new Vector2(0f, Mathf.Lerp(startY, endY, progress));
            yield return null;
        }

        // Fade to black then load first scene
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed * 0.6f;
            _fadeGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        SceneManager.LoadScene(firstSceneName);
    }

    // ── Build all UI ───────────────────────────────────────────────────────
    void BuildUI()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Main canvas
        GameObject cGO = new GameObject("StartMenuCanvas");
        Canvas canvas  = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<GraphicRaycaster>();

        // Black background
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(cGO.transform, false);
        _bg               = bgGO.AddComponent<Image>();
        _bg.sprite        = white;
        _bg.color         = Color.black;
        _bg.raycastTarget = false;
        var bgrt = bgGO.GetComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero; bgrt.anchorMax = Vector2.one;
        bgrt.offsetMin = Vector2.zero; bgrt.offsetMax = Vector2.zero;

        // ── MENU GROUP ────────────────────────────────────────────────────
        GameObject menuGO = new GameObject("MenuGroup");
        menuGO.transform.SetParent(cGO.transform, false);
        var mgrt = menuGO.AddComponent<RectTransform>();
        mgrt.anchorMin = Vector2.zero; mgrt.anchorMax = Vector2.one;
        mgrt.offsetMin = Vector2.zero; mgrt.offsetMax = Vector2.zero;
        _menuGroup               = menuGO.AddComponent<CanvasGroup>();
        _menuGroup.alpha         = 1f;
        _menuGroup.blocksRaycasts = false;

        // Facility label — small text above title
        GameObject facilityGO = new GameObject("FacilityLabel");
        facilityGO.transform.SetParent(menuGO.transform, false);
        TMP_Text facility     = facilityGO.AddComponent<TextMeshProUGUI>();
        facility.text         = "FEDERAL THREAT RESPONSE TRAINING CENTER";
        facility.fontSize     = 18;
        facility.color        = new Color(0.6f, 0.6f, 0.6f);
        facility.alignment    = TextAlignmentOptions.Center;
        facility.fontStyle    = FontStyles.Normal;
        facility.characterSpacing = 4f;
        facility.raycastTarget = false;
        var flrt = facilityGO.GetComponent<RectTransform>();
        flrt.anchorMin = new Vector2(0f, 0.62f);
        flrt.anchorMax = new Vector2(1f, 0.68f);
        flrt.offsetMin = Vector2.zero; flrt.offsetMax = Vector2.zero;

        // Main title — LAST DESCENT
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(menuGO.transform, false);
        TMP_Text title     = titleGO.AddComponent<TextMeshProUGUI>();
        title.text         = "LAST DESCENT";
        title.fontSize     = 110;
        title.fontStyle    = FontStyles.Bold;
        title.color        = Color.white;
        title.alignment    = TextAlignmentOptions.Center;
        title.characterSpacing = 8f;
        title.raycastTarget = false;
        var trt = titleGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 0.42f);
        trt.anchorMax = new Vector2(1f, 0.64f);
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

        // Thin divider line under title
        GameObject lineGO = new GameObject("Divider");
        lineGO.transform.SetParent(menuGO.transform, false);
        Image line       = lineGO.AddComponent<Image>();
        line.sprite      = white;
        line.color       = new Color(0.4f, 0.4f, 0.4f);
        line.raycastTarget = false;
        var lrt = lineGO.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.3f, 0.415f);
        lrt.anchorMax = new Vector2(0.7f, 0.418f);
        lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

        // Prompt — blinking spacebar text
        GameObject promptGO = new GameObject("Prompt");
        promptGO.transform.SetParent(menuGO.transform, false);
        TMP_Text prompt     = promptGO.AddComponent<TextMeshProUGUI>();
        prompt.text         = "PRESS SPACEBAR TO BEGIN YOUR DESCENT";
        prompt.fontSize     = 22;
        prompt.color        = new Color(0.75f, 0.75f, 0.75f);
        prompt.alignment    = TextAlignmentOptions.Center;
        prompt.characterSpacing = 3f;
        prompt.raycastTarget = false;
        var prt = promptGO.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0f, 0.33f);
        prt.anchorMax = new Vector2(1f, 0.40f);
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

        // Add blink component to prompt
        promptGO.AddComponent<BlinkText>().text = prompt;

        // Version / build info bottom right
        GameObject buildGO = new GameObject("BuildInfo");
        buildGO.transform.SetParent(menuGO.transform, false);
        TMP_Text build     = buildGO.AddComponent<TextMeshProUGUI>();
        build.text         = "EVALUATION BUILD";
        build.fontSize     = 14;
        build.color        = new Color(0.3f, 0.3f, 0.3f);
        build.alignment    = TextAlignmentOptions.Right;
        build.raycastTarget = false;
        var brt = buildGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.7f, 0.02f);
        brt.anchorMax = new Vector2(0.98f, 0.07f);
        brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;

        // ── CRAWL GROUP ───────────────────────────────────────────────────
        GameObject crawlGO = new GameObject("CrawlGroup");
        crawlGO.transform.SetParent(cGO.transform, false);
        var cgrt = crawlGO.AddComponent<RectTransform>();
        cgrt.anchorMin = Vector2.zero; cgrt.anchorMax = Vector2.one;
        cgrt.offsetMin = Vector2.zero; cgrt.offsetMax = Vector2.zero;
        _crawlGroup               = crawlGO.AddComponent<CanvasGroup>();
        _crawlGroup.alpha         = 0f;
        _crawlGroup.blocksRaycasts = false;
        crawlGO.SetActive(false);

        // Gradient top and bottom to fade crawl text in/out
        GameObject topFadeGO = new GameObject("TopFade");
        topFadeGO.transform.SetParent(crawlGO.transform, false);
        Image topFade     = topFadeGO.AddComponent<Image>();
        topFade.sprite    = white;
        topFade.color     = new Color(0f, 0f, 0f, 0.95f);
        topFade.raycastTarget = false;
        var tfrt = topFadeGO.GetComponent<RectTransform>();
        tfrt.anchorMin = new Vector2(0f, 0.72f);
        tfrt.anchorMax = new Vector2(1f, 1f);
        tfrt.offsetMin = Vector2.zero; tfrt.offsetMax = Vector2.zero;

        GameObject botFadeGO = new GameObject("BottomFade");
        botFadeGO.transform.SetParent(crawlGO.transform, false);
        Image botFade     = botFadeGO.AddComponent<Image>();
        botFade.sprite    = white;
        botFade.color     = new Color(0f, 0f, 0f, 0.95f);
        botFade.raycastTarget = false;
        var bfrt = botFadeGO.GetComponent<RectTransform>();
        bfrt.anchorMin = new Vector2(0f, 0f);
        bfrt.anchorMax = new Vector2(1f, 0.18f);
        bfrt.offsetMin = Vector2.zero; bfrt.offsetMax = Vector2.zero;

        // Crawl text container — scrolls upward
        GameObject crawlTextGO = new GameObject("CrawlText");
        crawlTextGO.transform.SetParent(crawlGO.transform, false);
        _crawlText               = crawlTextGO.AddComponent<TextMeshProUGUI>();
        _crawlText.text          = GetCrawlText();
        _crawlText.fontSize      = 28;
        _crawlText.color         = new Color(0.95f, 0.85f, 0.45f);  // classic crawl amber/gold
        _crawlText.alignment     = TextAlignmentOptions.Center;
        _crawlText.lineSpacing   = 12f;
        _crawlText.raycastTarget = false;
        _crawlText.textWrappingMode = TextWrappingModes.Normal;
        _crawlRT = crawlTextGO.GetComponent<RectTransform>();
        _crawlRT.anchorMin = new Vector2(0.25f, 0f);
        _crawlRT.anchorMax = new Vector2(0.75f, 0f);
        _crawlRT.pivot     = new Vector2(0.5f, 0f);
        _crawlRT.sizeDelta = new Vector2(0f, 900f);

        // Skip hint bottom of screen during crawl
        GameObject skipGO = new GameObject("SkipHint");
        skipGO.transform.SetParent(crawlGO.transform, false);
        TMP_Text skip     = skipGO.AddComponent<TextMeshProUGUI>();
        skip.text         = "SPACEBAR TO SKIP";
        skip.fontSize     = 16;
        skip.color        = new Color(0.35f, 0.35f, 0.35f);
        skip.alignment    = TextAlignmentOptions.Center;
        skip.raycastTarget = false;
        var skrt = skipGO.GetComponent<RectTransform>();
        skrt.anchorMin = new Vector2(0f, 0.02f);
        skrt.anchorMax = new Vector2(1f, 0.07f);
        skrt.offsetMin = Vector2.zero; skrt.offsetMax = Vector2.zero;

        // ── FADE OVERLAY ──────────────────────────────────────────────────
        GameObject fadeGO  = new GameObject("FadeOverlay");
        fadeGO.transform.SetParent(cGO.transform, false);
        Image fadeBg       = fadeGO.AddComponent<Image>();
        fadeBg.sprite      = white;
        fadeBg.color       = Color.black;
        fadeBg.raycastTarget = false;
        var fdrt = fadeBg.GetComponent<RectTransform>();
        fdrt.anchorMin = Vector2.zero; fdrt.anchorMax = Vector2.one;
        fdrt.offsetMin = Vector2.zero; fdrt.offsetMax = Vector2.zero;
        _fadeGroup             = fadeGO.AddComponent<CanvasGroup>();
        _fadeGroup.alpha       = 1f;
        _fadeGroup.blocksRaycasts = false;
    }

    string GetCrawlText()
    {
        return
            "CLASSIFIED\n" +
            "FEDERAL THREAT RESPONSE TRAINING CENTER\n" +
            "INCIDENT FILE 7-DELTA\n\n\n" +

            "The Federal Threat Response Training Center was built to prepare the nation's most elite operatives " +
            "for the worst imaginable scenarios.\n\n" +

            "You helped design it.\n\n" +

            "Every floor. Every trial. Every carefully engineered obstacle was created by researchers like you " +
            "to push recruits to their absolute limit — safely, under controlled conditions.\n\n" +

            "Tonight, the facility is empty.\n\n" +

            "You stayed late on a weekend shift, alone on the top floor, when an electrical fire ignited in the " +
            "upper mechanical levels. Within minutes, smoke began flooding the building from above.\n\n" +

            "The automated systems, designed to lock down the facility during active evaluations, have " +
            "misidentified the emergency as a live training drill.\n\n" +

            "Every exit is sealed.\n\n" +

            "The only way down is the Evaluation Elevator — the same system you helped build " +
            "to test recruits floor by floor under escalating pressure.\n\n" +

            "It will not descend until each floor's challenge is completed.\n\n" +

            "The smoke does not know the difference between a trainee and its creator.\n\n" +

            "Neither does the building.\n\n\n" +

            "SURVIVE YOUR OWN CREATION.";
    }
}

// Simple blink component for the press spacebar prompt
public class BlinkText : MonoBehaviour
{
    public TMP_Text text;
    private float _timer;
    private float _interval = 0.85f;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _interval)
        {
            _timer = 0f;
            if (text != null)
                text.enabled = !text.enabled;
        }
    }
}