using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Full-screen black fade used for every scene transition (fade in on load, fade out before loading).</summary>
public class Fade : MonoBehaviour
{
    public static Fade Instance;
    CanvasGroup group;

    public static Fade Ensure()
    {
        if (Instance != null) return Instance;
        Canvas c = UIKit.CreateCanvas("FadeCanvas", 5000);
        Fade f = c.gameObject.AddComponent<Fade>();
        f.group = c.gameObject.AddComponent<CanvasGroup>();
        Image img = UIKit.Full(c.transform, Color.black);
        img.raycastTarget = true;
        f.group.alpha = 1f;
        f.group.blocksRaycasts = false;
        f.group.interactable = false;
        Instance = f;
        f.StartCoroutine(f.Anim(0f, 0.8f));
        return f;
    }

    IEnumerator Anim(float to, float dur)
    {
        float from = group.alpha;
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
            yield return null;
        }
        group.alpha = to;
    }

    IEnumerator LoadRoutine(string scene, float dur)
    {
        group.blocksRaycasts = true;
        yield return Anim(1f, dur);
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    /// <summary>Fade to black, then load the scene.</summary>
    public static void LoadScene(string scene, float duration = 0.8f)
    {
        Fade f = Ensure();
        f.StartCoroutine(f.LoadRoutine(scene, duration));
    }

    public static void FadeOutOnly(float duration)
    {
        Fade f = Ensure();
        f.StartCoroutine(f.Anim(1f, duration));
    }
}
