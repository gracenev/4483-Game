using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 3f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Right click
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactRange))
            {
                InteractableButton button = hit.collider.GetComponent<InteractableButton>();
                if (button != null)
                {
                    button.Press();
                }
            }
        }
    }
}