using UnityEngine;

public class CameraChase : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 8.5f, -12.5f);
    public float followSmoothness = 6f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSmoothness * Time.deltaTime));
        transform.LookAt(target.position + Vector3.up * 0.6f);
    }
}
