using UnityEngine;

public class PickupKey : MonoBehaviour
{
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
        transform.Rotate(Vector3.up * 50f * Time.deltaTime);
    }
}