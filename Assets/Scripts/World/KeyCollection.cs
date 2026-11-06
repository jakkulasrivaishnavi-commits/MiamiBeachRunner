using System.Collections;
using UnityEngine;

/// <summary>Key pickup: +1 key (used to revive after a crash), shrink-and-rise animation (PPT "KeyCollect").</summary>
public class KeyCollection : MonoBehaviour
{
    [SerializeField] bool collectedKey = false;

    void OnTriggerEnter(Collider other)
    {
        if (collectedKey) return;
        if (other.GetComponent<PlayerMovement>() == null) return;
        collectedKey = true;
        MasterLevelInfo.keyCount += 1;
        AudioManager.Play(Sfx.Key);
        Debris.Burst(transform.position, GameConfig.Cyan, 12, 4f, 0.6f, 0.1f);
        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = false;
        StartCoroutine(ShrinkAndRise());
    }

    IEnumerator ShrinkAndRise()
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            transform.Translate(Vector3.up * Time.deltaTime * 4f, Space.World);
            transform.localScale = Vector3.Lerp(start, Vector3.zero, t / 0.5f);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
