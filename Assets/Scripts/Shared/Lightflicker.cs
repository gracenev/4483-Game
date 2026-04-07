using UnityEngine;

// Makes a ceiling light flicker like a failing fluorescent tube.
// There are two layers to the effect - a slow gentle drift and
// occasional rapid on/off bursts to sell the broken tube feel.
// Attach to any GameObject that has a Light component.
[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [Header("Baseline")]
    [Tooltip("Normal brightness when the light is fully on")]
    public float baseIntensity = 1.2f;
    [Tooltip("How dim it gets at its lowest during the slow drift")]
    public float minIntensity  = 0.6f;

    [Header("Slow Drift")]
    [Tooltip("How fast the slow drift moves - lower feels lazier")]
    public float driftSpeed    = 0.8f;
    [Tooltip("How much the drift affects the brightness")]
    [Range(0f, 1f)]
    public float driftStrength = 0.3f;

    [Header("Stutter Burst")]
    [Tooltip("Roughly how often the rapid flicker bursts happen in seconds")]
    public float stutterInterval = 4f;
    [Tooltip("Random amount added or subtracted from the interval so it doesn't feel predictable")]
    public float stutterVariance = 2f;
    [Tooltip("How many on/off flips happen in a single burst")]
    public int   stutterFlips    = 6;
    [Tooltip("Time between each flip during a burst")]
    public float stutterFlipTime = 0.04f;

    private Light _light;
    private float _noiseOffset;       // randomised so every light flickers differently
    private float _nextStutterTime;
    private bool  _stuttering;
    private int   _stutterFlipsLeft;
    private float _nextFlipTime;
    private bool  _stutterPhase;      // tracks whether we're in the on or off part of a flip

    void Awake()
    {
        _light       = GetComponent<Light>();
        _noiseOffset = Random.Range(0f, 100f);
        ScheduleNextStutter();
    }

    void Update()
    {
        if (_stuttering)
            UpdateStutter();
        else
        {
            UpdateDrift();
            if (Time.time >= _nextStutterTime)
                BeginStutter();
        }
    }

    // Slowly varies the brightness using Perlin noise
    void UpdateDrift()
    {
        float noise  = Mathf.PerlinNoise(_noiseOffset + Time.time * driftSpeed, 0f);
        float target = Mathf.Lerp(minIntensity, baseIntensity,
                                  1f - driftStrength + noise * driftStrength);
        _light.intensity = target;
    }

    // Kicks off a rapid flicker burst
    void BeginStutter()
    {
        _stuttering       = true;
        _stutterFlipsLeft = stutterFlips + Random.Range(-2, 3);
        _stutterPhase     = false;
        _nextFlipTime     = Time.time;
    }

    // Alternates the light on and off rapidly during a burst
    void UpdateStutter()
    {
        if (Time.time < _nextFlipTime) return;

        _stutterPhase    = !_stutterPhase;
        _light.intensity = _stutterPhase ? baseIntensity : 0f;
        _nextFlipTime    = Time.time + stutterFlipTime + Random.Range(-0.01f, 0.01f);
        _stutterFlipsLeft--;

        if (_stutterFlipsLeft <= 0)
        {
            // Burst is done - go back to normal and schedule the next one
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

    // Call this if you want to lock the light on permanently
    public void SetSolid()
    {
        enabled          = false;
        _light.intensity = baseIntensity;
    }

    // Call this to turn the light off completely
    public void TurnOff()
    {
        enabled          = false;
        _light.intensity = 0f;
    }
}