using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Handles the full Simon Says circuit game on Floor 3.
// The player watches a sequence of wires light up, then has to click them in the same order.
// 3 rounds total, sequence gets longer each round.
// Run out of attempts and the level restarts. Complete all rounds and the elevator works.
[RequireComponent(typeof(AudioSource))]
public class SimonSaysGame : MonoBehaviour
{
    // Wire colour names used to find the wire GameObjects in the scene
    private static readonly string[] ColorNames = new string[]
    { "Red", "Blue", "Green", "Yellow", "Purple", "Orange" };

    [Header("Game Settings")]
    public int   totalRounds    = 3;
    public int   startingLength = 3;    // how many wires in the first round's sequence
    public float showInterval   = 0.6f; // how long each wire glows when showing the sequence
    public float showGap        = 0.2f; // pause between each glow
    public int   maxAttempts    = 3;    // wrong presses before the round resets

    [Header("References")]
    public InteractableButton closeButton;
    [Tooltip("The key object — gets made invisible once the circuit is fixed")]
    public GameObject keyObject;

    [Header("Audio")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip successSound;

    [Header("Fade")]
    public float fadeSpeed = 1.5f;

    // UI built at runtime
    private CaptionUI3  _caption;
    private Image       _fadeOverlay;
    private TMP_Text    _hintText;    // status text at the top of the screen
    private Image       _failOverlay;
    private CanvasGroup _failGroup;
    private TMP_Text    _failText;

    // Game state
    private WireButton[] _wires;
    private List<int>    _sequence       = new List<int>();
    private int          _inputIndex     = 0;
    private int          _currentRound   = 0;
    private int          _attemptsLeft;
    private bool         _playerCanInput = false;
    private bool         _gameStarted    = false;
    private bool         _gameComplete   = false;
    private AudioSource  _audio;

    void Start()
    {
        _audio        = GetComponent<AudioSource>();
        _attemptsLeft = maxAttempts;

        // Find each wire by name in the scene
        _wires = new WireButton[6];
        for (int i = 0; i < 6; i++)
        {
            string name   = $"Wire_{ColorNames[i]}";
            GameObject go = GameObject.Find(name);
            if (go != null)
                _wires[i] = go.GetComponent<WireButton>();
            else
                Debug.LogWarning($"Couldn't find wire GameObject named: {name}");
        }

        // Keep the key hidden until the game is complete
        if (keyObject != null) keyObject.SetActive(false);

        if (closeButton != null)
            closeButton.OnPressedCallback += OnLevelComplete;

        BuildUI();
        StartCoroutine(OpeningSequence());
    }

    void OnDestroy()
    {
        if (closeButton != null)
            closeButton.OnPressedCallback -= OnLevelComplete;
    }

    // Opening captions that play when the scene starts
    IEnumerator OpeningSequence()
    {
        yield return new WaitForSeconds(1f);
        _caption?.Show("Ahh shoot... the elevator isn't working. I need to fix the circuit to make it functional.", 5f);
        yield return new WaitForSeconds(6f);
        _caption?.Show("Click the fuse box to begin.", 3f);
    }

    // Called by FuseBoxInteractable when the player clicks the fuse box
    public void StartGame()
    {
        if (_gameStarted) return;
        _gameStarted = true;
        _caption?.Show("Malfunctioning circuit detected. Restore the sequence!", 3f);
        StartCoroutine(StartRoundAfterDelay(1.5f));
    }

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

        _inputIndex     = 0;
        _playerCanInput = false;
        _attemptsLeft   = maxAttempts;

        // Sequence gets one wire longer each round
        int seqLength = startingLength + (_currentRound - 1);
        _sequence.Clear();
        for (int i = 0; i < seqLength; i++)
            _sequence.Add(UnityEngine.Random.Range(0, 6));

        SetHint($"ROUND {_currentRound} / {totalRounds} — MEMORIZE THE SEQUENCE");
        _caption?.Show($"Round {_currentRound} of {totalRounds} — watch carefully!", 2f);

        StartCoroutine(ShowSequence());
    }

    // Flashes each wire in the sequence so the player can memorize it
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

