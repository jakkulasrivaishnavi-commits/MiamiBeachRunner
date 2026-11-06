using UnityEngine;

/// <summary>Keeps the sunset sky quad centred in front of the camera.</summary>
public class SkyDome : MonoBehaviour
{
    public Transform cam;

    void LateUpdate()
    {
        if (cam == null) return;
        transform.position = new Vector3(cam.position.x, cam.position.y + 100f, cam.position.z + 500f);
        transform.rotation = Quaternion.identity;
    }
}
