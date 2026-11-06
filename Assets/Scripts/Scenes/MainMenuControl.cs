using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Scene 1 - Main Menu. Shows saved coins / keys / best distance and the Start Game button (PPT "MainMenuControl").</summary>
public class MainMenuControl : MonoBehaviour
{
    Text clickText;
    GameObject settingsPanel;
    bool starting;

    void Awake()
    {
        Time.timeScale = 1f;
        AudioManager.Ensure();
        Fade.Ensure();
        Camera cam = WorldSetup.Apply();
        ScenicRun.Create(cam);
        BuildUi();
    }

    void BuildUi()
    {
        Canvas c = UIKit.CreateCanvas("MenuCanvas", 10);
        UIKit.EnsureEventSystem();
        Transform ui = c.transform;

        // saved stats (top-left HUD)
        Image panel = UIKit.Box(ui, "Stats", new Vector2(0f, 1f), new Vector2(230f, -130f), new Vector2(420f, 210f), new Color(0.08f, 0.03f, 0.2f, 0.65f));
        UIKit.Icon(panel.transform, new Vector2(0f, 1f), new Vector2(50f, -45f), 54f, GameConfig.Sun, "$", 34);
        UIKit.Label(panel.transform, PlayerPrefs.GetInt(GameConfig.PrefCoins, 0).ToString(), 44, Color.white, new Vector2(0f, 1f), new Vector2(250f, -45f), new Vector2(300f, 60f), TextAnchor.MiddleLeft);
        UIKit.Icon(panel.transform, new Vector2(0f, 1f), new Vector2(50f, -105f), 54f, GameConfig.Cyan, "K", 32);
        UIKit.Label(panel.transform, PlayerPrefs.GetInt(GameConfig.PrefKeys, 0).ToString(), 44, Color.white, new Vector2(0f, 1f), new Vector2(250f, -105f), new Vector2(300f, 60f), TextAnchor.MiddleLeft);
        UIKit.Label(panel.transform, "BEST", 26, GameConfig.HotPink, new Vector2(0f, 1f), new Vector2(60f, -163f), new Vector2(110f, 40f), TextAnchor.MiddleLeft);
        UIKit.Label(panel.transform, PlayerPrefs.GetInt(GameConfig.PrefDistance, 0) + " M", 40, GameConfig.Sun, new Vector2(0f, 1f), new Vector2(250f, -163f), new Vector2(300f, 50f), TextAnchor.MiddleLeft);

        // title
        UIKit.Label(ui, "MIAMI BEACH", 128, GameConfig.HotPink, new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(1500f, 170f));
        UIKit.Label(ui, "SUBWAY SURFERS  -  SUNSET RUN", 40, GameConfig.Cyan, new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(1200f, 60f));

        clickText = UIKit.Label(ui, "CLICK TO START", 64, Color.white, new Vector2(0.5f, 0f), new Vector2(0f, 330f), new Vector2(1000f, 90f));
        UIKit.MakeButton(ui, "START GAME", new Vector2(0.5f, 0f), new Vector2(0f, 170f), new Vector2(520f, 120f), GameConfig.HotPink, Color.white, 56, StartGame);
        UIKit.MakeButton(ui, "SETTINGS", new Vector2(1f, 0f), new Vector2(-170f, 100f), new Vector2(280f, 90f), GameConfig.Purple, Color.white, 38, ToggleSettings);
        UIKit.Label(ui, "Run  -  Dodge  -  Collect  -  Survive", 30, new Color(1f, 1f, 1f, 0.85f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(1000f, 50f), TextAnchor.MiddleCenter, false);

        // settings panel
        Image sp = UIKit.Box(ui, "Settings", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 640f), new Color(0.08f, 0.03f, 0.22f, 0.96f));
        settingsPanel = sp.gameObject;
        UIKit.Label(sp.transform, "AUDIO SETTINGS", 64, GameConfig.Cyan, new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(800f, 90f));
        UIKit.AddVolumeSliders(sp.transform, 40f);
        UIKit.MakeButton(sp.transform, "CLOSE", new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(320f, 90f), GameConfig.HotPink, Color.white, 42, ToggleSettings);
        settingsPanel.SetActive(false);
    }

    void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void StartGame()
    {
        if (starting) return;
        starting = true;
        StartCoroutine(StartButton());
    }

    IEnumerator StartButton()
    {
        AudioManager.Play(Sfx.Click);
        Fade.FadeOutOnly(0.9f);
        yield return new WaitForSeconds(1f);
        Fade.LoadScene(GameConfig.SceneStageSelect, 0.05f);
    }

    void Update()
    {
        if (clickText != null) clickText.color = new Color(1f, 1f, 1f, 0.55f + 0.45f * Mathf.Sin(Time.time * 4f));
        if (starting || settingsPanel.activeSelf) return;
        bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (GameInput.Confirm || (GameInput.AnyPress && !overUi && !Input_IsKeyboardOnlyEscape())) StartGame();
    }

    static bool Input_IsKeyboardOnlyEscape() { return GameInput.Pause; }
}
