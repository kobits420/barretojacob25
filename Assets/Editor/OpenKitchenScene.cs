using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
static class OpenKitchenScene
{
    const string ScenePath = "Assets/Scenes/Kitchen.unity";

    static bool tried;

    static OpenKitchenScene()
    {
        EditorApplication.delayCall += TryOpen;
    }

    static void TryOpen()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            tried = false;
            EditorApplication.delayCall += TryOpen;
            return;
        }

        if (tried) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var scene = EditorSceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(scene.path)) return;
        if (!System.IO.File.Exists(ScenePath)) return;

        tried = true;
        try
        {
            EditorSceneManager.OpenScene(ScenePath);
        }
        catch (System.Exception ex)
        {
            tried = false;
            UnityEngine.Debug.LogWarning("Could not open the kitchen scene yet: " + ex.Message);
            EditorApplication.delayCall += TryOpen;
        }
    }
}
