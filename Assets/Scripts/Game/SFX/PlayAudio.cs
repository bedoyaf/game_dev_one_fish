using UnityEngine;

/// <summary>
/// Adds a permanent sfx audio source to the object.
/// </summary>
public class PlayAudio : MonoBehaviour
{
    public SoundData sfxClip;
    void Start()
    {
        var audioSource = AudioManager.Instance.CreateSFXAudioSource(gameObject, sfxClip);
        audioSource.loop = true;
        audioSource.Play();
    }
}
