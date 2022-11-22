using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[ExecuteInEditMode]
public class StartMainScene : EditorWindow 
{
    [MenuItem("Help/Play-Stop MainMenu Scene %0")]
    public static void PlayFromScene()
    {
        if ( EditorApplication.isPlaying == true )
        {
            EditorApplication.isPlaying = false;
            return;
        }
        
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        EditorApplication.isPlaying = true;
    }
}