    // Called by WireButton when the player clicks a wire
    public void OnWirePressed(int wireIndex)
    {
        if (!_playerCanInput || _gameComplete) return;

        if (wireIndex == _sequence[_inputIndex])
        {
            // Correct wire pressed
            if (_wires[wireIndex] != null) _wires[wireIndex].FlashGlow(0.25f);
            if (correctSound != null) _audio.PlayOneShot(correctSound);
            _inputIndex++;
            SetHint($"INPUT: {_inputIndex} / {_sequence.Count}  |  ATTEMPTS: {_attemptsLeft}");

            if (_inputIndex >= _sequence.Count)
            {
                // Finished the full sequence for this round
                _playerCanInput = false;
                _caption?.Show($"Round {_currentRound} complete!", 1.5f);
                StartCoroutine(NextRoundDelay());
            }
        }
        else
        {
            // Wrong wire pressed
            if (_wires[wireIndex] != null) _wires[wireIndex].FlashError(0.4f);
            if (wrongSound != null) _audio.PlayOneShot(wrongSound);
            _attemptsLeft--;

            if (_attemptsLeft <= 0)
            {
                // Out of attempts — restart the whole level
                _playerCanInput = false;
                _caption?.Show("Circuit overloaded! Restarting sequence...", 2f);
                StartCoroutine(RestartRound());
            }
            else
            {
                // Still have attempts left — replay the sequence so they can try again
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

    // No attempts left — fade to black and show the fail screen
    IEnumerator RestartRound()
    {
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeToBlack());

        _failGroup.alpha = 1f;
        _failText.text   = "Circuit overloaded...\n<size=24>Press Spacebar to Restart</size>";

        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Wrong press — show the sequence again before the player retries
    IEnumerator ReplayAfterError()
    {
        _playerCanInput = false;
        yield return new WaitForSeconds(1f);
        StartCoroutine(ShowSequence());
    }

    // All 3 rounds done — circuit is fixed
    void OnAllRoundsComplete()
    {
        _gameComplete = true;
        if (successSound != null) _audio.PlayOneShot(successSound);
        _caption?.Show("Circuit restored! The elevator is operational.", 3f);
        SetHint("CIRCUIT RESTORED");

        // Make the key active but invisible — player just needs to press Button_Close
        if (keyObject != null)
        {
            keyObject.SetActive(true);
            foreach (var r in keyObject.GetComponentsInChildren<Renderer>())
                r.enabled = false;
        }

        Debug.Log("All rounds complete — press Button_Close to leave.");
    }

    // Button_Close was pressed — fade out and finish the level
    void OnLevelComplete()
    {
        if (!_gameComplete) return;
        StartCoroutine(FadeOutSequence());
    }

    IEnumerator FadeOutSequence()
    {
        yield return StartCoroutine(FadeToBlack());
        Debug.Log("Floor 3 complete — ready for the next scene.");
        // TODO: SceneManager.LoadScene("Floor_6");
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

    void SetHint(string text)
    {
        if (_hintText != null) _hintText.text = text;
    }

    // Builds all the UI elements in code — no manual Canvas setup needed
    void BuildUI()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        _caption = gameObject.AddComponent<CaptionUI3>();

        // Main canvas for this floor's UI
        GameObject cGO = new GameObject("Floor3UICanvas");
        Canvas canvas  = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = cGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode         = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Fullscreen black overlay for fading
        GameObject fadeGO  = new GameObject("FadeOverlay");
        fadeGO.transform.SetParent(cGO.transform, false);
        _fadeOverlay               = fadeGO.AddComponent<Image>();
        _fadeOverlay.sprite        = white;
        _fadeOverlay.color         = new Color(0f, 0f, 0f, 0f);
        _fadeOverlay.raycastTarget = false;
        var frt = fadeGO.GetComponent<RectTransform>();
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
        frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;

        // Status text at the top of the screen showing round and input progress
        GameObject hGO = new GameObject("HintText");
        hGO.transform.SetParent(cGO.transform, false);
        _hintText               = hGO.AddComponent<TextMeshProUGUI>();
        _hintText.fontSize      = 24;
        _hintText.color         = new Color(0.2f, 1f, 0.4f);  // green like a terminal
        _hintText.alignment     = TextAlignmentOptions.Center;
        _hintText.fontStyle     = FontStyles.Bold;
        _hintText.text          = "";
        _hintText.raycastTarget = false;
        var hrt = hGO.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0f, 0.88f);
        hrt.anchorMax = new Vector2(1f, 1f);
        hrt.offsetMin = Vector2.zero;
        hrt.offsetMax = Vector2.zero;

        // Fail panel — hidden until the player runs out of attempts
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

        // The actual fail message text
        GameObject ftGO  = new GameObject("FailText");
        ftGO.transform.SetParent(failGO.transform, false);
        _failText               = ftGO.AddComponent<TextMeshProUGUI>();
        _failText.fontSize      = 52;
        _failText.color         = new Color(0.9f, 0.2f, 0.2f);
        _failText.alignment     = TextAlignmentOptions.Center;
        _failText.fontStyle     = FontStyles.Bold;
        _failText.text          = "";
        _failText.raycastTarget = false;
        var ftrt = ftGO.GetComponent<RectTransform>();
        ftrt.anchorMin = Vector2.zero; ftrt.anchorMax = Vector2.one;
        ftrt.offsetMin = Vector2.zero; ftrt.offsetMax = Vector2.zero;
    }
}