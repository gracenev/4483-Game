using UnityEngine;

// Draws a small dot in the centre of the screen as a crosshair.
// Gets bigger when the player is looking at something they can interact with.
public class Crosshair : MonoBehaviour
{
    public float dotSize      = 4f;   // normal crosshair size
    public float hoverDotSize = 10f;  // size when hovering over something interactable
    public float interactRange = 2f;

    private Texture2D dotTexture;
    private bool hovering = false;

    void Start()
    {
        // Create a plain white 1x1 texture to use as the dot
        dotTexture = new Texture2D(1, 1);
        dotTexture.SetPixel(0, 0, Color.white);
        dotTexture.Apply();
    }

    void Update()
    {
        // Cast a ray forward from the camera to check what the player is looking at
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            // Grow the dot if it's a button or a key
            hovering = hit.collider.GetComponent<InteractableButton>() != null
                    || hit.collider.GetComponent<PickupKey>() != null;
        }
        else
        {
            hovering = false;
        }
    }

    void OnGUI()
    {
        // Draw the dot centred on the screen
        float size = hovering ? hoverDotSize : dotSize;
        float x    = Screen.width  / 2f - size / 2f;
        float y    = Screen.height / 2f - size / 2f;
        GUI.DrawTexture(new Rect(x, y, size, size), dotTexture);
    }
}