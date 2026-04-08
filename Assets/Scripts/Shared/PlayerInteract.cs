using UnityEngine;

// Handles all player interactions - buttons, keys, wires, and the fuse box.
// Shoots a raycast forward from the camera when the player clicks
// and checks what they're looking at.
public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 2f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactRange))
            {
                // Check if it's a button
                InteractableButton button = hit.collider.GetComponent<InteractableButton>();
                if (button != null)
                {
                    GameObject player = transform.parent.gameObject;
                    if (button.CanPress(player))
                        button.Press();
                    else
                        button.DenyPress();
                }

                // Check if it's a key on the ground
                PickupKey key = hit.collider.GetComponent<PickupKey>();
                if (key != null)
                {
                    PlayerInventory inventory = transform.parent.GetComponent<PlayerInventory>();
                    if (inventory != null)
                    {
                        inventory.hasKey = true;
                        Debug.Log("Key picked up!");
                        Destroy(key.gameObject);
                    }
                }

                // Check if it's one of the Simon Says wires
                WireButton wire = hit.collider.GetComponent<WireButton>();
                if (wire != null)
                    wire.Press();

                // Check if it's the fuse box that starts the circuit game
                FuseBoxInteractable fuse = hit.collider.GetComponent<FuseBoxInteractable>();
                if (fuse != null)
                    fuse.Interact();
            }
        }
    }
}