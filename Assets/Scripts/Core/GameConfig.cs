using UnityEngine;

/// <summary>Central constants: scene names, PlayerPrefs keys and the Miami colour palette.</summary>
public static class GameConfig
{
    // ---- Scene names (must match the files in Assets/Scenes) ----
    public const string SceneMainMenu = "MainMenu";
    public const string SceneStageSelect = "StageSelect";
    public const string SceneInformation = "Information";
    public const string SceneGameplay = "Gameplay";

    // ---- Track ----
    public const float LaneDistance = 2.5f;
    public const float SegmentLength = 60f;

    // ---- PlayerPrefs keys (same names as the original PPT) ----
    public const string PrefCoins = "SavedCoins";
    public const string PrefKeys = "SavedKeys";
    public const string PrefDistance = "SavedDistance";
    public const string PrefBestScore = "SavedBestScore";
    public const string PrefMaster = "VolMaster";
    public const string PrefMusic = "VolMusic";
    public const string PrefSfx = "VolSfx";

    public static Color RGB(int r, int g, int b)
    {
        return new Color(r / 255f, g / 255f, b / 255f, 1f);
    }

    // ---- Miami palette ----
    public static readonly Color HotPink = new Color(1f, 0.20f, 0.59f);
    public static readonly Color Cyan = new Color(0f, 0.92f, 1f);
    public static readonly Color Sun = new Color(1f, 0.78f, 0.25f);
    public static readonly Color Purple = new Color(0.50f, 0.25f, 0.95f);
    public static readonly Color Navy = new Color(0.10f, 0.06f, 0.24f);
    public static readonly Color Lime = new Color(0.55f, 1f, 0.35f);
    public static readonly Color Orange = new Color(1f, 0.55f, 0.15f);

    /// <summary>Art-deco pastel building colours (Ocean Drive).</summary>
    public static readonly Color[] Pastels =
    {
        new Color(1.00f, 0.69f, 0.80f),  // flamingo pink
        new Color(0.62f, 0.92f, 0.80f),  // mint
        new Color(1.00f, 0.92f, 0.58f),  // lemon
        new Color(0.60f, 0.82f, 1.00f),  // sky blue
        new Color(1.00f, 0.75f, 0.58f),  // peach
        new Color(0.80f, 0.68f, 1.00f),  // lilac
        new Color(0.98f, 0.96f, 0.93f),  // white deco
    };

    public static readonly Color[] NeonColors = { HotPink, Cyan, Lime, Orange, Purple };
}
