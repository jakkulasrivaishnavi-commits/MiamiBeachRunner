using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Scene 3 - Information. Shows the controls, then fades into Gameplay automatically (PPT "LoadToStage").</summary>
public class LoadToStage : MonoBehaviour
{
    Text loadingText;
    RectTransform barFill;
    const float BarW = 520f, BarH = 24f;
    float elapsed;
    bool loading;

    void Awake()
    {
        Time.timeScale = 1f;
        AudioManager.Ensure();
        Fade.Ensure();
        Camera cam = WorldSetup.GetCamera();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = GameConfig.Navy;
        BuildUi();
    }

    void BuildUi()
    {
        Canvas c = UIKit.CreateCanvas("InfoCanvas", 10);
        UIKit.EnsureEventSystem();
        Transform ui = c.transform;

        Image bg = UIKit.Full(ui, Color.white);
        bg.sprite = UIKit.GradientSprite(new Color(0.12f, 0.04f, 0.32f), new Color(0.75f, 0.20f, 0.45f));

        UIKit.Label(ui, "HOW TO PLAY", 96, GameConfig.Sun, new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(1400f, 130f));

        KeyCard(ui, new Vector2(-560f, 80f), "MOVE LEFT", "A / LEFT", GameConfig.Cyan);
        KeyCard(ui, new Vector2(0f, 80f), "JUMP", "SPACE / UP", GameConfig.HotPink);
        KeyCard(ui, new Vector2(560f, 80f), "MOVE RIGHT", "D / RIGHT", GameConfig.Lime);
        KeyCard(ui, new Vector2(-280f, -250f), "SLIDE", "S / DOWN", GameConfig.Orange);
        UIKit.Label(ui, "Touch: swipe left, right, up or down", 32, new Color(1f, 1f, 1f, 0.85f), new Vector2(0.5f, 0.5f), new Vector2(380f, -215f), new Vector2(800f, 50f), TextAnchor.MiddleLeft, false);
        UIKit.Label(ui, "Jump over beach barriers  -  Slide under neon signs", 32, new Color(1f, 1f, 1f, 0.85f), new Vector2(0.5f, 0.5f), new Vector2(380f, -265f), new Vector2(900f, 50f), TextAnchor.MiddleLeft, false);
        UIKit.Label(ui, "Collect coins and keys. Keys revive you after a crash!", 32, GameConfig.Cyan, new Vector2(0.5f, 0.5f), new Vector2(380f, -315f), new Vector2(900f, 50f), TextAnchor.MiddleLeft, false);

        loadingText = UIKit.Label(ui, "LOADING...", 48, Color.white, new Vector2(1f, 0f), new Vector2(-330f, 110f), new Vector2(500f, 70f), TextAnchor.MiddleRight);
        UIKit.Box(ui, "BarBg", new Vector2(1f, 0f), new Vector2(-330f, 60f), new Vector2(BarW, BarH), new Color(1f, 1f, 1f, 0.25f));
        RectTransform rt = UIKit.MakeRect(ui, "BarFill", new Vector2(1f, 0f), new Vector2(0f, 0.5f), new Vector2(-330f - BarW * 0.5f, 60f), new Vector2(2f, BarH));
        Image fi = rt.gameObject.AddComponent<Image>();
        fi.sprite = UIKit.Rounded; fi.type = Image.Type.Sliced; fi.color = GameConfig.Cyan; fi.raycastTarget = false;
        barFill = rt;
        UIKit.Label(ui, "ENTER = skip", 28, new Color(1f, 1f, 1f, 0.6f), new Vector2(0f, 0f), new Vector2(150f, 50f), new Vector2(260f, 40f), TextAnchor.MiddleLeft, false, false);
    }

    void KeyCard(Transform ui, Vector2 pos, string title, string keys, Color color)
    {
        Image card = UIKit.Box(ui, "Card_" + title, new Vector2(0.5f, 0.5f), pos, new Vector2(480f, 300f), new Color(0.06f, 0.02f, 0.18f, 0.88f));
        UIKit.Box(card.transform, "Accent", new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(480f, 20f), color);
        UIKit.Label(card.transform, title, 52, color, new Vector2(0.5f, 1f), new Vector2(0f, -85f), new Vector2(460f, 70f));
        Image cap = UIKit.Box(card.transform, "Cap", new Vector2(0.5f, 0.5f), new Vector2(0f, -45f), new Vector2(380f, 110f), Color.white);
        UIKit.Label(cap.transform, keys, 48, GameConfig.Navy, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(370f, 100f), TextAnchor.MiddleCenter, true, false);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float p = Mathf.Clamp01(elapsed / 3f);
        if (barFill != null) barFill.sizeDelta = new Vector2(Mathf.Max(2f, BarW * p), BarH);
        int dots = (int)(Time.time * 3f) % 4;
        if (loadingText != null) loadingText.text = "LOADING" + new string('.', dots);
        if (!loading && (elapsed >= 3f || GameInput.Confirm))
        {
            loading = true;
            Fade.LoadScene(GameConfig.SceneGameplay, 1f);
        }
    }
}
