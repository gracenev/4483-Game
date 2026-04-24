using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Manages the maze floor.
// Smoke builds over 2 minutes - escape before it fills the maze.
// Step onto the outdoor platform to win.
public class MazeLevelManager : MonoBehaviour
{
    [Header("Smoke")]
    public float smokeDuration       = 120f;   // 2 minutes
    public float maxSmokeAlpha       = 0.82f;
    public float pulseAmount         = 0.04f;
    public float pulseSpeed          = 0.4f;
    public float fadeToBlackDuration = 1.5f;

    [Header("Fade")]
    public float fadeSpeed = 1.5f;

    [Header("Audio")]
    public AudioClip clockTickingClip;
    public AudioClip escapeClip;

    // Built at runtime
    private CaptionUI3  _caption;
    private Image       _smokeOverlay;
    private Image       _fadeOverlay;
    private CanvasGroup _failGroup;
    private TMP_Text    _failText;
    private CanvasGroup _winGroup;

    private AudioSource _audio;
    private bool _escaped  = false;
    private bool _winShown  = false;
    private bool _dead     = false;
    private Coroutine _smokeRoutine;

    void Update()
    {
        if (!_winShown) return;
        if (Input.GetKeyDown(KeyCode.Space))
            UnityEngine.SceneManagement.SceneManager.LoadScene("Start Menu");
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    void Start()
    {
        BuildUI();
        _caption = gameObject.AddComponent<CaptionUI3>();

        // Set up audio
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.loop        = true;
        _audio.playOnAwake = false;
        _audio.spatialBlend = 0f;
        _audio.volume      = 0.2f;
        if (clockTickingClip != null)
        {
            _audio.clip = clockTickingClip;
            _audio.Play();
        }

        StartCoroutine(OpeningSequence());
    }

    // Opening caption then smoke begins
    IEnumerator OpeningSequence()
    {
        yield return new WaitForSeconds(5.5f);
        _caption?.Show("Where am I... this floor is a complete maze.", 4f);
        yield return new WaitForSeconds(5f);
        _caption?.Show("There has to be a way out. I need to find the exit.", 3f);

        // Start smoke after a short breather
        yield return new WaitForSeconds(8f);
        StartSmoke();
    }

    // Smoke system
    void StartSmoke()
    {
        _caption?.Show("Oh no... I think the smoke is seeping down through the vents. I need to escape before I can't breathe!", 6f);
        _smokeRoutine = StartCoroutine(SmokeRoutine());
    }

    IEnumerator SmokeRoutine()
    {
        float elapsed = 0f;

        // Halfway warning
        bool halfWarning = false;

        while (elapsed < smokeDuration)
        {
            if (_escaped) yield break;

            elapsed += Time.deltaTime;
            float baseAlpha = Mathf.Lerp(0f, maxSmokeAlpha, elapsed / smokeDuration);
            float pulse     = Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) * pulseAmount;
            Color c         = _smokeOverlay.color;
            c.a             = Mathf.Clamp01(baseAlpha + pulse);
            _smokeOverlay.color = c;

            // Halfway warning caption
            if (!halfWarning && elapsed >= smokeDuration * 0.5f)
            {
                halfWarning = true;
                _caption?.Show("The smoke is getting really thick... I'm running out of time!", 4f);
            }

            yield return null;
        }

        // Time's up - player consumed
        if (!_escaped)
            StartCoroutine(SmokeDeathSequence());
    }

