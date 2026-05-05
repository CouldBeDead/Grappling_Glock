using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class TimeSlow : MonoBehaviour
{
    [Header("Time Settings")]
    public float slowTimeScale = 0.2f;
    public float slowPitch = 0.6f;

    [Header("Audio Settings")]
    public float audioPitchFadeSpeed = 4f;

    [Header("Vignette Settings")]
    public float vignetteFadeSpeed = 2f;
    public float maxVignetteIntensity = 0.4f;
    public float vignetteSmoothness = 0.5f;

    [Header("URP Volume Settings")]
    [SerializeField] private Volume volume;
    [SerializeField] private VolumeProfile defaultVolumeProfile;
    [SerializeField] private bool createGlobalVolumeIfMissing = true;
    [SerializeField] private Camera targetCamera;

    private float originalFixedDeltaTime;

    private Vignette vignette;
    private VolumeProfile runtimeProfile;

    private bool isTimeSlowed = false;

    private float targetPitch = 1f;
    private float currentPitch = 1f;

    public float CurrentTargetPitch => currentPitch;

    private void Awake()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void Start()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;

        isTimeSlowed = false;
        targetPitch = 1f;
        currentPitch = 1f;

        SetupCameraPostProcessing();
        SetupVolume();
        UpdateAllAudioPitches();
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame)
        {
            StartSlowMotion();
        }
        else if (kb.leftShiftKey.wasReleasedThisFrame || kb.rightShiftKey.wasReleasedThisFrame)
        {
            StopSlowMotion();
        }

        UpdateAudioPitchFade();
        UpdateVignette();
    }

    private void StartSlowMotion()
    {
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime * slowTimeScale;

        isTimeSlowed = true;
        targetPitch = slowPitch;
    }

    private void StopSlowMotion()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        isTimeSlowed = false;
        targetPitch = 1f;
    }

    private void UpdateAudioPitchFade()
    {
        currentPitch = Mathf.Lerp(
            currentPitch,
            targetPitch,
            Time.unscaledDeltaTime * audioPitchFadeSpeed
        );

        UpdateAllAudioPitches();
    }

    private void SetupCameraPostProcessing()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogWarning("No camera found. Make sure your camera has URP post-processing enabled.");
            return;
        }

        UniversalAdditionalCameraData cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();

        if (cameraData != null)
        {
            cameraData.renderPostProcessing = true;
        }
        else
        {
            Debug.LogWarning("Camera does not have UniversalAdditionalCameraData. Make sure the project is using URP.");
        }
    }

    private void SetupVolume()
    {
        if (volume == null)
        {
#if UNITY_2023_1_OR_NEWER
            volume = FindFirstObjectByType<Volume>();
#else
            volume = FindObjectOfType<Volume>();
#endif
        }

        if (volume == null && createGlobalVolumeIfMissing)
        {
            GameObject volumeObject = new GameObject("Runtime Global Volume");
            volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 100f;
        }

        if (volume == null)
        {
            Debug.LogWarning("No Volume found or created. Vignette effects will not be applied.");
            return;
        }

        volume.isGlobal = true;

        VolumeProfile sourceProfile = null;

        if (defaultVolumeProfile != null)
        {
            sourceProfile = defaultVolumeProfile;
        }
        else if (volume.sharedProfile != null)
        {
            sourceProfile = volume.sharedProfile;
        }

        if (sourceProfile != null)
        {
            runtimeProfile = Instantiate(sourceProfile);
        }
        else
        {
            runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        }

        runtimeProfile.name = "Runtime TimeSlow Volume Profile";
        volume.profile = runtimeProfile;

        if (!runtimeProfile.TryGet(out vignette))
        {
            vignette = runtimeProfile.Add<Vignette>(true);
        }

        vignette.active = true;

        vignette.intensity.overrideState = true;
        vignette.intensity.value = 0f;

        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = vignetteSmoothness;

        vignette.color.overrideState = true;
        vignette.color.value = Color.black;
    }

    private void UpdateVignette()
    {
        if (vignette == null) return;

        float targetIntensity = isTimeSlowed ? maxVignetteIntensity : 0f;

        vignette.intensity.value = Mathf.Lerp(
            vignette.intensity.value,
            targetIntensity,
            Time.unscaledDeltaTime * vignetteFadeSpeed
        );
    }

    private void UpdateAllAudioPitches()
    {
#if UNITY_2023_1_OR_NEWER
        AudioSource[] allSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
#else
        AudioSource[] allSources = FindObjectsOfType<AudioSource>();
#endif

        foreach (AudioSource source in allSources)
        {
            if (source != null)
            {
                source.pitch = currentPitch;
            }
        }
    }

    public void ApplyPitchTo(AudioSource source)
    {
        if (source != null)
        {
            source.pitch = currentPitch;
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime > 0f ? originalFixedDeltaTime : 0.02f;

        isTimeSlowed = false;
        targetPitch = 1f;
        currentPitch = 1f;

        if (vignette != null)
        {
            vignette.intensity.value = 0f;
        }

        UpdateAllAudioPitches();
    }
}