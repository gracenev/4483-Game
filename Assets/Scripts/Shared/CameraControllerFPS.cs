using UnityEngine;

// Handles the first person camera rotation based on mouse input.
// Attach this to the camera and make sure the player body is its parent.
public class CameraControllerFPS : MonoBehaviour
{
    public float Sensitivity = 2f;
    private float xRotation = 0f;

    void Start()
    {
        // Lock and hide the cursor so it doesn't show up during gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * Sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * Sensitivity;

        // Up and down rotation is applied to the camera
        xRotation -= mouseY;
        xRotation  = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Left and right rotation is applied to the player body
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}