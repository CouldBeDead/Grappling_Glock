using UnityEngine;
using UnityEngine.SceneManagement;

public class KillPlayerOnCollision : MonoBehaviour
{
    public string sceneName = "Dead"; // The scene to load on collision

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
