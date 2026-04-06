using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections.Generic;

public class TimeSlow : MonoBehaviour
{
    [Header("Time Settings")]
    public float slowTimeScale = 0.2f;
    public float slowPitch = 0.6f;
    public KeyCode slowKey = KeyCode.LeftShift;

    [Header("Vignette Settings")]
    public PostProcessVolume postProcessVolume;
    public float vignetteFadeSpeed = 2f;
    public float maxVignetteIntensity = 0.4f;

    private float originalFixedDeltaTime;
    private Vignette vignette;

    private bool isTimeSlowed = false;
    private float currentTargetPitch = 1f;

    private PlayerMovement playerMovement;

    // Public getter so other scripts can read pitch if needed
    public float CurrentTargetPitch => currentTargetPitch;


    void Awake()
{
    // Force reset time BEFORE any physics or game logic runs
    Time.timeScale = 1f;
    Time.fixedDeltaTime = 0.02f; // Unity's default fixedDeltaTime
}

    void Start()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
        
        isTimeSlowed = false;
        currentTargetPitch = 1f;

        if (postProcessVolume != null && postProcessVolume.profile.TryGetSettings(out vignette))
        {
            vignette.intensity.value = 0f;
        }
        else
        {
            Debug.LogWarning("Vignette not found on assigned Post Process Volume.");
        }

        UpdateAllAudioPitches(); // Reset audio pitch just in case
    }

    void Update()
    {
        // Toggle time slow
        if (Input.GetKeyDown(slowKey))
        {
            Time.timeScale = slowTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime * slowTimeScale;
            isTimeSlowed = true;
        }
        else if (Input.GetKeyUp(slowKey))
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            isTimeSlowed = false;
        }

        // Update pitch globally
        currentTargetPitch = isTimeSlowed ? slowPitch : 1f;
        UpdateAllAudioPitches();

        // Vignette animation
        if (vignette != null)
        {
            float target = isTimeSlowed ? maxVignetteIntensity : 0f;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, target, Time.unscaledDeltaTime * vignetteFadeSpeed);
        }
    }

    void UpdateAllAudioPitches()
    {
        AudioSource[] allSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allSources)
        {
            if (!Mathf.Approximately(source.pitch, currentTargetPitch))
            {
                source.pitch = currentTargetPitch;
            }
        }
    }

    public void ApplyPitchTo(AudioSource source)
    {
        if (source != null)
        {
            source.pitch = currentTargetPitch;
        }
    }
}
