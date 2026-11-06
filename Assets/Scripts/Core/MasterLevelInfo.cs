using UnityEngine;
using UnityEngine.UI;

/// <summary>Holds the live coin / key / distance counters and refreshes the HUD (PPT "MasterLevelInfo").</summary>
public class MasterLevelInfo : MonoBehaviour
{
    public static int coinCount = 0;
    public static int keyCount = 0;      // key wallet (saved keys + keys found this run)
    public static int distanceRun = 0;
    public static int Score { get { return distanceRun + coinCount * 10; } }

    public Text coinDisplay, keyDisplay, runDisplay, scoreDisplay;

    public static void ResetRun()
    {
        coinCount = 0;
        distanceRun = 0;
        keyCount = PlayerPrefs.GetInt(GameConfig.PrefKeys, 0);
    }

    void Update()
    {
        if (coinDisplay) coinDisplay.text = coinCount.ToString();
        if (keyDisplay) keyDisplay.text = keyCount.ToString();
        if (runDisplay) runDisplay.text = distanceRun + " M";
        if (scoreDisplay) scoreDisplay.text = Score.ToString("N0");
    }
}
