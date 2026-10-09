using UnityEditor;
using UnityEditor.SceneManagement;

// Opens the main scene when the editor starts on an empty Untitled scene,
// and makes sure the scene is listed in Build Settings.
[InitializeOnLoad]
public static class OpenMainScene
{
    const string ScenePath = "Assets/Scenes/SampleScene.unity";

    static OpenMainScene()
    {
        EditorApplication.delayCall += Run;
    }

    static void Run()
    {
        if (EditorBuildSettings.scenes.Length == 0)
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        var active = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(active.path) && !active.isDirty)
        {
            EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