    IEnumerator SmokeDeathSequence()
    {
        if (_dead) yield break;
        _dead = true;

        // Stop music on death
        if (_audio != null) _audio.Stop();

        // Finish fading to full black
        yield return StartCoroutine(FadeOverlayToBlack());

        // Show fail message
        _failGroup.alpha = 1f;
        _failText.text   = "You were consumed by the smoke...\n<size=24>Press Spacebar to try again</size>";

        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Fade the smoke overlay all the way to solid black
    IEnumerator FadeOverlayToBlack()
    {
        float t = 0f;
        Color start = _smokeOverlay.color;
        while (t < fadeToBlackDuration)
        {
            t += Time.deltaTime;
            Color c = _smokeOverlay.color;
            c.a = Mathf.Lerp(start.a, 1f, t / fadeToBlackDuration);
            _smokeOverlay.color = c;
            yield return null;
        }
        _smokeOverlay.color = new Color(0f, 0f, 0f, 1f);
    }

    // Called by MazeExitTrigger when player steps onto outdoor platform
    public void OnPlayerEscaped()
    {
        if (_escaped || _dead) return;
        _escaped = true;

        if (_smokeRoutine != null) StopCoroutine(_smokeRoutine);

        StartCoroutine(EscapeSequence());
    }

    IEnumerator EscapeSequence()
    {
        // Switch to escape music
        if (_audio != null && escapeClip != null)
        {
            _audio.Stop();
            _audio.clip   = escapeClip;
            _audio.volume = 0.2f;
            _audio.Play();
        }

        _caption?.Show("Fresh air... I made it out!", 2.5f);
        yield return new WaitForSeconds(5f);

        // Fade to white - bright outdoor feel
        yield return StartCoroutine(FadeToWhite());

        // Show congratulations
        _winGroup.alpha = 1f;
        _winShown = true;
        Debug.Log("Player escaped the maze - congratulations!");

        // Wait for spacebar to restart or ESC to quit
        StartCoroutine(WinInputWait());
    }

    IEnumerator FadeToWhite()
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

    IEnumerator WinInputWait()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Start Menu");
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
                yield break;
            }
            yield return null;
        }
    }

    // Build all UI
    void BuildUI()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Canvas
        GameObject cGO = new GameObject("MazeOverlayCanvas");
        Canvas canvas  = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<GraphicRaycaster>();

        // Smoke overlay - dark grey that builds up
        GameObject smokeGO  = new GameObject("SmokeOverlay");
        smokeGO.transform.SetParent(cGO.transform, false);
        _smokeOverlay               = smokeGO.AddComponent<Image>();
        _smokeOverlay.sprite        = white;
        _smokeOverlay.color         = new Color(0.05f, 0.05f, 0.05f, 0f);
        _smokeOverlay.raycastTarget = false;
        var srt = smokeGO.GetComponent<RectTransform>();
        srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one;
        srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;

        // White fade overlay for win
        GameObject fadeGO  = new GameObject("FadeOverlay");
        fadeGO.transform.SetParent(cGO.transform, false);
        _fadeOverlay               = fadeGO.AddComponent<Image>();
        _fadeOverlay.sprite        = white;
        _fadeOverlay.color         = new Color(1f, 1f, 1f, 0f);
        _fadeOverlay.raycastTarget = false;
        var frt = fadeGO.GetComponent<RectTransform>();
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
        frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;

        // Fail panel
        GameObject failGO  = new GameObject("FailPanel");
        failGO.transform.SetParent(cGO.transform, false);
        Image failBg       = failGO.AddComponent<Image>();
        failBg.sprite      = white;
        failBg.color       = new Color(0f, 0f, 0f, 0f);
        failBg.raycastTarget = false;
        _failGroup             = failGO.AddComponent<CanvasGroup>();
        _failGroup.alpha       = 0f;
        _failGroup.blocksRaycasts = false;
        var fp = failGO.GetComponent<RectTransform>();
        fp.anchorMin = Vector2.zero; fp.anchorMax = Vector2.one;
        fp.offsetMin = Vector2.zero; fp.offsetMax = Vector2.zero;

        GameObject textGO  = new GameObject("FailText");
        textGO.transform.SetParent(failGO.transform, false);
        _failText            = textGO.AddComponent<TextMeshProUGUI>();
        _failText.fontSize   = 52;
        _failText.color      = new Color(0.9f, 0.5f, 0.1f);  // smoky orange
        _failText.alignment  = TextAlignmentOptions.Center;
        _failText.fontStyle  = FontStyles.Bold;
        _failText.text       = "";
        _failText.raycastTarget = false;
        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

        // Win panel - bright white with green congratulations text
        GameObject winGO = new GameObject("WinPanel");
        winGO.transform.SetParent(cGO.transform, false);
        Image winBg      = winGO.AddComponent<Image>();
        winBg.sprite     = white;
        winBg.color      = new Color(0.95f, 0.98f, 1f, 1f);
        winBg.raycastTarget = false;
        _winGroup            = winGO.AddComponent<CanvasGroup>();
        _winGroup.alpha      = 0f;
        _winGroup.blocksRaycasts = false;
        var wrt = winGO.GetComponent<RectTransform>();
        wrt.anchorMin = Vector2.zero; wrt.anchorMax = Vector2.one;
        wrt.offsetMin = Vector2.zero; wrt.offsetMax = Vector2.zero;

        GameObject titleGO = new GameObject("WinTitle");
        titleGO.transform.SetParent(winGO.transform, false);
        TMP_Text title     = titleGO.AddComponent<TextMeshProUGUI>();
        title.text         = "Congratulations!";
        title.fontSize     = 82;
        title.fontStyle    = FontStyles.Bold;
        title.color        = new Color(0.1f, 0.55f, 0.1f);
        title.alignment    = TextAlignmentOptions.Center;
        title.raycastTarget = false;
        var ttrt = titleGO.GetComponent<RectTransform>();
        ttrt.anchorMin = new Vector2(0f, 0.55f);
        ttrt.anchorMax = new Vector2(1f, 0.75f);
        ttrt.offsetMin = Vector2.zero; ttrt.offsetMax = Vector2.zero;

        GameObject subGO = new GameObject("WinSubtitle");
        subGO.transform.SetParent(winGO.transform, false);
        TMP_Text sub     = subGO.AddComponent<TextMeshProUGUI>();
        sub.text         = "You escaped the office!\nAgainst all odds, you made it out alive.";
        sub.fontSize     = 36;
        sub.color        = new Color(0.15f, 0.15f, 0.15f);
        sub.alignment    = TextAlignmentOptions.Center;
        sub.raycastTarget = false;
        var srt2 = subGO.GetComponent<RectTransform>();
        srt2.anchorMin = new Vector2(0.1f, 0.35f);
        srt2.anchorMax = new Vector2(0.9f, 0.55f);
        srt2.offsetMin = Vector2.zero; srt2.offsetMax = Vector2.zero;

        // Restart / quit prompt
        GameObject winPromptGO = new GameObject("WinPrompt");
        winPromptGO.transform.SetParent(winGO.transform, false);
        TMP_Text winPrompt     = winPromptGO.AddComponent<TextMeshProUGUI>();
        winPrompt.text         = "Press Spacebar to Return to Menu     |     ESC to Quit";
        winPrompt.fontSize     = 20;
        winPrompt.color        = new Color(0.35f, 0.35f, 0.35f);
        winPrompt.alignment    = TextAlignmentOptions.Center;
        winPrompt.characterSpacing = 1f;
        winPrompt.raycastTarget = false;
        var prrt = winPromptGO.GetComponent<RectTransform>();
        prrt.anchorMin = new Vector2(0.1f, 0.22f);
        prrt.anchorMax = new Vector2(0.9f, 0.30f);
        prrt.offsetMin = Vector2.zero; prrt.offsetMax = Vector2.zero;

        // Restart / quit prompt

    }
}