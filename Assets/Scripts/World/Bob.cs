using UnityEngine;

/// <summary>Gentle bobbing / rocking for boats and flags.</summary>
public class Bob : MonoBehaviour
{
    public float amplitude = 0.12f, speed = 1.2f, roll = 3f;
    Vector3 basePos;
    float phase;

    void Start()
    {
        basePos = transform.localPosition;
        phase = Random.value * 6.28f;
    }

    void Update()
    {
        float s = Mathf.Sin(Time.time * speed + phase);
        transform.localPosition = basePos + Vector3.up * s * amplitude;
        transform.localRotation = Quaternion.Euler(0f, transform.localEulerAngles.y, s * roll);
    }
}
