using UnityEngine;

/// <summary>
/// Automatic forward run, 3-lane switching, jump and slide (PPT "PlayerMovement").
/// Kinematic body: movement is scripted, Unity physics is used only for trigger events.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 11f;
    public float startSpeed = 11f;
    public float maxSpeed = 26f;
    public float laneDistance = 2.5f;
    public float jumpForce = 9f;
    public float gravity = 26f;
    public float slideTime = 0.75f;

    public RunnerModel model;
    public bool Running;

    int lane = 1;                // 0 left, 1 middle, 2 right
    float vy;
    bool grounded = true, sliding, slideOnLand;
    float slideTimer;
    CapsuleCollider col;

    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        col = GetComponent<CapsuleCollider>();
        Stand();
    }

    void Stand()
    {
        col.direction = 1; col.radius = 0.35f; col.height = 1.7f; col.center = new Vector3(0f, 0.9f, 0f);
    }

    void Duck()
    {
        col.direction = 1; col.radius = 0.35f; col.height = 0.8f; col.center = new Vector3(0f, 0.4f, 0f);
    }

    void StartSlide()
    {
        sliding = true;
        slideTimer = slideTime;
        Duck();
        if (model != null) model.SetPose(RunnerModel.Pose.Slide);
        AudioManager.Play(Sfx.Slide);
    }

    void EndSlide()
    {
        sliding = false;
        Stand();
        if (model != null && grounded) model.SetPose(RunnerModel.Pose.Run);
    }

    /// <summary>Called after a revive: stands the runner up and resumes.</summary>
    public void ResumeAfterRevive()
    {
        Vector3 p = transform.position;
        p.y = 0f;
        transform.position = p;
        vy = 0f; grounded = true; sliding = false; slideOnLand = false;
        Stand();
        if (model != null) model.SetPose(RunnerModel.Pose.Run);
        Running = true;
    }

    void Update()
    {
        if (!Running) return;
        float dt = Mathf.Min(Time.deltaTime, 0.033f);
        Vector3 p = transform.position;

        playerSpeed = Mathf.Min(maxSpeed, startSpeed + Mathf.Max(0f, p.z) * 0.0045f);
        p.z += playerSpeed * dt;

        if (GameInput.Left && lane > 0) { lane--; AudioManager.Play(Sfx.Lane); }
        if (GameInput.Right && lane < 2) { lane++; AudioManager.Play(Sfx.Lane); }

        if (GameInput.Jump && grounded)
        {
            if (sliding) EndSlide();
            vy = jumpForce;
            grounded = false;
            if (model != null) model.SetPose(RunnerModel.Pose.Jump);
            AudioManager.Play(Sfx.Jump);
        }
        if (GameInput.Slide)
        {
            if (!grounded) { vy = -jumpForce * 1.3f; slideOnLand = true; }
            else if (!sliding) StartSlide();
            else slideTimer = slideTime;
        }

        if (!grounded)
        {
            vy -= gravity * dt;
            p.y += vy * dt;
            if (p.y <= 0f)
            {
                p.y = 0f; vy = 0f; grounded = true;
                if (slideOnLand) { slideOnLand = false; StartSlide(); }
                else if (model != null) model.SetPose(RunnerModel.Pose.Run);
            }
        }

        if (sliding)
        {
            slideTimer -= dt;
            if (slideTimer <= 0f) EndSlide();
        }

        float tx = (lane - 1) * laneDistance;
        p.x = Mathf.Lerp(p.x, tx, 1f - Mathf.Exp(-14f * dt));

        transform.position = p;
        MasterLevelInfo.distanceRun = Mathf.Max(0, (int)p.z);
        if (model != null) model.animSpeed = playerSpeed / 11f;
    }
}
