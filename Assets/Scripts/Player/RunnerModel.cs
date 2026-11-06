using UnityEngine;

/// <summary>Procedural 3D surfer character (cap, shades, Hawaiian shirt, surfboard on his back) with code-driven animation.</summary>
public class RunnerModel : MonoBehaviour
{
    public enum Pose { Idle, Run, Jump, Slide, Stumble }
    public Pose pose = Pose.Idle;
    public float animSpeed = 1f;

    Transform body, armL, armR, legL, legR;
    Renderer[] rends;
    float t, poseTime;

    public static RunnerModel Create(Transform parent)
    {
        GameObject go = new GameObject("RunnerModel");
        go.transform.SetParent(parent, false);
        RunnerModel m = go.AddComponent<RunnerModel>();
        m.Build();
        return m;
    }

    void Build()
    {
        Material skin = Mats.Solid(GameConfig.RGB(240, 185, 140), 0.2f);
        Material shirt = Mats.Solid(GameConfig.RGB(255, 70, 150), 0.25f);
        Material shirt2 = Mats.Solid(GameConfig.RGB(255, 220, 60), 0.25f);
        Material shorts = Mats.Solid(GameConfig.RGB(20, 200, 200), 0.25f);
        Material white = Mats.Solid(GameConfig.RGB(250, 250, 250), 0.3f);
        Material dark = Mats.Solid(GameConfig.RGB(15, 15, 25), 0.85f);
        Material boardM = Mats.Solid(GameConfig.RGB(0, 210, 240), 0.6f);

        body = Prim.Pivot("Body", transform, new Vector3(0f, 0.95f, 0f));

        legL = Prim.Pivot("LegL", body, new Vector3(-0.13f, 0f, 0f));
        legR = Prim.Pivot("LegR", body, new Vector3(0.13f, 0f, 0f));
        foreach (Transform leg in new[] { legL, legR })
        {
            Prim.Box(leg, new Vector3(0f, -0.42f, 0f), new Vector3(0.17f, 0.84f, 0.17f), skin, true);
            Prim.Box(leg, new Vector3(0f, -0.86f, 0.05f), new Vector3(0.2f, 0.12f, 0.32f), white, true);
        }

        Prim.Box(body, new Vector3(0f, -0.02f, 0f), new Vector3(0.5f, 0.3f, 0.3f), shorts, true);
        Prim.Box(body, new Vector3(0f, 0.42f, 0f), new Vector3(0.52f, 0.6f, 0.3f), shirt, true);
        Prim.Box(body, new Vector3(0f, 0.30f, 0f), new Vector3(0.54f, 0.07f, 0.32f), shirt2);
        Prim.Box(body, new Vector3(0f, 0.54f, 0f), new Vector3(0.54f, 0.07f, 0.32f), shirt2);

        armL = Prim.Pivot("ArmL", body, new Vector3(-0.35f, 0.66f, 0f));
        armR = Prim.Pivot("ArmR", body, new Vector3(0.35f, 0.66f, 0f));
        foreach (Transform arm in new[] { armL, armR })
        {
            Prim.Box(arm, new Vector3(0f, -0.26f, 0f), new Vector3(0.13f, 0.52f, 0.13f), skin, true);
            Prim.Box(arm, new Vector3(0f, -0.04f, 0f), new Vector3(0.17f, 0.2f, 0.17f), shirt, true);
        }

        Transform head = Prim.Pivot("Head", body, new Vector3(0f, 0.9f, 0f));
        Prim.Sphere(head, Vector3.zero, 0.36f, skin, true);
        Prim.Box(head, new Vector3(0f, 0.13f, -0.01f), new Vector3(0.38f, 0.14f, 0.4f), white, true);
        Prim.Box(head, new Vector3(0f, 0.07f, 0.24f), new Vector3(0.3f, 0.03f, 0.2f), white);
        Prim.Box(head, new Vector3(0f, 0.02f, 0.18f), new Vector3(0.32f, 0.08f, 0.05f), dark);

        Transform board = Prim.Pivot("Board", body, new Vector3(0f, 0.4f, -0.22f));
        board.localRotation = Quaternion.Euler(0f, 0f, 12f);
        Prim.Box(board, Vector3.zero, new Vector3(0.4f, 1.3f, 0.06f), boardM, true);
        Prim.Box(board, new Vector3(0f, 0.1f, 0f), new Vector3(0.42f, 0.1f, 0.07f), shirt2);

        rends = GetComponentsInChildren<Renderer>();
    }

