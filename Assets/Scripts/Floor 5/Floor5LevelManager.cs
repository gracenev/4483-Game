using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Manages the full flow of Floor 5 — Power Routing Failure.
// Player enters → tries elevator → discovers power is out → finds breakers
// → solves puzzle → key spawns → picks up key → takes elevator out.
public class Floor5LevelManager : MonoBehaviour
{
    [Header("Breakers — assign in order B1 to B5")]
    public BreakerSwitch[] breakers;

    [Header("Buttons")]
    [Tooltip("The elevator call button — locked, requires key")]
    public InteractableButton callButton;
    [Tooltip("The close button inside the elevator")]
    public InteractableButton closeButton;

    [Header("Key")]
    [Tooltip("Drag your key prefab/object here — will spawn at player position when solved")]
    public GameObject keyObject;

    [Header("Environmental Feedback")]
    public Renderer[]      hallwayMonitors;
    public ParticleSystem[] elevatorSparks;
    public Renderer         elevatorButtonRenderer;

    [Header("Fade")]
    public float fadeSpeed = 1.5f;

    // ── Runtime refs ───────────────────────────────────────────────────────
    private CaptionUI3  _caption;
    private Image       _fadeOverlay;
    private TMP_Text[]  _breakerLabels  = new TMP_Text[5];
    private Image[]     _breakerDots    = new Image[5];
    private GameObject  _hudPanel;
    private bool        _hudVisible     = false;
    private bool        _elevatorTried  = false;
    private bool        _solved         = false;
    private bool        _levelEnded     = false;
    private Transform   _playerTransform;

    // ── Breaker colors for HUD dots ────────────────────────────────────────
    private static readonly Color[] BreakerColors =
    {
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow,
        Color.white
    };

    private static readonly string[] BreakerNames =
    { "B1", "B2", "B3", "B4", "B5" };

    // ── Solution: all 5 breakers ON ────────────────────────────────────────
    private bool[] Solution = { true, true, true, true, true };

    // ── Link chain: B1->B2, B2->B3, B3->B5, B4->B5, B5=none ───────────────
    // B3 turning on knocks B5 off — player must then flip B5 to finish
    private static readonly int[] LinkedTo = { 1, 2, 4, 4, -1 };

    // ── Breaker flip captions ──────────────────────────────────────────────
    private static readonly string[] FlipCaptions =
    {
        "B1 flipped — something nearby shifted too...",
        "B2 flipped — I can hear something changing.",
        "B3 online — that one seemed straightforward.",
        "B4 flipped — another one's reacting to this.",
        "B5 online — that one didn't affect anything else."
    };

    void Start()
    {
        BuildUI();
        _caption = gameObject.AddComponent<CaptionUI3>();

        // Find player
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _playerTransform = p.transform;

        // Hook call button — denied fires when player clicks without key
        if (callButton != null)
        {
            callButton.requiresKey        = true;
            callButton.OnDeniedCallback   += OnElevatorDenied;
        }
        else
            Debug.LogWarning("Floor5LevelManager: Call Button not assigned.");

        // Hook close button — only works after puzzle solved + key picked up
        if (closeButton != null)
            closeButton.OnPressedCallback += OnLevelComplete;

        // Hide key until puzzle is solved
        if (keyObject != null) keyObject.SetActive(false);

        RefreshHUD();
        RefreshAllEnvironment();
        StartCoroutine(OpeningSequence());
    }

    void OnDestroy()
    {
        if (callButton  != null) callButton.OnDeniedCallback  -= OnElevatorDenied;
        if (closeButton != null) closeButton.OnPressedCallback -= OnLevelComplete;
    }

    // ── Opening caption ────────────────────────────────────────────────────
    IEnumerator OpeningSequence()
    {
        yield return new WaitForSeconds(1f);
        _caption?.Show("Finally... a normal looking floor. Maybe I can actually just take the elevator.", 5f);
    }

    // ── Player tries the locked elevator ──────────────────────────────────
    void OnElevatorDenied()
    {
        if (_elevatorTried) return;
        _elevatorTried = true;
        StartCoroutine(ElevatorDeniedSequence());
    }

    IEnumerator ElevatorDeniedSequence()
    {
        _caption?.Show("The elevator's not responding... the power must be out on this floor.", 4f);
        yield return new WaitForSeconds(4.5f);
        _caption?.Show("There have to be breaker boxes somewhere around here. I need to restore the power.", 4f);

        // Show the breaker HUD now that the player knows power is out
        ShowHUD(true);
        RefreshHUD();
    }

