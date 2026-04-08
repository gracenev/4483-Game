using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Manages the overall flow of Floor 2.
// Handles the opening caption, the DON'T MOVE warning when cameras turn,
// the level complete fade, and the fail screen if the player gets caught.
[RequireComponent(typeof(AudioSource))]
public class Floor2LevelManager : MonoBehaviour
{
    [Header("UI")]
    public CaptionUI2 captionUI;

    [Header("Buttons")]
    public InteractableButton closeButton;

    [Header("Audio")]
    [Tooltip("The sound that plays when the cameras start turning toward the player")]
    public AudioClip cameraTurnSound;

    [Header("Fade Settings")]
    public float fadeSpeed = 1.5f;

    // These get built in code at runtime
    private Image       _fadeOverlay;
    private TMP_Text    _failText;
    private CanvasGroup _failGroup;

    private CameraSync          _sync;
    private AudioSource         _audio;
    private CameraSync.CamState _lastState;
    private bool _levelEnded = false;

    void Start()
    {
        _sync  = FindAnyObjectByType<CameraSync>();
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake  = false;
        _audio.spatialBlend = 0f;

        BuildOverlayCanvas();

        if (closeButton != null)
            closeButton.OnPressedCallback += OnLevelComplete;
        else
            Debug.LogWarning("Floor2LevelManager: Close Button isn't assigned.");

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

        // Only trigger the warning the moment the camera starts turning - not every frame
        if (current == CameraSync.CamState.TurningToPlayer &&
            _lastState != CameraSync.CamState.TurningToPlayer)
        {
            if (cameraTurnSound != null) _audio.PlayOneShot(cameraTurnSound);
            captionUI?.Show("DON'T MOVE!", 2f);
        }

        _lastState = current;
    }

    // Shows the opening caption when the level starts
    IEnumerator OpeningSequence()
    {
        yield return new WaitForSeconds(1f);
        captionUI?.Show(
            "Security cameras are sweeping the hall. " +
            "If they catch me moving, this whole floor goes into lockdown. " +
            "I need to time this perfectly.", 6f);
    }

    // Player pressed the close button - level is done
    void OnLevelComplete()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        StartCoroutine(LevelCompleteSequence());
    }

    IEnumerator LevelCompleteSequence()
    {
        yield return StartCoroutine(FadeToBlack());
        Debug.Log("Floor 2 complete - loading Floor 7.");
        SceneManager.LoadScene("Floor 7");
    }

    // Called by SurveillanceDetector when the player gets caught moving
    public void TriggerFail()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        StartCoroutine(FailSequence());
    }

    IEnumerator FailSequence()
    {
        yield return StartCoroutine(FadeToBlack());

        // Show the fail message on the black screen
        _failGroup.alpha = 1f;
        _failText.text   = "You Failed The Round...\n<size=28>Press Spacebar to Restart</size>";

        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Gradually fades the screen to black
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

    // Builds the fade overlay and fail screen in code at runtime
    void BuildOverlayCanvas()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // This canvas sits on top of everything including the caption canvas
        GameObject cGO = new GameObject("Floor2OverlayCanvas");
        Canvas canvas  = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<GraphicRaycaster>();

        // The black overlay that fades in when the level ends or the player fails
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

        // The fail panel - hidden until the player gets caught
        // Adding Image first so that RectTransform gets created automatically
        GameObject failGO = new GameObject("FailPanel");
        failGO.transform.SetParent(cGO.transform, false);
        Image failBg           = failGO.AddComponent<Image>();
        failBg.sprite          = white;
        failBg.color           = new Color(0f, 0f, 0f, 0f);
        failBg.raycastTarget   = false;
        _failGroup             = failGO.AddComponent<CanvasGroup>();
        _failGroup.alpha       = 0f;
        _failGroup.blocksRaycasts = false;
        RectTransform fp = failGO.GetComponent<RectTransform>();
        fp.anchorMin = Vector2.zero;
        fp.anchorMax = Vector2.one;
        fp.offsetMin = Vector2.zero;
        fp.offsetMax = Vector2.zero;

        // Red fail text shown on the black screen
        GameObject textGO = new GameObject("FailText");
        textGO.transform.SetParent(failGO.transform, false);
        _failText           = textGO.AddComponent<TextMeshProUGUI>();
        _failText.fontSize  = 52;
        _failText.color     = new Color(0.9f, 0.1f, 0.1f);
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