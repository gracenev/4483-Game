using UnityEngine;

/// <summary>
/// Single master controller — drives ALL SurveillanceCameras in sync.
/// One shared state machine with random timing.
/// RedLightEffect and SurveillanceDetector read State from this.
///
/// SETUP: Attach to any persistent GameObject. No other setup needed.
/// </summary>
public class CameraSync : MonoBehaviour
{
    [Header("Timing (randomised each cycle)")]
    public float minGreenDuration = 3f;
    public float maxGreenDuration = 7f;
    public float minRedDuration   = 2f;
    public float maxRedDuration   = 5f;

    // ── Shared state ───────────────────────────────────────────────────────
    public enum CamState { FacingAway, TurningToPlayer, FacingPlayer, TurningAway }
    public CamState State { get; private set; } = CamState.FacingAway;

    private const float AWAY_ANGLE   =   0f;
    private const float PLAYER_ANGLE = 180f;

    private SurveillanceCamera[] _cams;
    private float _timer = 0f;

    // ── Unity ──────────────────────────────────────────────────────────────
    [Tooltip("Seconds before cameras start moving after scene loads")]
    public float startDelay = 4.5f;

    void Start()
    {
        _cams = FindObjectsOfType<SurveillanceCamera>();
        if (_cams.Length == 0)
            Debug.LogWarning("[CameraSync] No SurveillanceCameras found!");

        // Hold cameras still for startDelay seconds then begin
        _timer = startDelay;
        State  = CamState.FacingAway;
    }

    void Update()
    {
        _timer -= Time.deltaTime;

        switch (State)
        {
            case CamState.FacingAway:
                if (_timer <= 0f)
                    EnterState(CamState.TurningToPlayer);
                break;

            case CamState.TurningToPlayer:
                if (AllReached())
                    EnterState(CamState.FacingPlayer);
                break;

            case CamState.FacingPlayer:
                if (_timer <= 0f)
                    EnterState(CamState.TurningAway);
                break;

            case CamState.TurningAway:
                if (AllReached())
                    EnterState(CamState.FacingAway);
                break;
        }
    }

    // ── State machine ──────────────────────────────────────────────────────
    void EnterState(CamState next)
    {
        State = next;

        switch (next)
        {
            case CamState.FacingAway:
                SetAllTargets(AWAY_ANGLE);
                _timer = Random.Range(minGreenDuration, maxGreenDuration);
                break;

            case CamState.TurningToPlayer:
                SetAllTargets(PLAYER_ANGLE);
                break;

            case CamState.FacingPlayer:
                _timer = Random.Range(minRedDuration, maxRedDuration);
                break;

            case CamState.TurningAway:
                SetAllTargets(AWAY_ANGLE);
                break;
        }
    }

    void SetAllTargets(float angle)
    {
        foreach (var cam in _cams)
            cam.SetTarget(angle);
    }

    bool AllReached()
    {
        foreach (var cam in _cams)
            if (!cam.ReachedTarget) return false;
        return true;
    }
}