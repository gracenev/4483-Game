using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Manages the full flow of Floor 9.
// Handles the smoke, captions, key spawning, and what happens when the player wins or loses.
public class LevelManager : MonoBehaviour
{
    [Header("Buttons")]
    [Tooltip("The elevator call button - locked at first, triggers the key spawn when denied")]
    public InteractableButton lockedButton;
    [Tooltip("The close button inside the elevator - pressing this ends the level")]
    public InteractableButton closeButton;

    [Header("Key")]
    public GameObject keyObject;
    public float      spawnHeight = 1.19f;

    [Header("UI")]
    public CaptionUI   captionUI;
    public SmokeEffect smokeEffect;

    // These get built in code at runtime
    private Image       _fadeOverlay;
    private TMP_Text    _failText;
    private CanvasGroup _failGroup;

    // All 6 room positions on this floor
    private static readonly Vector3[] AllRooms = new Vector3[]
    {
        new Vector3(-11f, 0f, 14.1666f),
        new Vector3( 11f, 0f, 14.1666f),
        new Vector3(-11f, 0f, 42.5f),
        new Vector3( 11f, 0f, 42.5f),
        new Vector3(-11f, 0f, 70.8333f),
        new Vector3( 11f, 0f, 70.8333f),
    };

    // Different spots to try inside each room so the key avoids furniture
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

    // Keeping track of what's happened in the level
    private bool _buttonPressed = false;
    private bool _keySpawned    = false;
    private bool _levelEnded    = false;

    void Start()
    {
        // Try to find references automatically if they weren't assigned in the Inspector
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

        if (captionUI    == null) Debug.LogWarning("Couldn't find CaptionUI in the scene.");
        if (smokeEffect  == null) Debug.LogWarning("Couldn't find SmokeEffect in the scene.");
        if (lockedButton == null) Debug.LogWarning("Couldn't find Button_Call in the scene.");
        if (closeButton  == null) Debug.LogWarning("Couldn't find Button_Close in the scene.");
        if (keyObject    == null) Debug.LogWarning("Couldn't find the Key object in the scene.");

        // Hide the key until the player tries the elevator button
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

    // Wait for the floor title card to finish before starting captions
    IEnumerator LevelStart()
    {
        yield return new WaitForSeconds(5.5f);

        // Start the smoke right away - it builds up over about 2 minutes
        if (smokeEffect != null)
            smokeEffect.StartSmoke();

        if (captionUI != null)
            captionUI.Show("Oh no! There's a fire, I need to get to the elevator.", 4f);
    }

    // Fires when the player tries to press the locked elevator button
    public void OnLockedButtonPressed()
    {
        if (_buttonPressed) return;
        _buttonPressed = true;
        StartCoroutine(LockedButtonSequence());
    }

    IEnumerator LockedButtonSequence()
    {
        if (captionUI != null)
            captionUI.Show("Ahhh, it's locked! I need to find the key.", 4f);

        SpawnKeyInRandomRoom();

        // Wait for the first caption to finish before showing the next one
        yield return new WaitForSeconds(5.2f);

        if (captionUI != null)
            captionUI.Show("It's getting hard to see and hard to breathe, I need to find this key fast!", 5f);
    }

    // Picks a random room and drops the key somewhere that isn't blocked by furniture
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

        // If nothing worked just float it above the room
        if (!found)
        {
            chosenPos = new Vector3(rooms[0].x, spawnHeight + 2f, rooms[0].z);
            Debug.LogWarning("Couldn't find a clear spot for the key, floating it above the room.");
        }

        keyObject.transform.position = chosenPos;
        keyObject.SetActive(true);
        _keySpawned = true;

        Debug.Log($"Key spawned at {chosenPos}");
    }

    // Called by PickupKey when the player grabs it
    public void OnKeyPickedUp()
    {
        if (captionUI != null)
            captionUI.Show("Found it! I need to get to the elevator now.", 4f);
    }

    // Player pressed Button_Close - level is done
    public void OnLevelComplete()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        StartCoroutine(LevelCompleteSequence());
    }

    IEnumerator LevelCompleteSequence()
    {
        if (smokeEffect != null)
            yield return StartCoroutine(smokeEffect.FadeToBlack());
        else
            yield return new WaitForSeconds(1.5f);

        Debug.Log("Floor 9 complete - loading Floor 8.");
        SceneManager.LoadScene("Floor 8");
    }

    // Smoke ran out - player didn't make it in time
    void OnSmokeTimeout()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        StartCoroutine(SmokeFailSequence());
    }

    IEnumerator SmokeFailSequence()
    {
        if (smokeEffect != null)
            yield return StartCoroutine(smokeEffect.FadeToBlack());
        else
            yield return new WaitForSeconds(1.5f);

        // Show the fail message on the black screen
        _failGroup.alpha = 1f;
        _failText.text   = "You were consumed by the smoke...\n<size=24>Press Spacebar to try again</size>";

        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Builds the fail screen canvas in code - shows up when the player loses
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

        GameObject textGO  = new GameObject("FailText");
        textGO.transform.SetParent(failGO.transform, false);
        _failText            = textGO.AddComponent<TextMeshProUGUI>();
        _failText.fontSize   = 52;
        _failText.color      = new Color(0.9f, 0.5f, 0.1f);
        _failText.alignment  = TextAlignmentOptions.Center;
        _failText.fontStyle  = FontStyles.Bold;
        _failText.text       = "";
        RectTransform tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
    }

    public bool ButtonHasBeenPressed => _buttonPressed;
    public bool KeyHasBeenSpawned    => _keySpawned;
}