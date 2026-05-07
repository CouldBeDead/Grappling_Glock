using UnityEngine;
using UnityEngine.SceneManagement;

public class Bullet : MonoBehaviour
{
    public float speed = 30f;
    public float lifetime = 3f;

    [Header("Layer Masks")]
    public LayerMask playerMask;
    public LayerMask breakMask;

    [Header("Audio")]
    public AudioClip spawnSound;

    private AudioSource audioSource;
    public TimeSlow timeSlow;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null && timeSlow != null)
        {
            if (spawnSound != null)
            {
                audioSource.clip = spawnSound;
            }

            timeSlow.ApplyPitchTo(audioSource);
            audioSource.Play();
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject);
    }

    private void HandleHit(GameObject hitObject)
    {
        int hitLayer = hitObject.layer;

        if (((1 << hitLayer) & playerMask) != 0)
        {
            SceneManager.LoadScene("Dead");
            return;
        }

        if (((1 << hitLayer) & breakMask) != 0)
        {
            Destroy(gameObject);
        }
    }
}
