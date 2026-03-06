using UnityEngine;

public class InteractableButton : MonoBehaviour
{
    public float pressDistance = 0.05f;
    public float pressSpeed = 5f;
    public Color pressedColor = Color.black;

    private Vector3 originalPos;
    private Vector3 pressedPos;
    private Color originalColor;
    private Renderer rend;
    private bool isPressed = false;
    private bool returning = false;

    void Start()
    {
        originalPos = transform.localPosition;
        pressedPos = originalPos + (Vector3.right * pressDistance);
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    void Update()
    {
        if (isPressed)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, pressedPos, Time.deltaTime * pressSpeed);

            if (Vector3.Distance(transform.localPosition, pressedPos) < 0.001f)
            {
                isPressed = false;
                returning = true;
            }
        }
        else if (returning)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos, Time.deltaTime * pressSpeed);
            rend.material.color = Color.Lerp(rend.material.color, originalColor, Time.deltaTime * pressSpeed);

            if (Vector3.Distance(transform.localPosition, originalPos) < 0.001f)
            {
                transform.localPosition = originalPos;
                rend.material.color = originalColor;
                returning = false;
            }
        }
    }

    public void Press()
    {
        if (!isPressed && !returning)
        {
            isPressed = true;
            rend.material.color = pressedColor;
            Debug.Log("Button pressed!");
        }
    }
}