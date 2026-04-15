using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Manages Floor 6 — handles the elevator exit and the void fall fail state.
public class Floor6LevelManager : MonoBehaviour
{
    [Header("Buttons")]
    public InteractableButton closeButton;

    [Header("Fade Settings")]
    public float fadeSpeed = 1.5f;

    // Built at runtime
    private Image       _fadeOverlay;
    private CanvasGroup _failGroup;
    private TMP_Text    _failText;

    private bool _levelEnded = false;
    private CaptionUI3 _caption;

    void Start()
    {
        // Auto-find close button if not assigned
        if (closeButton == null)
        {
            GameObject go = GameObject.Find("Button_Close");
            if (go != null) closeButton = go.GetComponent<InteractableButton>();
        }

        if (closeButton != null)
            closeButton.OnPressedCallback += OnLevelComplete;
        else
            Debug.LogWarning("Floor6LevelManager: Button_Close not found.");

        BuildUI();
        _caption = gameObject.AddComponent<CaptionUI3>();
        StartCoroutine(OpeningSequence());
        StartCoroutine(WatchForFirstDesk());

        // Hook up the void floor trigger
        GameObject voidFloor = GameObject.Find("VoidFloor");
        if (voidFloor != null)
        {
            VoidKillZone zone = voidFloor.AddComponent<VoidKillZone>();
            zone.manager = this;
            // Make sure VoidFloor has a trigger collider
            Collider col = voidFloor.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }
        else
            Debug.LogWarning("Floor6LevelManager: VoidFloor not found.");
    }

    // Opening captions when the scene loads
    IEnumerator OpeningSequence()
    {
        yield return new WaitForSeconds(1f);
        _caption?.Show("Oh no... there's fire everywhere! The whole floor's destroyed, I need to get to the elevator!", 5f);
    }

    // Watch for the player landing on TippedDesk_0 and show a caption
    IEnumerator WatchForFirstDesk()
    {
        yield return new WaitForSeconds(2f);

        GameObject desk = GameObject.Find("TippedDesk_0");
        if (desk == null) yield break;

        bool shown = false;
        while (!shown)
        {
            if (desk == null) yield break;

            // Check if the player is standing on it
            Vector3 size    = desk.transform.lossyScale;
            Vector3 centre  = desk.transform.position + desk.transform.up * (size.y * 0.5f + 0.2f);
            Vector3 halfExt = new Vector3(size.x * 0.45f, 0.2f, size.z * 0.45f);

            Collider[] hits = Physics.OverlapBox(centre, halfExt, desk.transform.rotation);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    _caption?.Show("This desk is so unstable... it feels like it's about to give way!", 4f);
                    shown = true;
                    break;
                }
            }
            yield return null;
        }
    }

    void OnDestroy()
    {
        if (closeButton != null)
            closeButton.OnPressedCallback -= OnLevelComplete;
    }

    // Button_Close pressed — level complete
    void OnLevelComplete()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        StartCoroutine(LevelCompleteSequence());
    }

    IEnumerator LevelCompleteSequence()
    {
        yield return StartCoroutine(FadeToBlack());
        Debug.Log("Floor 6 complete — loading next scene.");
        // TODO: SceneManager.LoadScene("NextFloor");
    }

    // Player fell into the void
    public void TriggerFallDeath()
    {
        if (_levelEnded) return;
        _levelEnded = true;
        StartCoroutine(FallDeathSequence());
    }

    IEnumerator FallDeathSequence()
    {
        yield return StartCoroutine(FadeToBlack());

        _failGroup.alpha = 1f;
        _failText.text   = "You have been engulfed in flames...\n<size=26>Press Spacebar to try again</size>";

        while (!Input.GetKeyDown(KeyCode.Space))
            yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

    void BuildUI()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        GameObject cGO = new GameObject("Floor6OverlayCanvas");
        Canvas canvas  = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        CanvasScaler scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        cGO.AddComponent<GraphicRaycaster>();

        // Fullscreen fade overlay
        GameObject fadeGO  = new GameObject("FadeOverlay");
        fadeGO.transform.SetParent(cGO.transform, false);
        _fadeOverlay               = fadeGO.AddComponent<Image>();
        _fadeOverlay.sprite        = white;
        _fadeOverlay.color         = new Color(0f, 0f, 0f, 0f);
        _fadeOverlay.raycastTarget = false;
        var frt = fadeGO.GetComponent<RectTransform>();
        frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
        frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;

        // Fail panel — hidden until player falls
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

        // Fail text
        GameObject textGO  = new GameObject("FailText");
        textGO.transform.SetParent(failGO.transform, false);
        _failText            = textGO.AddComponent<TextMeshProUGUI>();
        _failText.fontSize   = 52;
        _failText.color      = new Color(1f, 0.35f, 0.1f);  // fire orange
        _failText.alignment  = TextAlignmentOptions.Center;
        _failText.fontStyle  = FontStyles.Bold;
        _failText.text       = "";
        _failText.raycastTarget = false;
        var tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
    }
}

// Small helper that sits on the VoidFloor and reports back to the manager
public class VoidKillZone : MonoBehaviour
{
    public Floor6LevelManager manager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            manager?.TriggerFallDeath();
    }
}