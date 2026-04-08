using UnityEngine;

// Handles the physical rotation of a single surveillance camera.
// CameraSync tells it where to rotate - this script just handles the smooth movement.
public class SurveillanceCamera : MonoBehaviour
{
    public float rotateSpeed = 400f;

    private float _targetAngle   = 0f;
    private bool  _reachedTarget = true;

    // CameraSync checks this to know when all cameras have finished rotating
    public bool ReachedTarget => _reachedTarget;

    // Called by CameraSync to give this camera a new angle to rotate to
    public void SetTarget(float angle)
    {
        _targetAngle   = angle;
        _reachedTarget = false;
    }

    void Update()
    {
        float current = transform.localEulerAngles.y;
        float delta   = Mathf.DeltaAngle(current, _targetAngle);

        // Close enough - snap to the target and mark as done
        if (Mathf.Abs(delta) < 0.5f)
        {
            transform.localEulerAngles = new Vector3(
                transform.localEulerAngles.x, _targetAngle, transform.localEulerAngles.z);
            _reachedTarget = true;
            return;
        }

        // Still rotating - move toward the target this frame
        _reachedTarget = false;
        transform.Rotate(0f, Mathf.Sign(delta) * rotateSpeed * Time.deltaTime, 0f, Space.Self);
    }
}