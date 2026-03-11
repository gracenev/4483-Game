using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerMovementCC : MonoBehaviour
{
    public float speed = 5f;
    public float jump = 4f;
    public float gravity = -9.81f;

    private CharacterController cc;
    private Vector3 velocity;

    private float groundBufferTime = 0.15f;
    private float groundBufferTimer = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    bool IsGrounded()
    {
        // CharacterController check + raycast backup for stationary grounding
        if (cc.isGrounded) return true;
        return Physics.Raycast(transform.position, Vector3.down, cc.height / 2f + 0.2f);
    }

    void Update()
    {
        Vector3 move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
        cc.Move(move * speed * Time.deltaTime);

        if (IsGrounded())
        {
            groundBufferTimer = groundBufferTime;
            if (velocity.y < 0)
                velocity.y = -2f;
        }
        else
        {
            groundBufferTimer -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && groundBufferTimer > 0f)
        {
            velocity.y = Mathf.Sqrt(jump * -2f * gravity);
            groundBufferTimer = 0f;
        }

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}