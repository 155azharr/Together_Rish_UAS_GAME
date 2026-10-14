using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MainMenuInstaller
{
    private const string MenuScenePath = "Assets/Main Menu.unity";
    private const string GameplayScenePath = "Assets/Scenes/SampleScene.unity";
    private const string BackgroundPath = "Assets/Backgrounds/TOGETHERISH_MainMenu_4K.png";
    private const string RequestPath = "Temp/Level1MainMenu.request";

    private static string RequestFullPath =>
        Path.Combine(Directory.GetParent(Application.dataPath).FullName, RequestPath);

    [InitializeOnLoadMethod]
    private static void CheckInstallRequest()
    {
        EditorApplication.delayCall += TryInstallRequest;
    }

    private static void TryInstallRequest()
    {
        if (!File.Exists(RequestFullPath))
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            return;
        }

        File.Delete(RequestFullPath);
        InstallMainMenu();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode)
            return;

        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.delayCall += TryInstallRequest;
    }

    [MenuItem("Tools/Menu/Install Level 1 Main Menu")]
    private static void InstallFromMenu()
    {
        InstallMainMenu();
    }

    private static void InstallMainMenu()
    {
        bool wasLoaded = SceneManager.GetSceneByPath(MenuScenePath).isLoaded;
        Scene menuScene = wasLoaded
            ? SceneManager.GetSceneByPath(MenuScenePath)
            : EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Additive);

        GameObject menuObject = null;
        foreach (GameObject root in menuScene.GetRootGameObjects())
        {
            if (root.name == "Level1MainMenu")
            {
                menuObject = root;
                break;
            }
        }

        if (menuObject == null)
        {
            menuObject = new GameObject("Level1MainMenu");
            SceneManager.MoveGameObjectToScene(menuObject, menuScene);
            Undo.RegisterCreatedObjectUndo(menuObject, "Install Level 1 Main Menu");
        }

        Level1MainMenu menu = menuObject.GetComponent<Level1MainMenu>();
        if (menu == null)
            menu = Undo.AddComponent<Level1MainMenu>(menuObject);

        SerializedObject settings = new SerializedObject(menu);
        settings.FindProperty("gameplaySceneName").stringValue = "SampleScene";
        settings.FindProperty("backgroundTexture").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
        settings.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(menu);
        EditorSceneManager.MarkSceneDirty(menuScene);
        EditorSceneManager.SaveScene(menuScene);

        TextureImporter backgroundImporter = AssetImporter.GetAtPath(BackgroundPath) as TextureImporter;
        if (backgroundImporter != null)
        {
            backgroundImporter.textureType = TextureImporterType.Default;
            backgroundImporter.maxTextureSize = 4096;
            backgroundImporter.filterMode = FilterMode.Point;
            backgroundImporter.textureCompression = TextureImporterCompression.Uncompressed;
            backgroundImporter.mipmapEnabled = false;
            backgroundImporter.wrapMode = TextureWrapMode.Clamp;
            backgroundImporter.SaveAndReimport();
        }

        ConfigureBuildScenes();
        AssetDatabase.SaveAssets();

        if (!wasLoaded)
            EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        else
            SceneManager.SetActiveScene(menuScene);

        Debug.Log("Level 1 Main Menu installed and set as the first build scene.");
    }

    private static void ConfigureBuildScenes()
    {
        EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
        EditorBuildSettingsScene[] updated = new EditorBuildSettingsScene[current.Length + 2];
        int count = 0;
        updated[count++] = new EditorBuildSettingsScene(MenuScenePath, true);

        bool gameplayAdded = false;
        foreach (EditorBuildSettingsScene scene in current)
        {
            if (scene.path == MenuScenePath)
                continue;

            if (scene.path == GameplayScenePath)
            {
                if (gameplayAdded)
                    continue;
                gameplayAdded = true;
                updated[count++] = new EditorBuildSettingsScene(GameplayScenePath, true);
            }
            else
            {
                updated[count++] = scene;
            }
        }

        if (!gameplayAdded)
            updated[count++] = new EditorBuildSettingsScene(GameplayScenePath, true);

        System.Array.Resize(ref updated, count);
        EditorBuildSettings.scenes = updated;
    }
}
