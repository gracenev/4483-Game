using UnityEngine;
using System;

// A physical button that the player can press in the world.
// Has two events other scripts can listen to - one for a successful press
// and one for when the player tries to press it without a key.
public class InteractableButton : MonoBehaviour
{
    public float pressDistance = 0.05f;
    public float pressSpeed    = 5f;
    public Color pressedColor  = Color.black;
    public Color lockedColor   = Color.red;
    public bool  requiresKey   = true;

    // Fires when the button is successfully pressed
    public event Action OnPressedCallback;

    // Fires when the player tries to press it but doesn't have the key
    // - the LevelManager listens to this to trigger the key spawn
    public event Action OnDeniedCallback;

    private Vector3  originalPos;
    private Vector3  pressedPos;
    private Color    originalColor;
    private Renderer rend;
    private bool     isPressed = false;
    private bool     returning = false;

    void Start()
    {
        originalPos   = transform.localPosition;
        pressedPos    = originalPos + (Vector3.right * pressDistance);
        rend          = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    void Update()
    {
        if (isPressed)
        {
            // Move toward the pressed position
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
            // Spring back to the original position
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

    // Returns true if the player is allowed to press this button
    public bool CanPress(GameObject player)
    {
        if (!requiresKey) return true;
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        return inventory != null && inventory.hasKey;
    }

    // Called when the player successfully interacts with the button
    public void Press()
    {
        if (!isPressed && !returning)
        {
            isPressed           = true;
            rend.material.color = pressedColor;
            Debug.Log("Button pressed!");
            OnPressedCallback?.Invoke();
        }
    }

    // Called when the player tries to press the button but doesn't have the key
    public void DenyPress()
    {
        rend.material.color = lockedColor;
        Invoke("ResetColor", 0.3f);
        Debug.Log("Needs a key to press this.");
        OnDeniedCallback?.Invoke();
    }

    private void ResetColor()
    {
        rend.material.color = originalColor;
    }
}