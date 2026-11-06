#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// On first import this creates the four scenes (MainMenu, StageSelect, Information, Gameplay),
/// registers them in Build Settings in that order and opens MainMenu. Re-run it from the Miami Runner menu.
/// </summary>
[InitializeOnLoad]
public static class ProjectSetup
{
    const string Dir = "Assets/Scenes";

    static ProjectSetup()
    {
        EditorApplication.delayCall += AutoSetup;
    }

    static string Path(string n) { return Dir + "/" + n + ".unity"; }

    static void AutoSetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (File.Exists(Path(GameConfig.SceneMainMenu)) && File.Exists(Path(GameConfig.SceneGameplay))) return;
        Build();
    }

    [MenuItem("Miami Runner/Rebuild Scenes and Build Settings")]
    static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets", "Scenes");

        Create(GameConfig.SceneMainMenu, typeof(MainMenuControl));
        Create(GameConfig.SceneStageSelect, typeof(StageControls));
        Create(GameConfig.SceneInformation, typeof(LoadToStage));
        Create(GameConfig.SceneGameplay, typeof(GameplayController));

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Path(GameConfig.SceneMainMenu), true),
            new EditorBuildSettingsScene(Path(GameConfig.SceneStageSelect), true),
            new EditorBuildSettingsScene(Path(GameConfig.SceneInformation), true),
            new EditorBuildSettingsScene(Path(GameConfig.SceneGameplay), true),
        };

        PlayerSettings.productName = "Miami Beach Runner";
        PlayerSettings.runInBackground = true;
        AddAlwaysIncludedShaders();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(Path(GameConfig.SceneMainMenu));
        Debug.Log("Miami Beach Runner: scenes created. Open MainMenu and press Play.");
    }

    static void Create(string name, System.Type controller)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject go = new GameObject("GameManager");
        go.AddComponent(controller);
        EditorSceneManager.SaveScene(scene, Path(name));
    }

    [MenuItem("Miami Runner/Play From Main Menu")]
    static void PlayFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(Path(GameConfig.SceneMainMenu));
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Miami Runner/Reset Saved Progress")]
    static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Miami Beach Runner: saved progress cleared.");
    }

    // Runtime materials are created with Shader.Find, so the shaders must be included in player builds.
    static void AddAlwaysIncludedShaders()
    {
        try
        {
            Object gs = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/GraphicsSettings.asset");
            SerializedObject so = new SerializedObject(gs);
            SerializedProperty arr = so.FindProperty("m_AlwaysIncludedShaders");
            string[] names = { "Standard", "Sprites/Default", "Universal Render Pipeline/Lit", "UI/Default" };
            foreach (string n in names)
            {
                Shader sh = Shader.Find(n);
                if (sh == null) continue;
                bool has = false;
                for (int i = 0; i < arr.arraySize; i++)
                    if (arr.GetArrayElementAtIndex(i).objectReferenceValue == sh) { has = true; break; }
                if (has) continue;
                arr.arraySize++;
                arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = sh;
            }
            so.ApplyModifiedProperties();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not add shaders to Always Included list (add Standard / Sprites-Default manually): " + e.Message);
        }
    }
}
#endif
