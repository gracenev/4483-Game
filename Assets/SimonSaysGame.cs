using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simon Says game for Floor 3 — Circuit Recovery.
///
/// Flow:
///   Player clicks wire panel → game starts
///   Round 1: show sequence of 3 → player repeats
///   Round 2: show sequence of 4 → player repeats
///   Round 3: show sequence of 5 → player repeats
///   All correct → key spawns invisible + screen fades black
///   Wrong input → flash error + brief freeze + replay round
///
/// SETUP:
///   1. Attach to any persistent GameObject.
///   2. Assign closeButton (Button_Close) in Inspector.
///   3. Wire panel spawns itself — just run WirePanel.SpawnWirePanel() first.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SimonSaysGame : MonoBehaviour
{
    private static readonly string[] ColorNames = new string[]
    { "Red", "Blue", "Green", "Yellow", "Purple", "Orange" };

    [Header("Game Settings")]
    public int   totalRounds       = 3;
    public int   startingLength    = 3;   // Round 1 sequence length
    public float showInterval      = 0.6f; // seconds each wire glows during show
    public float showGap           = 0.2f; // gap between glows
    public int   maxAttempts       = 3;

    [Header("References")]
    public InteractableButton closeButton;
    public GameObject         keyObject;   // assign your key — will be set inactive+invisible

    [Header("Audio")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip successSound;

    [Header("Fade")]
    public float fadeSpeed = 1.5f;

    // ── Built at runtime ───────────────────────────────────────────────────
    private CaptionUI3   _caption;
    private Image        _fadeOverlay;
    private TMP_Text     _hintText;   // shows "INPUT: X / Y" and "MEMORIZE"

    // ── State ──────────────────────────────────────────────────────────────
    private WireButton[]     _wires;
    private List<int>        _sequence      = new List<int>();
    private int              _inputIndex    = 0;
    private int              _currentRound  = 0;
    private int              _attemptsLeft;
    private bool             _playerCanInput = false;
    private bool             _gameStarted    = false;
    private bool             _gameComplete   = false;
    private AudioSource      _audio;
    private Image            _failOverlay;
    private CanvasGroup _failGroup;
    private TMP_Text         _failText;
    private ElevatorButton   _elevatorButton;

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        _audio        = GetComponent<AudioSource>();
        _attemptsLeft = maxAttempts;

        // Auto-find wires
        _wires = new WireButton[6];
        for (int i = 0; i < 6; i++)
        {
            string name = $"Wire_{ColorNames[i]}";
            GameObject go = GameObject.Find(name);
            if (go != null)
                _wires[i] = go.GetComponent<WireButton>();
            else
                Debug.LogWarning($"[SimonSays] Wire not found: {name}");
        }

        // Hide key
        if (keyObject != null) keyObject.SetActive(false);

        // Hook close button for level end
        if (closeButton != null)
        {
            // Gate via game completion, not a key
            closeButton.requiresKey = false;
            closeButton.OnPressedCallback += OnLevelComplete;

            // Lock the ElevatorButton so the scene cannot load until the game is complete
            _elevatorButton = closeButton.GetComponent<ElevatorButton>();
            if (_elevatorButton != null)
                _elevatorButton.locked = true;
        }

        BuildUI();
        StartCoroutine(OpeningSequence());
    }

    void OnDestroy()
    {
        if (closeButton != null)
            closeButton.OnPressedCallback -= OnLevelComplete;
    }

    // ── Opening captions ──────────────────────────────────────────────────
    IEnumerator OpeningSequence()
    {
        yield return new WaitForSeconds(1f);
        _caption?.Show("Ahh shoot... the elevator isn't working. I need to fix the circuit to make it functional.", 5f);
        yield return new WaitForSeconds(6f);
        _caption?.Show("Click the fuse box to begin.", 3f);
    }

    // ── Called when player interacts with the wire panel box (FuseBox) ─────
    public void StartGame()
    {
        if (_gameStarted) return;
        _gameStarted = true;
        _caption?.Show("Malfunctioning circuit detected. Restore the sequence!", 3f);
        StartCoroutine(StartRoundAfterDelay(1.5f));
    }

    // ── Round logic ────────────────────────────────────────────────────────
    IEnumerator StartRoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextRound();
    }

    void StartNextRound()
    {
        _currentRound++;
        if (_currentRound > totalRounds)
        {
            OnAllRoundsComplete();
            return;
        }

        _inputIndex    = 0;
        _playerCanInput = false;
        _attemptsLeft  = maxAttempts;

        // Build sequence — add one new random wire each round
        int seqLength = startingLength + (_currentRound - 1);
        _sequence.Clear();
        for (int i = 0; i < seqLength; i++)
            _sequence.Add(UnityEngine.Random.Range(0, 6));

        SetHint($"ROUND {_currentRound} / {totalRounds} — MEMORIZE THE SEQUENCE");
        _caption?.Show($"Round {_currentRound} of {totalRounds} — watch carefully!", 2f);

        StartCoroutine(ShowSequence());
    }

    IEnumerator ShowSequence()
    {
        yield return new WaitForSeconds(1.8f);

        SetHint("MEMORIZE THE SEQUENCE");

        foreach (int idx in _sequence)
        {
            if (idx >= 0 && idx < _wires.Length && _wires[idx] != null)
                _wires[idx].FlashGlow(showInterval);

            if (correctSound != null) _audio.PlayOneShot(correctSound, 0.5f);
            yield return new WaitForSeconds(showInterval + showGap);
        }

        yield return new WaitForSeconds(0.3f);
        SetHint($"INPUT: 0 / {_sequence.Count}  |  ATTEMPTS: {_attemptsLeft}");
        _playerCanInput = true;
    }

    // ── Player presses a wire ──────────────────────────────────────────────
    public void OnWirePressed(int wireIndex)
    {
        if (!_playerCanInput || _gameComplete) return;

        if (wireIndex == _sequence[_inputIndex])
        {
            // Correct
            if (_wires[wireIndex] != null) _wires[wireIndex].FlashGlow(0.25f);
            if (correctSound != null) _audio.PlayOneShot(correctSound);
            _inputIndex++;
            SetHint($"INPUT: {_inputIndex} / {_sequence.Count}  |  ATTEMPTS: {_attemptsLeft}");

            if (_inputIndex >= _sequence.Count)
            {
                // Round complete
                _playerCanInput = false;
                _caption?.Show($"Round {_currentRound} complete!", 1.5f);
                StartCoroutine(NextRoundDelay());
            }
        }
        else
        {
            // Wrong
            if (_wires[wireIndex] != null) _wires[wireIndex].FlashError(0.4f);
            if (wrongSound != null) _audio.PlayOneShot(wrongSound);
            _attemptsLeft--;

            if (_attemptsLeft <= 0)
            {
                _playerCanInput = false;
                _caption?.Show("Circuit overloaded! Restarting sequence...", 2f);
                StartCoroutine(RestartRound());
            }
            else
            {
                _inputIndex = 0;
                SetHint($"WRONG! Starting over — ATTEMPTS: {_attemptsLeft}");
                StartCoroutine(ReplayAfterError());
            }
        }
    }

    IEnumerator NextRoundDelay()
    {
        yield return new WaitForSeconds(2f);
        StartNextRound();
    }

    IEnumerator RestartRound()
    {
        // Out of attempts — fade to black and show fail screen
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeToBlack());

        // Show fail message
        _failGroup.alpha  = 1f;
        _failText.text = "Circuit overloaded...\n<size=24>Press Spacebar to Restart</size>";

        // Wait for spacebar
        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator ReplayAfterError()
    {
        _playerCanInput = false;
        yield return new WaitForSeconds(1f);
        // Replay the sequence again so player can re-memorize
        StartCoroutine(ShowSequence());
    }

    // ── All rounds complete ────────────────────────────────────────────────
    void OnAllRoundsComplete()
    {
        _gameComplete = true;
        if (successSound != null) _audio.PlayOneShot(successSound);
        _caption?.Show("Circuit restored! The elevator is operational.", 3f);
        SetHint("CIRCUIT RESTORED");

        // Spawn key invisible (player just needs to press Button_Close)
        if (keyObject != null)
        {
            keyObject.SetActive(true);
            // Make all renderers invisible
            foreach (var r in keyObject.GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }

        // Unlock the elevator button now that the game is complete
        if (_elevatorButton != null)
            _elevatorButton.locked = false;

        Debug.Log("[SimonSays] All rounds complete — press Button_Close to proceed.");
    }

    // ── Button_Close pressed → fade to black ──────────────────────────────
    void OnLevelComplete()
    {
        if (!_gameComplete) return;
        StartCoroutine(FadeOutSequence());
    }

    IEnumerator FadeOutSequence()
    {
        yield return StartCoroutine(FadeToBlack());
        // ElevatorButton handles the actual scene load after its own delay
    }

    IEnumerator FadeToBlack()
    {
        if (_fadeOverlay == null) yield break;
        Color c = _fadeOverlay.color;
        while (c.a < 1f)
        {
            c.a += fadeSpeed * Time.deltaTime;
            c.a  = Mathf.Clamp01(c.a);
            _fadeOverlay.color = c;
            yield return null;
        }
    }

    // ── UI ─────────────────────────────────────────────────────────────────
    void SetHint(string text)
    {
        if (_hintText != null) _hintText.text = text;
    }

    void BuildUI()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Build a CaptionUI3 in code
        _caption = gameObject.AddComponent<CaptionUI3>();

        // Canvas
        GameObject cGO = new GameObject("Floor3UICanvas");
        Canvas canvas  = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = cGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode         = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

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

        // Hint text — top of screen
        GameObject hGO = new GameObject("HintText");
        hGO.transform.SetParent(cGO.transform, false);
        _hintText           = hGO.AddComponent<TextMeshProUGUI>();
        _hintText.fontSize  = 24;
        _hintText.color     = new Color(0.2f, 1f, 0.4f);   // green terminal colour
        _hintText.alignment = TextAlignmentOptions.Center;
        _hintText.fontStyle = FontStyles.Bold;
        _hintText.text      = "";
        _hintText.raycastTarget = false;
        var hrt = hGO.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0f, 0.88f);
        hrt.anchorMax = new Vector2(1f, 1f);
        hrt.offsetMin = Vector2.zero;
        hrt.offsetMax = Vector2.zero;

        // Fail panel
        GameObject failGO    = new GameObject("FailPanel");
        failGO.transform.SetParent(cGO.transform, false);
        Image failBg         = failGO.AddComponent<Image>();
        failBg.sprite        = white;
        failBg.color         = new Color(0f, 0f, 0f, 0f);
        failBg.raycastTarget = false;
        _failGroup           = failGO.AddComponent<CanvasGroup>();
        _failGroup.alpha     = 0f;
        _failGroup.blocksRaycasts = false;
        var frt2 = failGO.GetComponent<RectTransform>();
        frt2.anchorMin = Vector2.zero; frt2.anchorMax = Vector2.one;
        frt2.offsetMin = Vector2.zero; frt2.offsetMax = Vector2.zero;

        GameObject ftGO  = new GameObject("FailText");
        ftGO.transform.SetParent(failGO.transform, false);
        _failText            = ftGO.AddComponent<TextMeshProUGUI>();
        _failText.fontSize   = 52;
        _failText.color      = new Color(0.9f, 0.2f, 0.2f);
        _failText.alignment  = TextAlignmentOptions.Center;
        _failText.fontStyle  = FontStyles.Bold;
        _failText.text       = "";
        _failText.raycastTarget = false;
        var ftrt = ftGO.GetComponent<RectTransform>();
        ftrt.anchorMin = Vector2.zero; ftrt.anchorMax = Vector2.one;
        ftrt.offsetMin = Vector2.zero; ftrt.offsetMax = Vector2.zero;
    }
}