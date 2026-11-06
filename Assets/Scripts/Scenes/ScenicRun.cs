using UnityEngine;

/// <summary>Cinematic background for the menu scenes: the surfer jogs down the boardwalk while the camera sways behind.</summary>
public class ScenicRun : MonoBehaviour
{
    public float speed = 8f;
    public Camera cam;
    RunnerModel model;
    float t;

    public static ScenicRun Create(Camera cam)
    {
        GameObject go = new GameObject("ScenicRunner");
        ScenicRun s = go.AddComponent<ScenicRun>();
        s.cam = cam;
        s.model = RunnerModel.Create(go.transform);
        s.model.SetPose(RunnerModel.Pose.Run);
        s.model.animSpeed = 0.8f;
        GameObject g = new GameObject("SegmentGenerator");
        SegmentGenerator gen = g.AddComponent<SegmentGenerator>();
        gen.target = go.transform;
        gen.withObstacles = false;
        return s;
    }

    void Update()
    {
        t += Time.deltaTime;
        Vector3 p = transform.position;
        p.z += speed * Time.deltaTime;
        p.x = Mathf.Lerp(p.x, Mathf.Sin(t * 0.6f) * GameConfig.LaneDistance, 3f * Time.deltaTime);
        transform.position = p;
        if (cam == null) return;
        float ang = Mathf.Sin(t * 0.3f) * 40f;
        Vector3 off = Quaternion.Euler(0f, ang, 0f) * new Vector3(0f, 1.7f, -5.4f);
        cam.transform.position = p + off;
        cam.transform.LookAt(p + new Vector3(0f, 1.3f, 3f));
    }
}
