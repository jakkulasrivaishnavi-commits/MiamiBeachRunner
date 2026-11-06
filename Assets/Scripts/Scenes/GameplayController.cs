using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Scene 4 - Gameplay. Builds the player, track, HUD, game over / revive and pause screens,
/// and drives the Ready -> Running -> Dead state machine.
/// </summary>
public class GameplayController : MonoBehaviour
{
    public static GameplayController Instance;
    public enum State { Ready, Running, Dead, Paused }
    public State state = State.Ready;

    PlayerMovement player;
    CollisionDetect collision;
    CameraFollow camFollow;
    SegmentGenerator generator;

    Text tapText, hintText, diffText, reviveLabel;
    Text[] goValues = new Text[4];
    Image flash, diffChip;
    GameObject goPanel, pausePanel, hudRoot;
    Button reviveButton;

    int reviveCount;
    float readyTimer;
    State stateBeforePause;

    int ReviveCost { get { return 1 << Mathf.Min(reviveCount, 3); } }

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        AudioManager.Ensure();
        Fade.Ensure();
        SaveLoad.Load();
        MasterLevelInfo.ResetRun();

        Camera cam = WorldSetup.Apply();
        BuildPlayer(cam);
        BuildHud();
        gameObject.AddComponent<SaveLoad>();
        state = State.Ready;
        player.model.SetPose(RunnerModel.Pose.Idle);
    }

    void BuildPlayer(Camera cam)
    {
        GameObject pgo = new GameObject("Player");
        pgo.transform.position = Vector3.zero;
        pgo.AddComponent<Rigidbody>();
        pgo.AddComponent<CapsuleCollider>();
        player = pgo.AddComponent<PlayerMovement>();
        collision = pgo.AddComponent<CollisionDetect>();
        player.model = RunnerModel.Create(pgo.transform);

        camFollow = cam.gameObject.GetComponent<CameraFollow>();
        if (camFollow == null) camFollow = cam.gameObject.AddComponent<CameraFollow>();
        camFollow.target = pgo.transform;
        camFollow.player = player;
        camFollow.Snap();

        GameObject g = new GameObject("SegmentGenerator");
        generator = g.AddComponent<SegmentGenerator>();
        generator.target = pgo.transform;
        generator.withObstacles = true;
    }

    // ================================================================== HUD

    void BuildHud()
    {
        Canvas canvas = UIKit.CreateCanvas("HUD", 10);
        UIKit.EnsureEventSystem();
        Transform ui = canvas.transform;
        MasterLevelInfo info = gameObject.AddComponent<MasterLevelInfo>();

        hudRoot = new GameObject("HudRoot", typeof(RectTransform));
        RectTransform hr = hudRoot.GetComponent<RectTransform>();
        hr.SetParent(ui, false);
        hr.anchorMin = Vector2.zero; hr.anchorMax = Vector2.one; hr.offsetMin = Vector2.zero; hr.offsetMax = Vector2.zero;
        Transform hud = hudRoot.transform;

        // score panel (top-left)
        Image sp = UIKit.Box(hud, "ScorePanel", new Vector2(0f, 1f), new Vector2(215f, -95f), new Vector2(400f, 150f), new Color(0.08f, 0.03f, 0.2f, 0.62f));
        UIKit.Label(sp.transform, "SCORE", 26, GameConfig.Cyan, new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(380f, 40f));
        info.scoreDisplay = UIKit.Label(sp.transform, "0", 68, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(380f, 90f));
        info.runDisplay = UIKit.Label(sp.transform, "0 M", 30, GameConfig.Sun, new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(380f, 40f));

        // pause, coins, keys (top-right)
        UIKit.MakeButton(hud, "II", new Vector2(1f, 1f), new Vector2(-70f, -70f), new Vector2(80f, 80f), GameConfig.Purple, Color.white, 40, () => Pause());
        Image cc = UIKit.Box(hud, "CoinChip", new Vector2(1f, 1f), new Vector2(-230f, -70f), new Vector2(210f, 80f), new Color(0.08f, 0.03f, 0.2f, 0.62f));
        UIKit.Icon(cc.transform, new Vector2(0f, 0.5f), new Vector2(48f, 0f), 56f, GameConfig.Sun, "$", 36);
        info.coinDisplay = UIKit.Label(cc.transform, "0", 44, Color.white, new Vector2(1f, 0.5f), new Vector2(-70f, 0f), new Vector2(120f, 70f), TextAnchor.MiddleRight);
        Image kc = UIKit.Box(hud, "KeyChip", new Vector2(1f, 1f), new Vector2(-455f, -70f), new Vector2(210f, 80f), new Color(0.08f, 0.03f, 0.2f, 0.62f));
        UIKit.Icon(kc.transform, new Vector2(0f, 0.5f), new Vector2(48f, 0f), 56f, GameConfig.Cyan, "K", 34);
        info.keyDisplay = UIKit.Label(kc.transform, "0", 44, Color.white, new Vector2(1f, 0.5f), new Vector2(-70f, 0f), new Vector2(120f, 70f), TextAnchor.MiddleRight);

        // difficulty chip
        diffChip = UIKit.Box(hud, "DiffChip", new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(230f, 60f), new Color(0.2f, 0.8f, 0.4f, 0.85f));
        diffText = UIKit.Label(diffChip.transform, "EASY", 34, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220f, 56f), TextAnchor.MiddleCenter, true, false);

        tapText = UIKit.Label(ui, "TAP TO START", 96, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(1400f, 130f));
        hintText = UIKit.Label(ui, "A / D or LEFT / RIGHT: lane    SPACE / UP: jump    S / DOWN: slide", 34, new Color(1f, 1f, 1f, 0.9f),
            new Vector2(0.5f, 0.5f), new Vector2(0f, -250f), new Vector2(1500f, 50f), TextAnchor.MiddleCenter, false);

        flash = UIKit.Full(ui, new Color(1f, 0.1f, 0.2f, 0f));

        BuildGameOver(ui);
        BuildPause(ui);
    }

    void BuildGameOver(Transform ui)
    {
        Image p = UIKit.Box(ui, "GameOver", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 740f), new Color(0.08f, 0.03f, 0.22f, 0.95f));
        goPanel = p.gameObject;
        UIKit.Box(p.transform, "Bar", new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(900f, 28f), GameConfig.HotPink);
        UIKit.Label(p.transform, "GAME OVER", 110, GameConfig.HotPink, new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(860f, 140f));

        string[] names = { "SCORE", "DISTANCE", "COINS", "BEST DISTANCE" };
        for (int i = 0; i < 4; i++)
        {
            float y = 110f - i * 78f;
            UIKit.Label(p.transform, names[i], 40, GameConfig.Cyan, new Vector2(0.5f, 0.5f), new Vector2(-190f, y), new Vector2(380f, 60f), TextAnchor.MiddleLeft, true, false);
            goValues[i] = UIKit.Label(p.transform, "0", 48, Color.white, new Vector2(0.5f, 0.5f), new Vector2(190f, y), new Vector2(380f, 60f), TextAnchor.MiddleRight, true, false);
        }

        reviveButton = UIKit.MakeButton(p.transform, "REVIVE", new Vector2(0.5f, 0f), new Vector2(-300f, 90f), new Vector2(270f, 110f), GameConfig.Lime, GameConfig.Navy, 32, Revive);
        reviveLabel = reviveButton.GetComponentInChildren<Text>();
        UIKit.MakeButton(p.transform, "RESTART", new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(270f, 110f), GameConfig.HotPink, Color.white, 34, Restart);
        UIKit.MakeButton(p.transform, "MENU", new Vector2(0.5f, 0f), new Vector2(300f, 90f), new Vector2(270f, 110f), GameConfig.Purple, Color.white, 34, ToMenu);
        goPanel.SetActive(false);
    }

    void BuildPause(Transform ui)
    {
        Image p = UIKit.Box(ui, "Pause", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 820f), new Color(0.08f, 0.03f, 0.22f, 0.97f));
        pausePanel = p.gameObject;
        UIKit.Label(p.transform, "PAUSED", 96, GameConfig.Cyan, new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(800f, 120f));
        UIKit.AddVolumeSliders(p.transform, 190f);
        UIKit.MakeButton(p.transform, "RESUME", new Vector2(0.5f, 0f), new Vector2(0f, 270f), new Vector2(420f, 100f), GameConfig.Lime, GameConfig.Navy, 42, () => Resume());
        UIKit.MakeButton(p.transform, "RESTART", new Vector2(0.5f, 0f), new Vector2(-220f, 130f), new Vector2(380f, 100f), GameConfig.HotPink, Color.white, 38, Restart);
        UIKit.MakeButton(p.transform, "MENU", new Vector2(0.5f, 0f), new Vector2(220f, 130f), new Vector2(380f, 100f), GameConfig.Purple, Color.white, 38, ToMenu);
        pausePanel.SetActive(false);
    }

    // ================================================================== STATE

    void Update()
    {
        if (GameInput.Pause)
        {
            if (state == State.Running || state == State.Ready) { Pause(); return; }
            if (state == State.Paused) { Resume(); return; }
        }

        if (state == State.Ready)
        {
            readyTimer += Time.deltaTime;
            if (tapText != null) tapText.color = new Color(1f, 1f, 1f, 0.55f + 0.45f * Mathf.Sin(Time.time * 5f));
            bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (readyTimer > 0.5f && GameInput.AnyPress && !overUi) StartRun();
        }
        else if (state == State.Running)
        {
            UpdateDifficulty();
        }
    }

    void StartRun()
    {
        state = State.Running;
        player.Running = true;
        player.model.SetPose(RunnerModel.Pose.Run);
        tapText.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);
    }

    void UpdateDifficulty()
    {
        int d = MasterLevelInfo.distanceRun;
        string label = "EASY"; Color c = new Color(0.2f, 0.8f, 0.4f, 0.9f);
        if (d >= 1500) { label = "HARD"; c = new Color(0.95f, 0.2f, 0.3f, 0.9f); }
        else if (d >= 500) { label = "MEDIUM"; c = new Color(1f, 0.6f, 0.1f, 0.9f); }
        diffText.text = label;
        diffChip.color = c;
    }

    public void Pause()
    {
        if (state != State.Running && state != State.Ready) return;
        stateBeforePause = state;
        state = State.Paused;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        if (state != State.Paused) return;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        state = stateBeforePause;
        if (state == State.Ready) readyTimer = 0f;
    }

    public void FlashRed()
    {
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        float t = 0f;
        while (t < 0.6f)
        {
            t += Time.unscaledDeltaTime;
            flash.color = new Color(1f, 0.1f, 0.2f, Mathf.Lerp(0.55f, 0f, t / 0.6f));
            yield return null;
        }
        flash.color = new Color(1f, 0.1f, 0.2f, 0f);
    }

    public void ShowGameOver()
    {
        state = State.Dead;
        SaveLoad.Save();
        goValues[0].text = MasterLevelInfo.Score.ToString("N0");
        goValues[1].text = MasterLevelInfo.distanceRun + " M";
        goValues[2].text = MasterLevelInfo.coinCount.ToString();
        goValues[3].text = SaveLoad.BestDistance + " M";
        bool can = MasterLevelInfo.keyCount >= ReviveCost;
        reviveButton.interactable = can;
        reviveLabel.text = "REVIVE  (" + ReviveCost + " KEY" + (ReviveCost > 1 ? "S" : "") + ")";
        goPanel.SetActive(true);
    }

    void Revive()
    {
        if (state != State.Dead) return;
        if (MasterLevelInfo.keyCount < ReviveCost) return;
        MasterLevelInfo.keyCount -= ReviveCost;
        reviveCount++;
        SaveLoad.Save();

        float z = player.transform.position.z;
        generator.ClearObstacles(z - 6f, z + 60f);
        goPanel.SetActive(false);
        player.ResumeAfterRevive();
        collision.ResetCrash();
        collision.invincibleUntil = Time.time + 3f;
        StartCoroutine(Blink(3f));
        AudioManager.Play(Sfx.Revive);
        state = State.Running;
    }

    IEnumerator Blink(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            player.model.SetVisible(((int)(t * 10f)) % 2 == 0);
            yield return null;
        }
        player.model.SetVisible(true);
    }

    void Restart()
    {
        SaveLoad.Save();
        Fade.LoadScene(GameConfig.SceneGameplay, 0.6f);
    }

    void ToMenu()
    {
        SaveLoad.Save();
        Fade.LoadScene(GameConfig.SceneMainMenu, 0.6f);
    }

    void OnApplicationQuit()
    {
        SaveLoad.Save();
    }
}
