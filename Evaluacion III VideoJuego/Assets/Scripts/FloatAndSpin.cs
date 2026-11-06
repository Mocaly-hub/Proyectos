using UnityEngine;

public class FloatAndSpin : MonoBehaviour
{
    public float spinSpeed = 100f;
    public float bobAmplitude = 0.20f;
    public float bobFrequency = 2.2f;
    private Vector3 origin;
    private float phase;

    void Start()
    {
        origin = transform.position;
        phase = transform.position.z * 0.65f;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        transform.position = origin + Vector3.up * (Mathf.Sin(Time.time * bobFrequency + phase) * bobAmplitude);
    }
}
