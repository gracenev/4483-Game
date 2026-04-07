using UnityEngine;

// Keeps track of what the player is carrying.
// Other scripts check hasKey to decide whether buttons can be pressed.
public class PlayerInventory : MonoBehaviour
{
    public bool hasKey = false;
}