using UnityEngine;

/// <summary>
/// Adds a permanent sfx audio source to the object.
/// </summary>
public class PlayAudio : MonoBehaviour
{
    public AudioClip sfxClip;
    void Start()
    {
        var audioSource = AudioManager.Instance.CreateSFXAudioSource(gameObject);
        audioSource.loop = true;
        audioSource.clip = sfxClip;
        audioSource.Play();
    }
}
