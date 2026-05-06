using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerSpin : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Transform playerCapsule;

    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad = "WiningRoom";

    [Header("Spin Settings")]
    [SerializeField] private float rotationSpeed = 45f;

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.transform == playerCapsule ||
            collision.collider.transform.IsChildOf(playerCapsule))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}