    // ── Called by BreakerSwitch when flipped ──────────────────────────────
    public void OnBreakerFlipped(int breakerID)
    {
        if (breakerID < 0 || breakerID >= breakers.Length) return;

        // Show caption for this specific breaker
        if (breakerID < FlipCaptions.Length)
            _caption?.Show(FlipCaptions[breakerID], 3f);

        // Mild link — toggle neighbour silently
        int linked = LinkedTo[breakerID];
        if (linked >= 0 && linked < breakers.Length && breakers[linked] != null)
            breakers[linked].ForceState(!breakers[linked].isOn);

        RefreshHUD();
        RefreshAllEnvironment();
        CheckSolution();
    }

    // ── Check if all breakers match solution ──────────────────────────────
    void CheckSolution()
    {
        if (_solved) return;
        for (int i = 0; i < Solution.Length; i++)
        {
            if (breakers == null || i >= breakers.Length || breakers[i] == null) return;
            if (breakers[i].isOn != Solution[i]) return;
        }

        _solved = true;
        RefreshHUD();
        RefreshAllEnvironment();
        StartCoroutine(SolvedSequence());
    }

    IEnumerator SolvedSequence()
    {
        _caption?.Show("Power restored! Now I just need to find a way into the elevator...", 4f);

        // Flash lights
        if (breakers != null)
            foreach (var b in breakers)
                if (b != null)
                    foreach (var lt in b.linkedLights)
                        if (lt != null) StartCoroutine(FlashLight(lt));

        yield return new WaitForSeconds(2f);

        // Spawn key at player's current position
        if (keyObject != null && _playerTransform != null)
        {
            Vector3 spawnPos = _playerTransform.position +
                               _playerTransform.forward * 1.2f +
                               Vector3.up * 0.5f;
            keyObject.transform.position = spawnPos;
            keyObject.SetActive(true);

        }

        Debug.Log("Floor 5 solved — key spawned. Pick it up and press Button_Close.");
    }

    IEnumerator FlashLight(Light lt)
    {
        for (int i = 0; i < 4; i++)
        {
            lt.enabled = !lt.enabled;
            yield return new WaitForSeconds(0.12f);
        }
        lt.enabled = true;
    }

    // ── Button_Close pressed ───────────────────────────────────────────────
    void OnLevelComplete()
    {
        if (!_solved)
        {
            _caption?.Show("The elevator still has no power... I need to find those breakers.", 3f);
            return;
        }
        if (_levelEnded) return;
        _levelEnded = true;
        StartCoroutine(LevelCompleteSequence());
    }

    IEnumerator LevelCompleteSequence()
    {
        yield return StartCoroutine(FadeToBlack());
        Debug.Log("Floor 5 complete.");
        // TODO: SceneManager.LoadScene("Floor 6");
    }

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

    // ── Environmental feedback ─────────────────────────────────────────────
    void RefreshAllEnvironment()
    {
        if (breakers == null) return;

        // B2 stops sparks
        if (breakers.Length > 1 && breakers[1] != null && elevatorSparks != null)
            foreach (var ps in elevatorSparks)
                if (ps != null) { if (breakers[1].isOn) ps.Stop(); else ps.Play(); }

        // B3 turns on monitors
        if (breakers.Length > 2 && breakers[2] != null && hallwayMonitors != null)
            foreach (var mon in hallwayMonitors)
                if (mon != null)
                {
                    mon.enabled = breakers[2].isOn;
                    if (breakers[2].isOn)
                    {
                        mon.material.EnableKeyword("_EMISSION");
                        mon.material.SetColor("_EmissionColor", new Color(0f, 0.8f, 0.2f) * 2f);
                    }
                }

        // Elevator button goes green when solved
        if (elevatorButtonRenderer != null)
            elevatorButtonRenderer.material.color = _solved
                ? new Color(0.1f, 0.9f, 0.1f)
                : new Color(0.9f, 0.1f, 0.1f);
    }

    // ── Breaker HUD ────────────────────────────────────────────────────────
    void ShowHUD(bool show)
    {
        _hudVisible = show;
        if (_hudPanel != null) _hudPanel.SetActive(show);
    }

    void RefreshHUD()
    {
        if (!_hudVisible) return;
        for (int i = 0; i < 5; i++)
        {
            if (_breakerLabels[i] == null) continue;
            bool on = breakers != null && i < breakers.Length && breakers[i] != null && breakers[i].isOn;
            _breakerLabels[i].text  = $"{BreakerNames[i]} — {(on ? "ON" : "OFF")}";
            _breakerLabels[i].color = on ? Color.white : new Color(0.45f, 0.45f, 0.45f);
            // Dot always shows the breaker's colour — brightness drops when off
            if (_breakerDots[i] != null)
                _breakerDots[i].color = on
                    ? BreakerColors[i]
                    : new Color(BreakerColors[i].r * 0.35f,
                                BreakerColors[i].g * 0.35f,
                                BreakerColors[i].b * 0.35f, 1f);
        }
    }

