using System.Collections;
using UnityEngine;

// Handles picking up the key when the player gets close enough.
// Uses both a trigger and a distance check so it works even if the collider setup isn't perfect.
public class PickupKey : MonoBehaviour
{
    public float bobSpeed        = 2f;
    public float bobHeight       = 0.2f;
    public float activationDelay = 0.5f;

    [Tooltip("How close the player needs to be to auto-pick up the key (set to 0 to disable)")]
    public float pickupRadius    = 1.5f;

    private Vector3   _startPos;
    private bool      _canPickup = false;
    private Transform _player;

    void OnEnable()
    {
        _startPos  = transform.position;
        _canPickup = false;
        StartCoroutine(ActivateAfterDelay());
    }

    // Short delay before the key becomes pickable so it doesn't get collected the instant it spawns
    IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        _canPickup = true;

        // Find and cache the player so we're not searching every frame
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            _player = p.transform;
        else
            Debug.LogWarning("PickupKey couldn't find a GameObject tagged 'Player'.");
    }

    void Update()
    {
        if (!_canPickup) return;

        // Make the key bob up and down while waiting to be picked up
        float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // If the player gets close enough, pick it up even without a trigger
        if (pickupRadius > 0f && _player != null)
        {
            if (Vector3.Distance(transform.position, _player.position) <= pickupRadius)
                Collect(_player.gameObject);
        }
    }

    // Also works through the trigger if the collider is set up as a trigger
    void OnTriggerEnter(Collider other)
    {
        if (!_canPickup) return;
        if (other.CompareTag("Player"))
            Collect(other.gameObject);
    }

    void Collect(GameObject player)
    {
        if (!_canPickup) return;
        _canPickup = false;   // make sure this only runs once

        PlayerInventory inv = player.GetComponent<PlayerInventory>();
        if (inv != null) inv.hasKey = true;

        LevelManager lm = FindAnyObjectByType<LevelManager>();
        if (lm != null)
            lm.OnKeyPickedUp();
        else
            Debug.LogWarning("PickupKey couldn't find the LevelManager in the scene.");

        Debug.Log("Key picked up!");
        gameObject.SetActive(false);
    }
}