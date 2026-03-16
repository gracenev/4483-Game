using UnityEngine;

/// <summary>
/// Attach to a GameObject that has a Light component to make it flicker
/// like a failing fluorescent tube.
///
/// The effect has two layers:
///   1. Slow drift — a Perlin-noise sway in intensity over several seconds
///   2. Fast stutter — occasional rapid on/off bursts that simulate tube failure
///
/// All parameters are tunable from the Inspector.
/// </summary>
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [Header("Baseline")]
    [Tooltip("Normal intensity when the light is fully on")]
    public float baseIntensity  = 1.2f;
    [Tooltip("Minimum intensity during slow drift (never fully off)")]
    public float minIntensity   = 0.6f;

    [Header("Slow Noise Drift")]
    [Tooltip("How fast the Perlin noise crawls (lower = lazier flicker)")]
    public float driftSpeed     = 0.8f;
    [Tooltip("How much the slow drift affects intensity (0 = no drift)")]
    [Range(0f, 1f)]
    public float driftStrength  = 0.3f;

    [Header("Stutter Burst")]
    [Tooltip("Average seconds between stutter bursts")]
    public float stutterInterval    = 4f;
    [Tooltip("Random variance added to stutterInterval")]
    public float stutterVariance    = 2f;
    [Tooltip("How many rapid flips happen in one burst")]
    public int   stutterFlips       = 6;
    [Tooltip("Seconds between each flip during a burst")]
    public float stutterFlipTime    = 0.04f;

    // -----------------------------------------------------------------------
    private Light  _light;
    private float  _noiseOffset;       // unique offset so rooms don't sync
    private float  _nextStutterTime;
    private bool   _stuttering;
    private int    _stutterFlipsLeft;
    private float  _nextFlipTime;
    private bool   _stutterPhase;      // alternates on/off during burst

    void Awake()
    {
        _light       = GetComponent<Light>();
        _noiseOffset = Random.Range(0f, 100f);
        ScheduleNextStutter();
    }

    void Update()
    {
        if (_stuttering)
        {
            UpdateStutter();
        }
        else
        {
            UpdateDrift();
            if (Time.time >= _nextStutterTime)
                BeginStutter();
        }
    }

    // ── Slow Perlin drift ───────────────────────────────────────────────────
    void UpdateDrift()
    {
        float noise  = Mathf.PerlinNoise(_noiseOffset + Time.time * driftSpeed, 0f);
        // Remap [0,1] → [minIntensity, baseIntensity]
        float target = Mathf.Lerp(minIntensity, baseIntensity,
                                  1f - driftStrength + noise * driftStrength);
        _light.intensity = target;
    }

    // ── Stutter burst ───────────────────────────────────────────────────────
    void BeginStutter()
    {
        _stuttering      = true;
        _stutterFlipsLeft = stutterFlips + Random.Range(-2, 3);
        _stutterPhase    = false;
        _nextFlipTime    = Time.time;
    }

    void UpdateStutter()
    {
        if (Time.time < _nextFlipTime) return;

        _stutterPhase         = !_stutterPhase;
        _light.intensity      = _stutterPhase ? baseIntensity : 0f;
        _nextFlipTime         = Time.time + stutterFlipTime + Random.Range(-0.01f, 0.01f);
        _stutterFlipsLeft--;

        if (_stutterFlipsLeft <= 0)
        {
            // End burst — restore normal intensity
            _stuttering      = false;
            _light.intensity = baseIntensity;
            ScheduleNextStutter();
        }
    }

    void ScheduleNextStutter()
    {
        _nextStutterTime = Time.time
                         + stutterInterval
                         + Random.Range(-stutterVariance, stutterVariance);
    }

    // ── Public helpers ──────────────────────────────────────────────────────

    /// <summary>Force the light to stay solid on (disables flicker).</summary>
    public void SetSolid()
    {
        enabled          = false;
        _light.intensity = baseIntensity;
    }

    /// <summary>Force the light fully off immediately.</summary>
    public void TurnOff()
    {
        enabled          = false;
        _light.intensity = 0f;
    }
}