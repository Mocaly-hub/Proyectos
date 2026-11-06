using UnityEngine;

public class HazardMover : MonoBehaviour
{
    public float distance = 1.35f;
    public float speed = 1.7f;
    private Vector3 origin;
    void Start() { origin = transform.position; }
    void Update() { transform.position = origin + Vector3.right * Mathf.Sin(Time.time * speed) * distance; }
}
