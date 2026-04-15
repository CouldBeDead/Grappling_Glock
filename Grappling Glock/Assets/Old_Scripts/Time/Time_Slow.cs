using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.InputSystem;

public class TimeSlow : MonoBehaviour
{
    [Header("Time Settings")]
    public float slowTimeScale = 0.2f;
    public float slowPitch = 0.6f;

    [Header("Vignette Settings")]
    public PostProcessVolume postProcessVolume;
    public float vignetteFadeSpeed = 2f;
    public float maxVignetteIntensity = 0.4f;

    private float originalFixedDeltaTime;
    private Vignette vignette;

    private bool isTimeSlowed = false;
    private float currentTargetPitch = 1f;

    public float CurrentTargetPitch => currentTargetPitch;

    void Awake()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
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

        UpdateAllAudioPitches();
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
            return;

        if (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame)
        {
            Time.timeScale = slowTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime * slowTimeScale;
            isTimeSlowed = true;
        }
        else if (kb.leftShiftKey.wasReleasedThisFrame || kb.rightShiftKey.wasReleasedThisFrame)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            isTimeSlowed = false;
        }

        currentTargetPitch = isTimeSlowed ? slowPitch : 1f;
        UpdateAllAudioPitches();

        if (vignette != null)
        {
            float target = isTimeSlowed ? maxVignetteIntensity : 0f;
            vignette.intensity.value = Mathf.Lerp(
                vignette.intensity.value,
                target,
                Time.unscaledDeltaTime * vignetteFadeSpeed
            );
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

    void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime > 0f ? originalFixedDeltaTime : 0.02f;
    }
}