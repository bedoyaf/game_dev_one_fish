using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

//vibe coded in 3 seconds:/
public class VolumeController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer audioMixer;
     private string exposedParameter = "";
    [SerializeField] private Channel channel = Channel.Master;
    [SerializeField] private float defaultVolume = 1f;

    private const string PrefKeyBase = "volume_";
    private string currentPrefKey;

    private enum Channel
    {
        Master,
        Music,
        SFX
    }

    void Awake()
    {
        if (volumeSlider == null || audioMixer == null)
        {
            Debug.LogWarning("VolumeController: assign Slider and AudioMixer (and set exposed parameter) in inspector.");
        }

        // If exposedParameter wasn't set in inspector, use the selected channel's default name
        if (string.IsNullOrEmpty(exposedParameter))
        {
            switch (channel)
            {
                case Channel.Music:
                    exposedParameter = "Music";
                    break;
                case Channel.SFX:
                    exposedParameter = "SFX";
                    break;
                default:
                    exposedParameter = "Master";
                    break;
            }
        }

        currentPrefKey = PrefKeyBase + exposedParameter;

        float saved = PlayerPrefs.HasKey(currentPrefKey) ? PlayerPrefs.GetFloat(currentPrefKey) : defaultVolume;
        SetVolume(saved);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = saved;
            volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        SetVolume(value);
        if (!string.IsNullOrEmpty(currentPrefKey))
            PlayerPrefs.SetFloat(currentPrefKey, value);
    }

    private void SetVolume(float value)
    {
        if (audioMixer == null)
            return;

        // Convert linear 0..1 to decibels. Use -80 dB as mute for zero.
        float clamped = Mathf.Clamp01(value);
        float dB;
        if (clamped <= 0f)
            dB = -80f;
        else
            dB = 20f * Mathf.Log10(clamped);

        bool ok = audioMixer.SetFloat(exposedParameter, dB);
        if (!ok)
        {
            Debug.LogWarning($"VolumeController: AudioMixer does not contain exposed parameter '{exposedParameter}'. Make sure it's exposed and the name matches exactly.");
        }
    }
}
