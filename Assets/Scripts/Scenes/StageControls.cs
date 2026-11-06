using UnityEngine;
using UnityEngine.UI;

/// <summary>Scene 2 - Stage Select. One stage: Miami Beach. PLAY loads the Information scene (PPT "StageControls").</summary>
public class StageControls : MonoBehaviour
{
    bool going;

    void Awake()
    {
        Time.timeScale = 1f;
        AudioManager.Ensure();
        Fade.Ensure();
        Camera cam = WorldSetup.Apply();
        ScenicRun s = ScenicRun.Create(cam);
        s.speed = 5f;
        BuildUi();
    }

    void BuildUi()
    {
        Canvas c = UIKit.CreateCanvas("StageCanvas", 10);
        UIKit.EnsureEventSystem();
        Transform ui = c.transform;
        UIKit.Full(ui, new Color(0.05f, 0.02f, 0.14f, 0.55f));

        UIKit.Label(ui, "SELECT STAGE", 80, Color.white, new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1200f, 110f));

        Image card = UIKit.Box(ui, "Card", new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(900f, 500f), new Color(0.12f, 0.05f, 0.3f, 0.92f));
        UIKit.Box(card.transform, "Bar", new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(900f, 28f), GameConfig.HotPink);
        UIKit.Label(card.transform, "MIAMI BEACH", 110, GameConfig.Sun, new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(860f, 150f));
        UIKit.Label(card.transform, "Sunset boardwalk  -  Ocean Drive", 42, GameConfig.Cyan, new Vector2(0.5f, 1f), new Vector2(0f, -230f), new Vector2(860f, 60f));
        UIKit.Label(card.transform, "Trams, beach barriers and neon signs", 34, new Color(1f, 1f, 1f, 0.85f), new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(860f, 50f), TextAnchor.MiddleCenter, false);
        UIKit.Label(card.transform, "BEST DISTANCE  " + PlayerPrefs.GetInt(GameConfig.PrefDistance, 0) + " M", 44, Color.white, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(860f, 60f));
        UIKit.Label(card.transform, "BEST SCORE  " + PlayerPrefs.GetInt(GameConfig.PrefBestScore, 0).ToString("N0"), 38, GameConfig.Orange, new Vector2(0.5f, 0f), new Vector2(0f, 85f), new Vector2(860f, 50f));

        UIKit.MakeButton(ui, "PLAY", new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(460f, 120f), GameConfig.HotPink, Color.white, 64, PressPlay);
        UIKit.MakeButton(ui, "BACK", new Vector2(0f, 0f), new Vector2(180f, 100f), new Vector2(260f, 90f), GameConfig.Purple, Color.white, 40, Back);
    }

    public void PressPlay()
    {
        if (going) return;
        going = true;
        Fade.LoadScene(GameConfig.SceneInformation, 0.8f);
    }

    void Back()
    {
        if (going) return;
        going = true;
        Fade.LoadScene(GameConfig.SceneMainMenu, 0.8f);
    }

    void Update()
    {
        if (!going && GameInput.Confirm) PressPlay();
    }
}
