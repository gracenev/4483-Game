using System.Collections;
using UnityEngine;

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
        if (keyObject != null) keyObject.SetActive(false);

        // Hook buttons
        if (lockedButton != null)
            lockedButton.OnDeniedCallback += OnLockedButtonPressed;
        else
            Debug.LogWarning("[LevelManager] Locked Button not assigned!");

        if (closeButton != null)
            closeButton.OnPressedCallback += OnLevelComplete;
        else
            Debug.LogWarning("[LevelManager] Close Button not assigned!");

        // Start level — show opening caption + begin smoke
        StartCoroutine(LevelStart());
    }

    void OnDestroy()
    {
        if (lockedButton != null) lockedButton.OnDeniedCallback -= OnLockedButtonPressed;
        if (closeButton  != null) closeButton.OnPressedCallback -= OnLevelComplete;
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
            int j = Random.Range(0, i + 1);
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

    // ── Public helpers ─────────────────────────────────────────────────────
    public bool ButtonHasBeenPressed => _buttonPressed;
    public bool KeyHasBeenSpawned    => _keySpawned;
}