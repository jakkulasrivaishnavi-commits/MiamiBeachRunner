using System.Collections;
using UnityEngine;

/// <summary>Detects hits on trains, barriers and sign boards, then plays the crash sequence (PPT "CollisionDetect").</summary>
public class CollisionDetect : MonoBehaviour
{
    public float invincibleUntil;
    bool crashed;

    public void ResetCrash() { crashed = false; }

    void OnTriggerEnter(Collider other)
    {
        if (crashed || Time.time < invincibleUntil) return;
        if (other.GetComponentInParent<Obstacle>() == null) return;
        crashed = true;
        SaveLoad.saveData = true;
        StartCoroutine(CollisionEnd());
    }

    IEnumerator CollisionEnd()
    {
        AudioManager.Play(Sfx.Crash);
        PlayerMovement pm = GetComponent<PlayerMovement>();
        pm.Running = false;
        if (pm.model != null) pm.model.SetPose(RunnerModel.Pose.Stumble);
        if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.6f, 0.9f);
        Debris.Burst(transform.position + Vector3.up, GameConfig.HotPink, 22, 7f, 1.2f, 0.16f);
        Debris.Burst(transform.position + Vector3.up, GameConfig.Cyan, 14, 6f, 1.0f, 0.12f);
        if (GameplayController.Instance != null) GameplayController.Instance.FlashRed();
        yield return new WaitForSeconds(1.4f);
        if (GameplayController.Instance != null) GameplayController.Instance.ShowGameOver();
    }
}
