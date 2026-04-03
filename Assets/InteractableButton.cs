using UnityEngine;
using System;

/// <summary>
/// Original InteractableButton — extended with an OnPressedCallback event
/// so ElevatorController (and anything else) can subscribe to button presses
/// without polling or modifying this script further.
/// </summary>
public class InteractableButton : MonoBehaviour
{
    public float pressDistance = 0.05f;
    public float pressSpeed    = 5f;
    public Color pressedColor  = Color.black;
    public Color lockedColor   = Color.red;
    public bool  requiresKey   = true;

    /// <summary>
    /// Subscribe to this to be notified when the button is successfully pressed.
    /// ElevatorController hooks in here automatically in Start().
    /// </summary>
    public event Action OnPressedCallback;

    /// <summary>
    /// Fires when the player tries to press the button but doesn't have the key.
    /// LevelManager listens to this to spawn the key on first attempt.
    /// </summary>
    public event Action OnDeniedCallback;

    private Vector3  originalPos;
    private Vector3  pressedPos;
    private Color    originalColor;
    private Renderer rend;
    private bool     isPressed  = false;
    private bool     returning  = false;

    void Start()
    {
        originalPos = transform.localPosition;
        pressedPos  = originalPos + (Vector3.right * pressDistance);
        rend        = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    void Update()
    {
        if (isPressed)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, pressedPos, Time.deltaTime * pressSpeed);

            if (Vector3.Distance(transform.localPosition, pressedPos) < 0.001f)
            {
                isPressed = false;
                returning = true;
            }
        }
        else if (returning)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition, originalPos, Time.deltaTime * pressSpeed);
            rend.material.color = Color.Lerp(
                rend.material.color, originalColor, Time.deltaTime * pressSpeed);

            if (Vector3.Distance(transform.localPosition, originalPos) < 0.001f)
            {
                transform.localPosition = originalPos;
                rend.material.color     = originalColor;
                returning               = false;
            }
        }
    }

    public bool CanPress(GameObject player)
    {
        if (!requiresKey) return true;
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        return inventory != null && inventory.hasKey;
    }

    public void Press()
    {
        if (!isPressed && !returning)
        {
            isPressed = true;
            rend.material.color = pressedColor;
            Debug.Log("Button pressed!");
            OnPressedCallback?.Invoke();   // ← notify subscribers
        }
    }

    public void DenyPress()
    {
        rend.material.color = lockedColor;
        Invoke("ResetColor", 0.3f);
        Debug.Log("Need a key!");
        OnDeniedCallback?.Invoke();
    }

    private void ResetColor()
    {
        rend.material.color = originalColor;
    }
}