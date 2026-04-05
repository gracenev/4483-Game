using UnityEngine;

/// <summary>
/// Detects player movement during red light and calls TriggerFail()
/// on Floor2LevelManager.
/// </summary>
public class SurveillanceDetector : MonoBehaviour
{
    [Header("References")]
    public GameObject        playerObject;
    public Floor2LevelManager levelManager;

    [Header("Detection")]
    public float movementThreshold = 0.3f;
    public float gracePeriod       = 0.5f;

    private CameraSync          _sync;
    private CharacterController _cc;
    private MonoBehaviour       _playerMovement;
    private Vector3             _lastPosition;
    private bool                _redActive  = false;
    private bool                _failed     = false;
    private float               _graceTimer = 0f;

    void Start()
    {
        _sync = FindObjectOfType<CameraSync>();

        // Auto-find LevelManager if not assigned in Inspector
        if (levelManager == null)
            levelManager = FindObjectOfType<Floor2LevelManager>();
        if (levelManager == null)
            Debug.LogWarning("[SurveillanceDetector] Floor2LevelManager not found!");

        if (playerObject != null)
        {
            _cc           = playerObject.GetComponent<CharacterController>();
            _lastPosition = playerObject.transform.position;

            _playerMovement = playerObject.GetComponent("PlayerMovement") as MonoBehaviour;
            if (_playerMovement == null)
                _playerMovement = playerObject.GetComponent("PlayerMove") as MonoBehaviour;
        }
        else
            Debug.LogWarning("[SurveillanceDetector] Player Object not assigned!");
    }

    void Update()
    {
        if (_failed || playerObject == null || _sync == null) return;

        bool isRed = _sync.State == CameraSync.CamState.FacingPlayer ||
                     _sync.State == CameraSync.CamState.TurningToPlayer;

        // Enter red
        if (isRed && !_redActive)
        {
            _redActive  = true;
            _graceTimer = gracePeriod;
            FreezePlayer(true);
        }
        // Exit red
        else if (!isRed && _redActive)
        {
            _redActive = false;
            FreezePlayer(false);
        }

        if (_redActive)
        {
            if (_graceTimer > 0f)
            {
                _graceTimer   -= Time.deltaTime;
                _lastPosition  = playerObject.transform.position;
                return;
            }

            float moved = Vector3.Distance(playerObject.transform.position, _lastPosition);
            if (moved > movementThreshold * Time.deltaTime)
            {
                Debug.Log($"[SurveillanceDetector] Player caught moving! Distance: {moved}");
                _failed = true;
                FreezePlayer(true);

                if (levelManager != null)
                    levelManager.TriggerFail();
                else
                    Debug.LogError("[SurveillanceDetector] Floor2LevelManager is NULL — assign it in Inspector!");
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