    // ── Build all UI ───────────────────────────────────────────────────────
    void BuildUI()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Main overlay canvas
        GameObject cGO = new GameObject("Floor5OverlayCanvas");
        Canvas canvas  = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<GraphicRaycaster>();

        // Fade overlay
        GameObject fadeGO  = new GameObject("FadeOverlay");
        fadeGO.transform.SetParent(cGO.transform, false);
        _fadeOverlay               = fadeGO.AddComponent<Image>();
        _fadeOverlay.sprite        = white;
        _fadeOverlay.color         = new Color(0f, 0f, 0f, 0f);
        _fadeOverlay.raycastTarget = false;
        var frt = fadeGO.GetComponent<RectTransform>();
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
        frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;

        // ── Breaker HUD panel — top right ─────────────────────────────────
        _hudPanel = new GameObject("BreakerHUD");
        _hudPanel.transform.SetParent(cGO.transform, false);

        // Panel background
        Image hudBg     = _hudPanel.AddComponent<Image>();
        hudBg.sprite    = white;
        hudBg.color     = new Color(0f, 0f, 0f, 0.75f);
        hudBg.raycastTarget = false;

        RectTransform hudRt = _hudPanel.GetComponent<RectTransform>();
        hudRt.anchorMin = new Vector2(1f, 1f);
        hudRt.anchorMax = new Vector2(1f, 1f);
        hudRt.pivot     = new Vector2(1f, 1f);
        hudRt.anchoredPosition = new Vector2(-20f, -20f);
        hudRt.sizeDelta        = new Vector2(220f, 200f);

        // Title
        GameObject titleGO = new GameObject("HUD_Title");
        titleGO.transform.SetParent(_hudPanel.transform, false);
        TMP_Text title     = titleGO.AddComponent<TextMeshProUGUI>();
        title.text         = "BREAKER STATUS";
        title.fontSize     = 16;
        title.fontStyle    = FontStyles.Bold;
        title.color        = new Color(1f, 0.8f, 0.2f);
        title.alignment    = TextAlignmentOptions.Center;
        title.raycastTarget = false;
        var titleRt = titleGO.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot     = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -8f);
        titleRt.sizeDelta        = new Vector2(0f, 24f);

        // One row per breaker
        for (int i = 0; i < 5; i++)
        {
            float rowY = -40f - i * 32f;

            // Coloured dot
            GameObject dotGO = new GameObject($"Dot_{i}");
            dotGO.transform.SetParent(_hudPanel.transform, false);
            _breakerDots[i]               = dotGO.AddComponent<Image>();
            _breakerDots[i].sprite        = white;
            _breakerDots[i].color         = new Color(0.2f, 0.2f, 0.2f);
            _breakerDots[i].raycastTarget = false;
            var dotRt = dotGO.GetComponent<RectTransform>();
            dotRt.anchorMin = new Vector2(0f, 1f);
            dotRt.anchorMax = new Vector2(0f, 1f);
            dotRt.pivot     = new Vector2(0f, 1f);
            dotRt.anchoredPosition = new Vector2(12f, rowY);
            dotRt.sizeDelta        = new Vector2(18f, 18f);

            // Label text
            GameObject labelGO = new GameObject($"Label_{i}");
            labelGO.transform.SetParent(_hudPanel.transform, false);
            _breakerLabels[i]               = labelGO.AddComponent<TextMeshProUGUI>();
            _breakerLabels[i].text          = $"{BreakerNames[i]} — OFF";
            _breakerLabels[i].fontSize      = 15;
            _breakerLabels[i].color         = new Color(0.6f, 0.6f, 0.6f);
            _breakerLabels[i].fontStyle     = FontStyles.Bold;
            _breakerLabels[i].raycastTarget = false;
            var labelRt = labelGO.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0f, 1f);
            labelRt.anchorMax = new Vector2(1f, 1f);
            labelRt.pivot     = new Vector2(0f, 1f);
            labelRt.anchoredPosition = new Vector2(38f, rowY);
            labelRt.sizeDelta        = new Vector2(-50f, 24f);
        }

        // HUD always visible from the start
        _hudPanel.SetActive(false);
        _hudVisible = true;
    }
}