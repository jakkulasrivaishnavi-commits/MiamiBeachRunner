using UnityEngine;

/// <summary>Chase camera behind the runner with speed-based FOV and a shake for crashes (PPT "Camera Follow").</summary>
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance;
    public Transform target;
    public PlayerMovement player;
    public float height = 3.7f, distance = -6.4f, lookAhead = 7f;

    Camera cam;
    float shakeT, shakeDur, shakeMag;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    public void Shake(float duration, float magnitude)
    {
        shakeT = shakeDur = duration;
        shakeMag = magnitude;
    }

    Vector3 Desired()
    {
        return new Vector3(target.position.x * 0.6f, height + target.position.y * 0.4f, target.position.z + distance);
    }

    public void Snap()
    {
        if (target == null) return;
        transform.position = Desired();
        Aim();
    }

    void Aim()
    {
        transform.LookAt(new Vector3(target.position.x * 0.35f, 1.6f + target.position.y * 0.3f, target.position.z + lookAhead));
    }

    void LateUpdate()
    {
        if (target == null) return;
        float dt = Time.unscaledDeltaTime;
        Vector3 d = Desired();
        Vector3 p = transform.position;
        p.x = Mathf.Lerp(p.x, d.x, 1f - Mathf.Exp(-8f * dt));
        p.y = Mathf.Lerp(p.y, d.y, 1f - Mathf.Exp(-6f * dt));
        p.z = d.z;
        transform.position = p;
        Aim();

        if (shakeT > 0f)
        {
            shakeT -= dt;
            transform.position += Random.insideUnitSphere * (shakeT / shakeDur) * shakeMag * 0.4f;
        }
        if (cam != null && player != null)
        {
            float k = Mathf.InverseLerp(player.startSpeed, player.maxSpeed, player.playerSpeed);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 62f + k * 12f, 3f * dt);
        }
    }
}
