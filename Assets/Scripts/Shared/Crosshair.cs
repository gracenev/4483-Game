using UnityEngine;

// Draws a crosshair dot in the centre of the screen.
// When hovering over something interactable it switches to a larger
// outlined square with no fill so the player knows they can click.
public class Crosshair : MonoBehaviour
{
    public float dotSize       = 6f;    // normal filled dot size
    public float hoverSize     = 8f;   // outline box size when hovering
    public float outlineThick  = 2f;    // thickness of the outline
    public float interactRange = 2f;

    private Texture2D _dotTexture;
    private Texture2D _whiteTexture;
    private bool      _hovering = false;

    void Start()
    {
        _dotTexture = new Texture2D(1, 1);
        _dotTexture.SetPixel(0, 0, Color.white);
        _dotTexture.Apply();

        _whiteTexture = new Texture2D(1, 1);
        _whiteTexture.SetPixel(0, 0, Color.white);
        _whiteTexture.Apply();
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            _hovering = hit.collider.GetComponent<InteractableButton>()  != null
                     || hit.collider.GetComponent<PickupKey>()           != null
                     || hit.collider.GetComponent<WireButton>()          != null
                     || hit.collider.GetComponent<FuseBoxInteractable>() != null
                     || hit.collider.GetComponent<BreakerSwitch>()       != null;
        }
        else
        {
            _hovering = false;
        }
    }

    void OnGUI()
    {
        float cx = Screen.width  * 0.5f;
        float cy = Screen.height * 0.5f;

        if (!_hovering)
        {
            // Normal small filled dot
            float half = dotSize * 0.5f;
            GUI.DrawTexture(new Rect(cx - half, cy - half, dotSize, dotSize), _dotTexture);
        }
        else
        {
            // Outlined square - draw 4 thin border rects, no fill
            float s  = hoverSize;
            float t  = outlineThick;
            float x  = cx - s * 0.5f;
            float y  = cy - s * 0.5f;

            // Top edge
            GUI.DrawTexture(new Rect(x,         y,         s, t), _whiteTexture);
            // Bottom edge
            GUI.DrawTexture(new Rect(x,         y + s - t, s, t), _whiteTexture);
            // Left edge
            GUI.DrawTexture(new Rect(x,         y,         t, s), _whiteTexture);
            // Right edge
            GUI.DrawTexture(new Rect(x + s - t, y,         t, s), _whiteTexture);
        }
    }
}