using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldSpaceButtonSceneLoader : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button button;

    [Header("Scene")]
    [SerializeField] private string sceneToLoad = "WiningRoom";

    private void Start()
    {
        if (button == null)
        {
            Debug.LogError("No button assigned to WorldSpaceButtonSceneLoader.");
            return;
        }

        button.onClick.AddListener(LoadScene);
    }

    private void LoadScene()
    {
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogError("No scene name assigned.");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(LoadScene);
        }
    }
}