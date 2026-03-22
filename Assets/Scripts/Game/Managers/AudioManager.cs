using UnityEngine;

/// <summary>
/// Singleton managing the sound in the game.
/// Any class is free to call its methods
/// </summary>
public class AudioManager : SmartSingleton<AudioManager>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    /// <summary>
    /// Plays the music
    /// Smooth transitions will be added (TODO)
    /// </summary>
    /// <param name="music">The music to play</param>
    public void PlayMusic(AudioClip music) {
        musicSource.clip = music;
        musicSource.Play();
    }

    /// <summary>
    /// Plays SFX
    /// </summary>
    /// <param name="sfx">The sfx to play</param>
    public void PlaySFX(AudioClip sfx) {
        sfxSource.PlayOneShot(sfx);
    }
}
