using System.Collections;
using UnityEngine;

/// <summary>
/// Picks up the key using both trigger AND distance check as fallback.
/// Works even if the collider isn't set to IsTrigger or player tag is wrong.
/// </summary>
public class PickupKey : MonoBehaviour
{
    public float bobSpeed        = 2f;
    public float bobHeight       = 0.2f;
    public float activationDelay = 0.5f;

    [Tooltip("Fallback: auto-pickup if player gets within this distance (set 0 to disable)")]
    public float pickupRadius    = 1.5f;

    private Vector3    _startPos;
    private bool       _canPickup = false;
    private Transform  _player;

    void OnEnable()
    {
        _startPos  = transform.position;
        _canPickup = false;
        StartCoroutine(ActivateAfterDelay());
    }

    IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        _canPickup = true;

        // Cache player reference once active
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            _player = p.transform;
        else
            Debug.LogWarning("[PickupKey] No GameObject tagged 'Player' found!");
    }

    void Update()
    {
        if (!_canPickup) return;

        // Bob
        float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Distance fallback — works even without trigger collider
        if (pickupRadius > 0f && _player != null)
        {
            if (Vector3.Distance(transform.position, _player.position) <= pickupRadius)
                Collect(_player.gameObject);
        }
    }

    // Trigger still works if collider IS set up correctly
    void OnTriggerEnter(Collider other)
    {
        if (!_canPickup) return;
        if (other.CompareTag("Player"))
            Collect(other.gameObject);
    }

    void Collect(GameObject player)
    {
        if (!_canPickup) return;
        _canPickup = false;   // prevent double-fire

        PlayerInventory inv = player.GetComponent<PlayerInventory>();
        if (inv != null) inv.hasKey = true;

        LevelManager lm = FindAnyObjectByType<LevelManager>();
        if (lm != null)
            lm.OnKeyPickedUp();
        else
            Debug.LogWarning("[PickupKey] LevelManager not found in scene!");

        Debug.Log("[PickupKey] Key collected!");
        gameObject.SetActive(false);
    }
}