using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public float dotSize = 4f;
    public float hoverDotSize = 10f;
    public float interactRange = 2f;
    private Texture2D dotTexture;
    private bool hovering = false;

    void Start()
    {
        dotTexture = new Texture2D(1, 1);
        dotTexture.SetPixel(0, 0, Color.white);
        dotTexture.Apply();
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
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
        float size = hovering ? hoverDotSize : dotSize;
        float x = Screen.width / 2f - size / 2f;
        float y = Screen.height / 2f - size / 2f;
        GUI.DrawTexture(new Rect(x, y, size, size), dotTexture);
    }
}