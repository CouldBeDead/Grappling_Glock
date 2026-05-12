using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "SceneName";

    public void LoadScene()
    {
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogError("SceneButton: No scene name was set.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            Debug.LogError("SceneButton: Scene '" + sceneToLoad + "' is not in the Build Profile / Scene List.");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}