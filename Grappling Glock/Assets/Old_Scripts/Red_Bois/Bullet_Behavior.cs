using UnityEngine;
using UnityEngine.SceneManagement;

public class Bullet : MonoBehaviour
{
    public float speed = 30f;
    public float lifetime = 3f;
    public LayerMask breakMask;
    public AudioClip spawnSound;

    private AudioSource audioSource;
    public TimeSlow timeSlow;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();



        // Apply pitch from TimeSlow
        if (audioSource != null && timeSlow != null)
        {
            if (spawnSound != null)
            {
                audioSource.clip = spawnSound;
            }

            timeSlow.ApplyPitchTo(audioSource);
            audioSource.Play();
        }

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;

        if (hitObject.CompareTag("Player"))
        {
            SceneManager.LoadScene("Dead");
        }

        if (((1 << hitObject.layer) & breakMask) != 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject hitObject = other.gameObject;

        if (hitObject.CompareTag("Player"))
        {
            SceneManager.LoadScene("Dead");
        }

        if (((1 << hitObject.layer) & breakMask) != 0)
        {
            Destroy(gameObject);
        }
    }
}
