using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnPlayerCollision : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Transform playerCapsule;

    [Header("Scene Settings")]
    [SerializeField] private string sceneName = "Dead";

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.transform == playerCapsule)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}