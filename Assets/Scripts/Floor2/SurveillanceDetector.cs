using UnityEngine;

// Watches the player during red light and triggers a fail if they move.
// Also freezes and unfreezes the player's movement script based on camera state.
public class SurveillanceDetector : MonoBehaviour
{
    [Header("References")]
    public GameObject         playerObject;
    public Floor2LevelManager levelManager;

    [Header("Detection")]
    [Tooltip("Minimum movement distance to count as actually moving")]
    public float movementThreshold = 0.3f;
    [Tooltip("Short window after red light starts before detection kicks in")]
    public float gracePeriod       = 0.5f;

    private CameraSync    _sync;
    private CharacterController _cc;
    private MonoBehaviour _playerMovement;
    private Vector3       _lastPosition;
    private bool          _redActive  = false;
    private bool          _failed     = false;
    private float         _graceTimer = 0f;

    void Start()
    {
        _sync = FindAnyObjectByType<CameraSync>();

        // Try to find the level manager automatically if it wasn't assigned
        if (levelManager == null)
            levelManager = FindAnyObjectByType<Floor2LevelManager>();
        if (levelManager == null)
            Debug.LogWarning("SurveillanceDetector couldn't find Floor2LevelManager in the scene.");

        if (playerObject != null)
        {
            _cc           = playerObject.GetComponent<CharacterController>();
            _lastPosition = playerObject.transform.position;

            // Try common names for the player movement script
            _playerMovement = playerObject.GetComponent("PlayerMovement") as MonoBehaviour;
            if (_playerMovement == null)
                _playerMovement = playerObject.GetComponent("PlayerMove") as MonoBehaviour;
        }
        else
            Debug.LogWarning("SurveillanceDetector: Player Object isn't assigned.");
    }

    void Update()
    {
        if (_failed || playerObject == null || _sync == null) return;

        bool isRed = _sync.State == CameraSync.CamState.FacingPlayer ||
                     _sync.State == CameraSync.CamState.TurningToPlayer;

        // Cameras just turned toward the player — freeze them and start the grace period
        if (isRed && !_redActive)
        {
            _redActive  = true;
            _graceTimer = gracePeriod;
            FreezePlayer(true);
        }
        // Cameras turned away — let the player move again
        else if (!isRed && _redActive)
        {
            _redActive = false;
            FreezePlayer(false);
        }

        if (_redActive)
        {
            // During the grace period just update the last position without checking
            if (_graceTimer > 0f)
            {
                _graceTimer   -= Time.deltaTime;
                _lastPosition  = playerObject.transform.position;
                return;
            }

            // Check if the player moved — if they did, they're caught
            float moved = Vector3.Distance(playerObject.transform.position, _lastPosition);
            if (moved > movementThreshold * Time.deltaTime)
            {
                Debug.Log($"Player caught moving! Movement distance: {moved}");
                _failed = true;
                FreezePlayer(true);

                if (levelManager != null)
                    levelManager.TriggerFail();
                else
                    Debug.LogError("SurveillanceDetector: Floor2LevelManager is null — make sure it's assigned.");
                return;
            }
        }

        _lastPosition = playerObject.transform.position;
    }

    void FreezePlayer(bool freeze)
    {
        if (_playerMovement != null)
            _playerMovement.enabled = !freeze;
    }
}