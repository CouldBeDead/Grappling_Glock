using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "SceneName"; // Set the scene in the inspector


    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}