    public void SetPose(Pose p)
    {
        if (pose == p) return;
        pose = p;
        poseTime = 0f;
    }

    public void SetVisible(bool v)
    {
        if (rends == null) return;
        foreach (Renderer r in rends) if (r != null) r.enabled = v;
    }

    void Update()
    {
        t += Time.deltaTime * animSpeed;
        poseTime += Time.deltaTime;
        if (body == null) return;

        switch (pose)
        {
            case Pose.Run:
                {
                    float s = Mathf.Sin(t * 13f);
                    body.localPosition = new Vector3(0f, 0.95f + Mathf.Abs(s) * 0.06f, 0f);
                    body.localRotation = Quaternion.Euler(8f, 0f, 0f);
                    armL.localRotation = Quaternion.Euler(s * 50f, 0f, 0f);
                    armR.localRotation = Quaternion.Euler(-s * 50f, 0f, 0f);
                    legL.localRotation = Quaternion.Euler(-s * 55f, 0f, 0f);
                    legR.localRotation = Quaternion.Euler(s * 55f, 0f, 0f);
                    break;
                }
            case Pose.Jump:
                body.localPosition = new Vector3(0f, 0.95f, 0f);
                body.localRotation = Quaternion.identity;
                armL.localRotation = Quaternion.Euler(-150f, 0f, 12f);
                armR.localRotation = Quaternion.Euler(-150f, 0f, -12f);
                legL.localRotation = Quaternion.Euler(-55f, 0f, 0f);
                legR.localRotation = Quaternion.Euler(25f, 0f, 0f);
                break;
            case Pose.Slide:
                body.localPosition = new Vector3(0f, 0.42f, 0f);
                body.localRotation = Quaternion.Euler(-78f, 0f, 0f);
                armL.localRotation = Quaternion.Euler(-60f, 0f, 20f);
                armR.localRotation = Quaternion.Euler(-60f, 0f, -20f);
                legL.localRotation = Quaternion.identity;
                legR.localRotation = Quaternion.identity;
                break;
            case Pose.Stumble:
                {
                    float p = Mathf.Clamp01(poseTime / 0.6f);
                    body.localPosition = new Vector3(0f, Mathf.Lerp(0.95f, 0.3f, p), 0f);
                    body.localRotation = Quaternion.Euler(Mathf.Lerp(8f, -95f, p), 0f, 0f);
                    float flail = Mathf.Sin(poseTime * 25f) * 70f * (1f - p);
                    armL.localRotation = Quaternion.Euler(-100f + flail, 0f, 20f);
                    armR.localRotation = Quaternion.Euler(-100f - flail, 0f, -20f);
                    legL.localRotation = Quaternion.Euler(30f * p, 0f, 0f);
                    legR.localRotation = Quaternion.Euler(-20f * p, 0f, 0f);
                    break;
                }
            default: // Idle
                {
                    float b = Mathf.Sin(t * 2.5f) * 0.015f;
                    body.localPosition = new Vector3(0f, 0.95f + b, 0f);
                    body.localRotation = Quaternion.identity;
                    armL.localRotation = Quaternion.Euler(5f, 0f, 4f);
                    armR.localRotation = Quaternion.Euler(5f, 0f, -4f);
                    legL.localRotation = Quaternion.identity;
                    legR.localRotation = Quaternion.identity;
                    break;
                }
        }
    }
}
