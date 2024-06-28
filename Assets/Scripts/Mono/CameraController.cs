using System.Collections;
using UnityEngine;
public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float smooth;
    private Vector3 velocity;
    public Vector2 xRange;
    public Vector2 yRange;
    void Update()
    {
        if (target != null)
        {
            Vector3 pos = Vector3.SmoothDamp(transform.position, target.position + offset, ref velocity, Time.deltaTime * smooth);
            SetPosition(pos);
        }
    }

    private void SetPosition(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, xRange.x, xRange.y);
        pos.y = Mathf.Clamp(pos.y, yRange.x, yRange.y);
        pos.z = -10;
        transform.position = pos;
    }
}