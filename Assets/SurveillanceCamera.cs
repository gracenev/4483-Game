using UnityEngine;

/// <summary>
/// Fully self-contained surveillance camera — Squid Game style.
/// Runs its own state machine. No other script needed at runtime.
/// CameraSystemSpawner just places this component; delete the spawner
/// after and this keeps working forever.
///
/// Cycle:
///   FACING AWAY  (green — safe)
///   → rotates 180° to face player
///   → FACING PLAYER  (red — freeze)
///   → rotates 180° back away
///   → repeat
/// </summary>
public class SurveillanceCamera : MonoBehaviour
{
    [Header("Rotation")]
    public float rotateSpeed   = 400f;   // degrees per second

    [Header("Timing")]
    public float greenDuration = 5f;     // seconds facing away (safe)
    public float redDuration   = 3f;     // seconds facing player (freeze)

    // ── State ──────────────────────────────────────────────────────────────
    public enum CamState { FacingAway, TurningToPlayer, FacingPlayer, TurningAway }
    public CamState State { get; private set; } = CamState.FacingAway;

    private const float AWAY_ANGLE   =   0f;
    private const float PLAYER_ANGLE = 180f;

    private float _targetAngle = AWAY_ANGLE;
    private float _stateTimer  = 0f;

    // ── Unity ──────────────────────────────────────────────────────────────
    void Start()
    {
        EnterState(CamState.FacingAway);
    }

    void Update()
    {
        RotateTick();
        StateTick();
    }

    // ── Rotation ───────────────────────────────────────────────────────────
    void RotateTick()
    {
        float current = transform.localEulerAngles.y;
        float delta   = Mathf.DeltaAngle(current, _targetAngle);

        if (Mathf.Abs(delta) < 0.5f)
        {
            // Snap and mark arrived
            transform.localEulerAngles = new Vector3(
                transform.localEulerAngles.x,
                _targetAngle,
                transform.localEulerAngles.z);
            OnReachedTarget();
            return;
        }

        transform.Rotate(0f, Mathf.Sign(delta) * rotateSpeed * Time.deltaTime, 0f, Space.Self);
    }

    void OnReachedTarget()
    {
        if (State == CamState.TurningToPlayer)
            EnterState(CamState.FacingPlayer);
        else if (State == CamState.TurningAway)
            EnterState(CamState.FacingAway);
    }

    // ── State machine ──────────────────────────────────────────────────────
    void StateTick()
    {
        _stateTimer -= Time.deltaTime;

        if (State == CamState.FacingAway && _stateTimer <= 0f)
            EnterState(CamState.TurningToPlayer);
        else if (State == CamState.FacingPlayer && _stateTimer <= 0f)
            EnterState(CamState.TurningAway);
    }

    void EnterState(CamState next)
    {
        State = next;

        switch (next)
        {
            case CamState.FacingAway:
                _targetAngle = AWAY_ANGLE;
                _stateTimer  = greenDuration;
                break;

            case CamState.TurningToPlayer:
                _targetAngle = PLAYER_ANGLE;
                break;

            case CamState.FacingPlayer:
                _targetAngle = PLAYER_ANGLE;
                _stateTimer  = redDuration;
                break;

            case CamState.TurningAway:
                _targetAngle = AWAY_ANGLE;
                break;
        }
    }
}