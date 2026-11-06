using System.Collections;
using UnityEngine;

/// <summary>Coin pickup: ding, +1 coin, shrink-and-rise animation, then disappear (PPT "CollectCoin").</summary>
public class CoinCollection : MonoBehaviour
{
    [SerializeField] bool collectedCoin = false;

    void OnTriggerEnter(Collider other)
    {
        if (collectedCoin) return;
        if (other.GetComponent<PlayerMovement>() == null) return;
        collectedCoin = true;
        MasterLevelInfo.coinCount += 1;
        AudioManager.Play(Sfx.Coin);
        Debris.Burst(transform.position, GameConfig.Sun, 6, 3f, 0.45f, 0.08f);
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
