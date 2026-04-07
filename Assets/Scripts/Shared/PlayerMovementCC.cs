using UnityEngine;

// Handles player movement using a CharacterController.
// Includes a small ground buffer window so jumping still works
// even if the player steps off a ledge a fraction of a second early.
public class PlayerMovementCC : MonoBehaviour
{
    public float speed   = 5f;
    public float jump    = 4f;
    public float gravity = -9.81f;

    private CharacterController cc;
    private Vector3 velocity;

    // Short window after leaving the ground where jumping is still allowed
    private float groundBufferTime  = 0.15f;
    private float groundBufferTimer = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Uses both the CharacterController's built-in check and a raycast as a backup
    bool IsGrounded()
    {
        if (cc.isGrounded) return true;
        return Physics.Raycast(transform.position, Vector3.down, cc.height / 2f + 0.2f);
    }

    void Update()
    {
        // Move the player based on WASD / arrow key input
        Vector3 move = transform.right   * Input.GetAxis("Horizontal")
                     + transform.forward * Input.GetAxis("Vertical");
        cc.Move(move * speed * Time.deltaTime);

        if (IsGrounded())
        {
            groundBufferTimer = groundBufferTime;

            // Reset downward velocity so the player doesn't build up speed on the ground
            if (velocity.y < 0)
                velocity.y = -2f;
        }
        else
        {
            groundBufferTimer -= Time.deltaTime;
        }

        // Allow jumping within the buffer window
        if (Input.GetButtonDown("Jump") && groundBufferTimer > 0f)
        {
            velocity.y        = Mathf.Sqrt(jump * -2f * gravity);
            groundBufferTimer = 0f;
        }

        // Apply gravity every frame
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}