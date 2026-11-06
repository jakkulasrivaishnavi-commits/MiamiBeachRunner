using UnityEngine;

/// <summary>Persists coins, keys, best distance and best score with PlayerPrefs (PPT "SaveLoad").</summary>
public class SaveLoad : MonoBehaviour
{
    public static bool saveData = false;
    public static int startCoins;

    public static int SavedCoins { get { return PlayerPrefs.GetInt(GameConfig.PrefCoins, 0); } }
    public static int SavedKeys { get { return PlayerPrefs.GetInt(GameConfig.PrefKeys, 0); } }
    public static int BestDistance { get { return PlayerPrefs.GetInt(GameConfig.PrefDistance, 0); } }
    public static int BestScore { get { return PlayerPrefs.GetInt(GameConfig.PrefBestScore, 0); } }

    public static void Load()
    {
        startCoins = SavedCoins;
    }

    /// <summary>Idempotent: safe to call again after a revive.</summary>
    public static void Save()
    {
        PlayerPrefs.SetInt(GameConfig.PrefCoins, startCoins + MasterLevelInfo.coinCount);
        PlayerPrefs.SetInt(GameConfig.PrefKeys, MasterLevelInfo.keyCount);
        if (MasterLevelInfo.distanceRun > BestDistance) PlayerPrefs.SetInt(GameConfig.PrefDistance, MasterLevelInfo.distanceRun);
        if (MasterLevelInfo.Score > BestScore) PlayerPrefs.SetInt(GameConfig.PrefBestScore, MasterLevelInfo.Score);
        PlayerPrefs.Save();
    }

    void Update()
    {
        if (saveData) { saveData = false; Save(); }
    }
}
