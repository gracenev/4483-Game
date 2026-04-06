using UnityEngine;

/// <summary>
/// Attached to each wire stub. Stores which wire this is.
/// Your player raycast script should call Press() when the player
/// looks at this and presses interact.
/// </summary>
public class WireButton : MonoBehaviour
{
    [HideInInspector] public int    wireIndex;
    [HideInInspector] public string colorName;

    // Glow state
    private MeshRenderer _mr;
    private Color        _baseColor;
    private bool         _glowing;
    private float        _glowTimer;

    void Awake()
    {
        _mr        = GetComponent<MeshRenderer>();
        _baseColor = _mr.material.color;
    }

    void Update()
    {
        if (!_glowing) return;
        _glowTimer -= Time.deltaTime;
        if (_glowTimer <= 0f)
        {
            _glowing           = false;
            _mr.material.color = _baseColor;
        }
    }

    /// <summary>Called by the player raycast interact system.</summary>
    public void Press()
    {
        SimonSaysGame game = FindAnyObjectByType<SimonSaysGame>();
        if (game != null)
            game.OnWirePressed(wireIndex);
    }

    /// <summary>Flash bright white for 0.3s to give visual feedback.</summary>
    public void FlashGlow(float duration = 0.3f)
    {
        _glowing           = true;
        _glowTimer         = duration;
        _mr.material.color = Color.white;
    }

    /// <summary>Briefly dim to show it was pressed incorrectly.</summary>
    public void FlashError(float duration = 0.4f)
    {
        _glowing           = true;
        _glowTimer         = duration;
        _mr.material.color = new Color(0.1f, 0.1f, 0.1f);
    }
}