using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Safety net: if a scene named MainMenu / StageSelect / Information / Gameplay is loaded without its controller
/// object, the controller is added automatically. The whole game therefore works even from empty scenes.
/// </summary>
public static class SceneBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        SceneManager.sceneLoaded -= OnLoaded;
        SceneManager.sceneLoaded += OnLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AfterFirstScene()
    {
        Ensure(SceneManager.GetActiveScene().name);
    }

    static void OnLoaded(Scene scene, LoadSceneMode mode)
    {
        Ensure(scene.name);
    }

    static void Ensure(string sceneName)
    {
        if (sceneName == GameConfig.SceneMainMenu && UnityUtil.FindFirst<MainMenuControl>() == null) Make<MainMenuControl>();
        else if (sceneName == GameConfig.SceneStageSelect && UnityUtil.FindFirst<StageControls>() == null) Make<StageControls>();
        else if (sceneName == GameConfig.SceneInformation && UnityUtil.FindFirst<LoadToStage>() == null) Make<LoadToStage>();
        else if (sceneName == GameConfig.SceneGameplay && UnityUtil.FindFirst<GameplayController>() == null) Make<GameplayController>();
    }

    static void Make<T>() where T : MonoBehaviour
    {
        new GameObject(typeof(T).Name).AddComponent<T>();
    }
}
