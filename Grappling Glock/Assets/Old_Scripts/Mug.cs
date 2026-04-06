using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerSpin : MonoBehaviour
{
    public string sceneToLoad = "NextScene"; // Name of the scene to load
    public float rotationSpeed = 45f; // Degrees per second

    void Update()
    {
        // Spin the object around its Y-axis
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the player collided with this object
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
