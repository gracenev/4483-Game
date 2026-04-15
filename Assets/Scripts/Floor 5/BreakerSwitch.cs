using UnityEngine;

// Attach to each breaker box in the scene.
// When the player clicks it, it flips its own state and tells the
// Floor5LevelManager to update any linked breakers and check the solution.
public class BreakerSwitch : MonoBehaviour
{
    [Header("Identity")]
    public int    breakerID   = 0;
    public string breakerName = "B1";
    public Color  breakerColor = Color.blue;

    [Header("State")]
    public bool isOn = false;

    [Header("Linked Lights")]
    [Tooltip("Lights in this breaker's zone that turn on/off with it")]
    public Light[] linkedLights;

    [Header("Visual")]
    [Tooltip("The handle/lever part of the breaker box that rotates")]
    public Transform leverTransform;

    private Renderer         _rend;
    private Floor5LevelManager _manager;

    void Start()
    {
        _rend    = GetComponent<Renderer>();
        _manager = FindAnyObjectByType<Floor5LevelManager>();
        UpdateVisuals();
    }

    // Called by PlayerInteract when the player clicks this breaker
    public void Interact()
    {
        isOn = !isOn;
        UpdateVisuals();
        _manager?.OnBreakerFlipped(breakerID);
        Debug.Log($"Breaker {breakerName} is now {(isOn ? "ON" : "OFF")}");
    }

    // Flip state silently (called by manager for linked breakers)
    public void ForceState(bool state)
    {
        isOn = state;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // Change the breaker colour to show on/off state
        if (_rend != null)
        {
            Color c = isOn ? breakerColor : new Color(0.15f, 0.15f, 0.15f);
            _rend.material.color = c;
        }

        // Rotate the lever up when on, down when off
        if (leverTransform != null)
            leverTransform.localRotation = Quaternion.Euler(isOn ? -30f : 30f, 0f, 0f);

        // Toggle linked lights
        foreach (var lt in linkedLights)
            if (lt != null) lt.enabled = isOn;
    }
}