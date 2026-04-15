using UnityEngine;

// Handles player movement using a CharacterController.
// Jumping works whether standing still or moving.
// Slopes are handled properly using a SphereCast instead of a single raycast.
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

    // SphereCast covers slopes and stationary standing better than a single raycast
    bool IsGrounded()
    {
        if (cc.isGrounded) return true;

        // SphereCast downward from the base of the controller
        // Radius slightly smaller than the CC radius to avoid false positives on walls
        float castRadius = cc.radius * 0.9f;
        float castDist   = (cc.height * 0.5f) - cc.radius + 0.25f;

        return Physics.SphereCast(
            transform.position,
            castRadius,
            Vector3.down,
            out _,
            castDist);
    }

    void Update()
    {
        // Move the player based on WASD / arrow key input
        Vector3 move = transform.right   * Input.GetAxis("Horizontal")
                     + transform.forward * Input.GetAxis("Vertical");

        // Always push slightly downward so cc.isGrounded works even when standing still
        Vector3 moveWithGrounding = move * speed + Vector3.down * 2f;
        cc.Move(moveWithGrounding * Time.deltaTime);

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

        // Jump works whether moving or standing still
        if (Input.GetButtonDown("Jump") && groundBufferTimer > 0f)
        {
            velocity.y        = Mathf.Sqrt(jump * -2f * gravity);
            groundBufferTimer = 0f;
        }

        // Apply gravity every frame
        velocity.y += gravity * Time.deltaTime;
        cc.Move(new Vector3(0f, velocity.y, 0f) * Time.deltaTime);
    }
}