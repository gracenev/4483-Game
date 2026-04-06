using UnityEngine;

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
                // ── Interactable Button ────────────────────────────────────
                InteractableButton button = hit.collider.GetComponent<InteractableButton>();
                if (button != null)
                {
                    GameObject player = transform.parent.gameObject;
                    if (button.CanPress(player))
                        button.Press();
                    else
                        button.DenyPress();
                }

                // ── Key pickup ─────────────────────────────────────────────
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

                // ── Wire (Simon Says) ──────────────────────────────────────
                WireButton wire = hit.collider.GetComponent<WireButton>();
                if (wire != null)
                    wire.Press();

                // ── Fuse Box (starts Simon Says game) ─────────────────────
                FuseBoxInteractable fuse = hit.collider.GetComponent<FuseBoxInteractable>();
                if (fuse != null)
                    fuse.Interact();
            }
        }
    }
}