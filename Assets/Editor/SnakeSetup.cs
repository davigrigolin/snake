using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class SnakeSetup
{
    [MenuItem("Tools/Snake 2D/Create or Reset Snake Scene")]
    public static void CreateSnakeScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 12f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.backgroundColor = new Color(0.055f, 0.07f, 0.09f);
        camera.clearFlags = CameraClearFlags.SolidColor;

        GameObject manager = new GameObject("Snake Game");
        manager.AddComponent<SnakeGame>();

        if (!Directory.Exists("Assets/Scenes"))
            Directory.CreateDirectory("Assets/Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/SnakeGame.unity");
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Snake 2D", "Cena criada em Assets/Scenes/SnakeGame.unity. Pressione Play para testar.", "OK");
    }
}
