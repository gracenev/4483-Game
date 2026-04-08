using UnityEngine;

// Attached to each coloured wire stub on the elevator wall.
// Handles the visual feedback when a wire is clicked or shown during the sequence.
public class WireButton : MonoBehaviour
{
    [HideInInspector] public int    wireIndex;  // which wire this is (0-5)
    [HideInInspector] public string colorName;  // just for identification in the hierarchy

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

        // Count down the glow timer and restore the original colour when it runs out
        _glowTimer -= Time.deltaTime;
        if (_glowTimer <= 0f)
        {
            _glowing           = false;
            _mr.material.color = _baseColor;
        }
    }

    // Called by PlayerInteract when the player clicks on this wire
    public void Press()
    {
        SimonSaysGame game = FindAnyObjectByType<SimonSaysGame>();
        if (game != null)
            game.OnWirePressed(wireIndex);
    }

    // Flashes the wire white - used both when the sequence is shown and when the player gets it right
    public void FlashGlow(float duration = 0.3f)
    {
        _glowing           = true;
        _glowTimer         = duration;
        _mr.material.color = Color.white;
    }

    // Briefly turns the wire almost black to show the player pressed the wrong one
    public void FlashError(float duration = 0.4f)
    {
        _glowing           = true;
        _glowTimer         = duration;
        _mr.material.color = new Color(0.1f, 0.1f, 0.1f);
    }
}