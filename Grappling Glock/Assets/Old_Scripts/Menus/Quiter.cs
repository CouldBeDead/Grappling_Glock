using UnityEngine;

public class QuitButton : MonoBehaviour
{
    // This function is called from the UI Button's OnClick() event
    public void QuitGame()
    {
        Debug.Log("Quit button pressed. Exiting game...");

        // If running in the Unity editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running a built game
        Application.Quit();
#endif
    }
}
