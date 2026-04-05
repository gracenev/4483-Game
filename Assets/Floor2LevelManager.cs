using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Floor 2 level manager.
/// - Opening caption on start
/// - DON'T MOVE caption + sound when camera turns
/// - Button_Close → fade to black → level complete
/// - TriggerFail() → fade to black → "You Failed" screen → Space to restart
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Floor2LevelManager : MonoBehaviour
{
    [Header("UI")]
    public CaptionUI2 captionUI;

    [Header("Buttons")]
    public InteractableButton closeButton;

    [Header("Audio")]
    public AudioClip cameraTurnSound;

    [Header("Fade Settings")]
    public float fadeSpeed = 1.5f;

    // ── Built at runtime ───────────────────────────────────────────────────
    private Image      _fadeOverlay;
    private TMP_Text   _failText;
    private CanvasGroup _failGroup;

    // ── Runtime state ──────────────────────────────────────────────────────
    private CameraSync          _sync;
    private AudioSource         _audio;
    private CameraSync.CamState _lastState;
    private bool _levelEnded = false;

    // ── Unity ──────────────────────────────────────────────────────────────
    void Start()
    {
        _sync  = FindObjectOfType<CameraSync>();
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake  = false;
        _audio.spatialBlend = 0f;

        BuildOverlayCanvas();

        if (closeButton != null)
            closeButton.OnPressedCallback += OnLevelComplete;
        else
            Debug.LogWarning("[Floor2LevelManager] Close Button not assigned!");

        StartCoroutine(OpeningSequence());
    }

    void OnDestroy()
    {
        if (closeButton != null)
            closeButton.OnPressedCallback -= OnLevelComplete;
    }

    void Update()
    {
        if (_sync == null || _levelEnded) return;

        CameraSync.CamState current = _sync.State;

        if (current == CameraSync.CamState.TurningToPlayer &&
            _lastState != CameraSync.CamState.TurningToPlayer)
        {
            if (cameraTurnSound != null) _audio.PlayOneShot(cameraTurnSound);
            captionUI?.Show("DON'T MOVE!", 2f);
        }

        _lastState = current;
    }

    // ── Opening caption ────────────────────────────────────────────────────
    IEnumerator OpeningSequence()
    {
        yield return new WaitForSeconds(1f);
        captionUI?.Show(
            "Security cameras are sweeping the hall. " +
            "If they catch me moving, this whole floor goes into lockdown. " +
            "I need to time this perfectly.", 6f);
    }

    // ── Level complete (Button_Close pressed) ──────────────────────────────
    void OnLevelComplete()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        StartCoroutine(LevelCompleteSequence());
    }

    IEnumerator LevelCompleteSequence()
    {
        yield return StartCoroutine(FadeToBlack());
        Debug.Log("[Floor2LevelManager] Level complete — ready for next scene.");
        // TODO: SceneManager.LoadScene("Floor_1");
    }

    // ── Fail (called by SurveillanceDetector) ─────────────────────────────
    public void TriggerFail()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        StartCoroutine(FailSequence());
    }

    IEnumerator FailSequence()
    {
        yield return StartCoroutine(FadeToBlack());

        // Show fail message
        _failGroup.alpha = 1f;
        _failText.text   = "You Failed The Round...\n<size=28>Press Spacebar to Restart</size>";

        // Wait for spacebar
        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        // Restart
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ── Fade to black ──────────────────────────────────────────────────────
    IEnumerator FadeToBlack()
    {
        Color c = _fadeOverlay.color;
        while (c.a < 1f)
        {
            c.a += fadeSpeed * Time.deltaTime;
            c.a  = Mathf.Clamp01(c.a);
            _fadeOverlay.color = c;
            yield return null;
        }
    }

    // ── Build overlay canvas (fade + fail screen) ──────────────────────────
    void BuildOverlayCanvas()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        GameObject cGO = new GameObject("Floor2OverlayCanvas");
        Canvas canvas  = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;   // on top of everything
        CanvasScaler scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<GraphicRaycaster>();

        // Black fullscreen fade overlay
        GameObject fadeGO = new GameObject("FadeOverlay");
        fadeGO.transform.SetParent(cGO.transform, false);
        _fadeOverlay               = fadeGO.AddComponent<Image>();
        _fadeOverlay.sprite        = white;
        _fadeOverlay.color         = new Color(0f, 0f, 0f, 0f);
        _fadeOverlay.raycastTarget = false;
        RectTransform fr = fadeGO.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero;
        fr.anchorMax = Vector2.one;
        fr.offsetMin = Vector2.zero;
        fr.offsetMax = Vector2.zero;

        // Fail message panel (hidden until fail)
        // Add Image first so RectTransform is created automatically
        GameObject failGO = new GameObject("FailPanel");
        failGO.transform.SetParent(cGO.transform, false);
        Image failBg           = failGO.AddComponent<Image>();
        failBg.sprite          = white;
        failBg.color           = new Color(0f, 0f, 0f, 0f);  // invisible bg
        failBg.raycastTarget   = false;
        _failGroup             = failGO.AddComponent<CanvasGroup>();
        _failGroup.alpha       = 0f;
        _failGroup.blocksRaycasts = false;
        RectTransform fp = failGO.GetComponent<RectTransform>();
        fp.anchorMin = Vector2.zero;
        fp.anchorMax = Vector2.one;
        fp.offsetMin = Vector2.zero;
        fp.offsetMax = Vector2.zero;

        // Fail text
        GameObject textGO = new GameObject("FailText");
        textGO.transform.SetParent(failGO.transform, false);
        _failText           = textGO.AddComponent<TextMeshProUGUI>();
        _failText.fontSize  = 52;
        _failText.color     = new Color(0.9f, 0.1f, 0.1f);   // red
        _failText.alignment = TextAlignmentOptions.Center;
        _failText.fontStyle = FontStyles.Bold;
        _failText.text      = "";
        RectTransform tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;
    }
}