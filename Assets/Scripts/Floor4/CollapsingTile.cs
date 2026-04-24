using System.Collections;
using UnityEngine;

// Attached to each floor/furniture piece on Floor 6.
// Detects the player standing on it using a CheckBox every frame instead of
// OnCollisionEnter (which is unreliable without a Rigidbody on both objects).
// When triggered it shakes, flashes red, then falls away.
public class CollapsingTile : MonoBehaviour
{
    [Tooltip("If true this piece never collapses")]
    public bool  isSafe        = false;
    [Tooltip("How long it shakes before falling")]
    public float shakeDelay    = 1.5f;
    [Tooltip("How fast it falls once it starts dropping")]
    public float collapseDelay = 0.5f;

    private bool     _triggered = false;
    private bool     _checking  = true;
    private Vector3  _startPos;
    private Renderer _rend;

    void Start()
    {
        _startPos = transform.position;
        _rend     = GetComponent<Renderer>();
    }

    void Update()
    {
        if (!_checking || _triggered || isSafe) return;

        // Use an overlap box just above the surface to detect the player standing on it
        Vector3 size    = transform.lossyScale;
        Vector3 centre  = transform.position + transform.up * (size.y * 0.5f + 0.15f);
        Vector3 halfExt = new Vector3(size.x * 0.45f, 0.2f, size.z * 0.45f);

        Collider[] hits = Physics.OverlapBox(centre, halfExt, transform.rotation);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                _triggered = true;
                _checking  = false;
                StartCoroutine(CollapseSequence());
                break;
            }
        }
    }

    IEnumerator CollapseSequence()
    {
        // Shake to warn the player
        float elapsed = 0f;
        while (elapsed < shakeDelay)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(elapsed * 40f) *
                          Mathf.Lerp(0.01f, 0.05f, elapsed / shakeDelay);
            transform.position = _startPos + new Vector3(
                UnityEngine.Random.Range(-shake, shake),
                0f,
                UnityEngine.Random.Range(-shake, shake));
            yield return null;
        }

        transform.position = _startPos;

        // Flash red just before falling
        if (_rend != null)
            _rend.material.color = new Color(0.8f, 0.1f, 0.1f);

        yield return new WaitForSeconds(0.1f);

        // Fall away - accelerates as it drops
        float fallTimer = 0f;
        Vector3 fallStart = transform.position;
        while (fallTimer < collapseDelay)
        {
            fallTimer += Time.deltaTime;
            float t = fallTimer / collapseDelay;
            transform.position = Vector3.Lerp(
                fallStart,
                fallStart + Vector3.down * 15f,
                t * t);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}