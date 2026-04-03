using UnityEngine;
public class FireAlarmVisual : MonoBehaviour
{
    [Header("Spin")]
    public float spinSpeed = 180f;          // degrees per second

    [Header("Flash")]
    public Color colorOn  = new Color(5f, 0.1f, 0.1f);     // HDR overbright red
    public Color colorOff = new Color(0.15f, 0f, 0f);       // dark red
    public float flashRate = 3f;            // flashes per second

    private Renderer _rend;
    private float    _timer;
    private bool     _on;

    void Awake()
    {
        _rend = GetComponent<Renderer>();
    }

    void Update()
    {
        // Spin
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);

        // Flash
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