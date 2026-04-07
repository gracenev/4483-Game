using UnityEngine;

// Controls all the surveillance cameras on Floor 2 at the same time.
// They all move together and share the same timing so it feels coordinated.
// Other scripts like RedLightEffect and SurveillanceDetector check the State here
// to know whether the player should be frozen or the lights should turn red.
public class CameraSync : MonoBehaviour
{
    [Header("Timing (randomised each cycle)")]
    public float minGreenDuration = 3f;   // shortest time cameras look away
    public float maxGreenDuration = 7f;   // longest time cameras look away
    public float minRedDuration   = 2f;   // shortest time cameras face the player
    public float maxRedDuration   = 5f;   // longest time cameras face the player

    [Tooltip("How long to wait after the scene loads before the cameras start moving")]
    public float startDelay = 4.5f;

    public enum CamState { FacingAway, TurningToPlayer, FacingPlayer, TurningAway }
    public CamState State { get; private set; } = CamState.FacingAway;

    private const float AWAY_ANGLE   =   0f;
    private const float PLAYER_ANGLE = 180f;

    private SurveillanceCamera[] _cams;
    private float _timer = 0f;

    void Start()
    {
        _cams = FindObjectsByType<SurveillanceCamera>(FindObjectsSortMode.None);
        if (_cams.Length == 0)
            Debug.LogWarning("CameraSync couldn't find any SurveillanceCamera objects in the scene.");

        // Give the player a moment to get their bearings before the cameras start
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
                // Pick a random amount of time to hold on the player
                _timer = Random.Range(minRedDuration, maxRedDuration);
                break;

            case CamState.TurningAway:
                SetAllTargets(AWAY_ANGLE);
                break;
        }
    }

    // Tell every camera to rotate to the same angle
    void SetAllTargets(float angle)
    {
        foreach (var cam in _cams)
            cam.SetTarget(angle);
    }

    // Check if all cameras have finished rotating before moving to the next state
    bool AllReached()
    {
        foreach (var cam in _cams)
            if (!cam.ReachedTarget) return false;
        return true;
    }
}