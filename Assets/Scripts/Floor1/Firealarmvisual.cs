using UnityEngine;

// Attach this to the fire alarm object to make it spin and flash red.
// Tweak the spin speed and flash rate in the Inspector to get the right feel.
public class FireAlarmVisual : MonoBehaviour
{
    [Header("Spin")]
    public float spinSpeed = 180f;          // degrees per second

    [Header("Flash")]
    public Color colorOn  = new Color(5f, 0.1f, 0.1f);     // bright overbright red for the glow
    public Color colorOff = new Color(0.15f, 0f, 0f);       // dim red when off
    public float flashRate = 3f;            // how many times it flashes per second

    private Renderer _rend;
    private float    _timer;
    private bool     _on;

    void Awake()
    {
        _rend = GetComponent<Renderer>();
    }

    void Update()
    {
        // Keep spinning every frame
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);

        // Alternate between the bright and dim colour based on the flash rate
        _timer += Time.deltaTime;
        float halfPeriod = 0.5f / flashRate;
        if (_timer >= halfPeriod)
        {
            _timer -= halfPeriod;
            _on = !_on;
            if (_rend != null)
                _rend.material.color = _on ? colorOn : colorOff;
        }
    }
}