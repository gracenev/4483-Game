using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Full level flow manager for Floor 9:
///
///  START          → Caption: "Oh no! There's a fire..."
///                   Smoke effect begins immediately
///  BUTTON_CALL    → Player tries locked button → DenyPress fires
///                   Caption: "Ahhh, it's locked..."
///                   Key spawns in random room
///                   0.5s later → Caption: "It's getting hard to see..."
///  KEY PICKED UP  → Caption: "Found it!..."
///  BUTTON_CLOSE   → Fade to black → Level complete
///
/// SETUP:
///   1. Attach to any persistent GameObject (e.g. Floor_9 root).
///   2. Assign all references in the Inspector.
///   3. Make sure CaptionUI and SmokeEffect are in the scene and assigned.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Buttons")]
    [Tooltip("Button_Call — player tries to press it while locked, spawning the key")]
    public InteractableButton lockedButton;
    [Tooltip("Button_Close inside elevator — pressing it ends the level")]
    public InteractableButton closeButton;

    [Header("Key")]
    public GameObject keyObject;
    public float      spawnHeight = 1.19f;

    [Header("UI")]
    public CaptionUI   captionUI;
    public SmokeEffect smokeEffect;

    // ── Built at runtime ───────────────────────────────────────────────────
    private Image      _fadeOverlay;
    private TMP_Text   _failText;
    private CanvasGroup _failGroup;

    // ── Room data ──────────────────────────────────────────────────────────
    private static readonly Vector3[] AllRooms = new Vector3[]
    {
        new Vector3(-11f, 0f, 14.1666f),
        new Vector3( 11f, 0f, 14.1666f),
        new Vector3(-11f, 0f, 42.5f),
        new Vector3( 11f, 0f, 42.5f),
        new Vector3(-11f, 0f, 70.8333f),
        new Vector3( 11f, 0f, 70.8333f),
    };

    private static readonly Vector2[] SpawnOffsets = new Vector2[]
    {
        new Vector2( 0f,    0f),
        new Vector2( 1.5f,  0f),
        new Vector2(-1.5f,  0f),
        new Vector2( 0f,    1.5f),
        new Vector2( 0f,   -1.5f),
        new Vector2( 2.5f,  2.0f),
        new Vector2(-2.5f,  2.0f),
        new Vector2( 2.5f, -2.0f),
        new Vector2(-2.5f, -2.0f),
    };

    // ── State ──────────────────────────────────────────────────────────────
    private bool _buttonPressed = false;
    private bool _keySpawned    = false;
    private bool _levelEnded    = false;

    // ── Unity ──────────────────────────────────────────────────────────────
    void Start()
    {
        // Auto-find everything if not assigned in Inspector
        if (captionUI   == null) captionUI   = FindAnyObjectByType<CaptionUI>();
        if (smokeEffect == null) smokeEffect = FindAnyObjectByType<SmokeEffect>();

        if (lockedButton == null)
        {
            GameObject go = GameObject.Find("Button_Call");
            if (go != null) lockedButton = go.GetComponent<InteractableButton>();
        }
        if (closeButton == null)
        {
            GameObject go = GameObject.Find("Button_Close");
            if (go != null) closeButton = go.GetComponent<InteractableButton>();
        }
        if (keyObject == null)
        {
            keyObject = GameObject.Find("Key");
            if (keyObject == null) keyObject = GameObject.FindGameObjectWithTag("Key");
        }

        if (captionUI    == null) Debug.LogWarning("[LevelManager] CaptionUI not found!");
        if (smokeEffect  == null) Debug.LogWarning("[LevelManager] SmokeEffect not found!");
        if (lockedButton == null) Debug.LogWarning("[LevelManager] Button_Call not found!");
        if (closeButton  == null) Debug.LogWarning("[LevelManager] Button_Close not found!");
        if (keyObject    == null) Debug.LogWarning("[LevelManager] Key object not found!");

        if (keyObject != null) keyObject.SetActive(false);

        if (lockedButton != null) lockedButton.OnDeniedCallback += OnLockedButtonPressed;
        if (closeButton  != null) closeButton.OnPressedCallback += OnLevelComplete;
        if (smokeEffect  != null) smokeEffect.OnSmokeTimeout    += OnSmokeTimeout;

        BuildFailOverlay();
        StartCoroutine(LevelStart());
    }

    void OnDestroy()
    {
        if (lockedButton != null) lockedButton.OnDeniedCallback -= OnLockedButtonPressed;
        if (closeButton  != null) closeButton.OnPressedCallback -= OnLevelComplete;
        if (smokeEffect  != null) smokeEffect.OnSmokeTimeout    -= OnSmokeTimeout;
    }

    // ── Level start ────────────────────────────────────────────────────────
    IEnumerator LevelStart()
    {
        // Brief pause so scene fully loads
        yield return new WaitForSeconds(0.5f);

        // Smoke starts immediately and builds over ~2 minutes
        if (smokeEffect != null)
            smokeEffect.StartSmoke();

        // Opening caption
        if (captionUI != null)
            captionUI.Show("Oh no! There's a fire, I need to get to the elevator.", 4f);
    }

    // ── Step 1: Player tries locked Button_Call ────────────────────────────
    public void OnLockedButtonPressed()
    {
        if (_buttonPressed) return;
        _buttonPressed = true;

        StartCoroutine(LockedButtonSequence());
    }

    IEnumerator LockedButtonSequence()
    {
        // Show locked caption
        if (captionUI != null)
            captionUI.Show("Ahhh, it's locked! I need to find the key.", 4f);

        // Spawn the key
        SpawnKeyInRandomRoom();

        // Wait for first caption to fully finish (fadeIn + duration + fadeOut)
        // before showing the urgency line
        yield return new WaitForSeconds(5.2f);

        if (captionUI != null)
            captionUI.Show("It's getting hard to see and hard to breathe, I need to find this key fast!", 5f);
    }

    // ── Step 2: Spawn key ──────────────────────────────────────────────────
    void SpawnKeyInRandomRoom()
    {
        if (keyObject == null) return;

        Vector3[] rooms = (Vector3[])AllRooms.Clone();
        for (int i = rooms.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (rooms[i], rooms[j]) = (rooms[j], rooms[i]);
        }

        Vector3 chosenPos = Vector3.zero;
        bool    found     = false;

        foreach (Vector3 room in rooms)
        {
            foreach (Vector2 offset in SpawnOffsets)
            {
                Vector3 candidate = new Vector3(
                    room.x + offset.x,
                    spawnHeight,
                    room.z + offset.y);

                if (!Physics.CheckSphere(candidate, 0.35f))
                {
                    chosenPos = candidate;
                    found     = true;
                    break;
                }
            }
            if (found) break;
        }

        if (!found)
        {
            chosenPos = new Vector3(rooms[0].x, spawnHeight + 2f, rooms[0].z);
            Debug.LogWarning("[LevelManager] No clear spot — elevating key.");
        }

        keyObject.transform.position = chosenPos;
        keyObject.SetActive(true);
        _keySpawned = true;

        Debug.Log($"[LevelManager] Key spawned at {chosenPos}");
    }

    // ── Step 3: Key picked up (called by PickupKey) ────────────────────────
    public void OnKeyPickedUp()
    {
        if (captionUI != null)
            captionUI.Show("Found it! I need to get to the elevator now.", 4f);
    }

    // ── Step 4: Button_Close pressed → fade to black → done ───────────────
    public void OnLevelComplete()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        StartCoroutine(LevelCompleteSequence());
    }

    IEnumerator LevelCompleteSequence()
    {
        // Stop smoke, fade screen to black
        if (smokeEffect != null)
            yield return StartCoroutine(smokeEffect.FadeToBlack());
        else
            yield return new WaitForSeconds(1.5f);

        Debug.Log("[LevelManager] Level complete — ready for next scene.");

        // TODO: swap in your rolling-screen transition here, e.g.:
        // UnityEngine.SceneManagement.SceneManager.LoadScene("Floor_8");
    }

    // ── Smoke timeout — player consumed ───────────────────────────────────
    void OnSmokeTimeout()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        StartCoroutine(SmokeFailSequence());
    }

    IEnumerator SmokeFailSequence()
    {
        // Finish fading to full black
        if (smokeEffect != null)
            yield return StartCoroutine(smokeEffect.FadeToBlack());
        else
            yield return new WaitForSeconds(1.5f);

        // Show fail message over black screen
        _failGroup.alpha = 1f;
        _failText.text   = "You were consumed by the smoke...\n<size=24>Press Spacebar to try again</size>";

        // Wait for spacebar
        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ── Build fail overlay canvas ──────────────────────────────────────────
    void BuildFailOverlay()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        GameObject cGO   = new GameObject("Floor9FailCanvas");
        Canvas canvas    = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 201;
        CanvasScaler scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<GraphicRaycaster>();

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
        RectTransform fp = failGO.GetComponent<RectTransform>();
        fp.anchorMin = Vector2.zero; fp.anchorMax = Vector2.one;
        fp.offsetMin = Vector2.zero; fp.offsetMax = Vector2.zero;

        // Fail text
        GameObject textGO  = new GameObject("FailText");
        textGO.transform.SetParent(failGO.transform, false);
        _failText            = textGO.AddComponent<TextMeshProUGUI>();
        _failText.fontSize   = 52;
        _failText.color      = new Color(0.9f, 0.5f, 0.1f);  // smoky orange
        _failText.alignment  = TextAlignmentOptions.Center;
        _failText.fontStyle  = FontStyles.Bold;
        _failText.text       = "";
        RectTransform tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
    }

    // ── Public helpers ─────────────────────────────────────────────────────
    public bool ButtonHasBeenPressed => _buttonPressed;
    public bool KeyHasBeenSpawned    => _keySpawned;
}