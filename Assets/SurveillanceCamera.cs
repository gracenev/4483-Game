using UnityEngine;

/// <summary>
/// Pure rotation component. CameraSync drives the state —
/// this just smoothly rotates to whatever angle it's told.
/// </summary>
public class SurveillanceCamera : MonoBehaviour
{
    public float rotateSpeed = 400f;

    private float _targetAngle  = 0f;
    private bool  _reachedTarget = true;
    public  bool  ReachedTarget  => _reachedTarget;

    public void SetTarget(float angle)
    {
        _targetAngle  = angle;
        _reachedTarget = false;
    }

    void Update()
    {
        float current = transform.localEulerAngles.y;
        float delta   = Mathf.DeltaAngle(current, _targetAngle);

        if (Mathf.Abs(delta) < 0.5f)
        {
            transform.localEulerAngles = new Vector3(
                transform.localEulerAngles.x, _targetAngle, transform.localEulerAngles.z);
            _reachedTarget = true;
            return;
        }

        _reachedTarget = false;
        transform.Rotate(0f, Mathf.Sign(delta) * rotateSpeed * Time.deltaTime, 0f, Space.Self);
    }
}