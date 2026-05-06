using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

/// <summary>
/// Singleton managing the sound in the game.
/// Any class is free to call its methods
/// TODO make appear in class on its own? Or should it be in every scene?
/// </summary>
public class AudioManager : SmartSingleton<AudioManager>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float slowdownSpeed;

    private List<AudioSource> activeAudioSources = new(); // Use for effects

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
        if (sfx == null) return;
        sfxSource.PlayOneShot(sfx);
    }

    /// <summary>
    /// Plays clip at a specific point in space
    /// We cannot do PlayClipAtPoint, because we want to use mixer
    /// So we create a new object ourselves
    /// Returns the created audio source.
    /// </summary>
    public AudioSource PlaySFX(AudioClip clip, Vector3 position, float duration = -1, Transform parent = null) {
        if (clip == null) return null;

        var obj = new GameObject(clip.name);
        if (parent != null) {
            obj.transform.parent = parent;
        }

        var source = obj.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        source.transform.position = position;
        source.clip = clip;
        source.loop = false;
        source.Play();

        activeAudioSources.Add(source);
        if (activeAudioSources.Count > 20) CleanUp();

        if (duration == -1) {
            duration = clip.length;
        }

        MyTime.ScheduleDestruction(obj, duration);
        return source;
    }

    /// <summary>
    /// Adds an audio source to the game object with correct audio mixer
    /// </summary>
    /// <param name="target"></param>
    public AudioSource CreateSFXAudioSource(GameObject target) {
        var audioSource = target.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        activeAudioSources.Add(audioSource);

        if (activeAudioSources.Count > 20) CleanUp();

        return audioSource;
    }

    /// <summary>
    /// Plays sound using the sound data
    /// </summary>
    public void PlayUISound(UISoundData sound) {
        var temp = sfxSource.volume;
        sfxSource.volume = sound.volume;
        PlaySFX(sound.sound);
        sfxSource.volume = temp;
    }

    /// <summary>
    /// Changes speed of the clips playing.
    /// </summary>
    public void ChangeSpeed(float newSpeed) {
        musicSource.DOPitch(newSpeed, slowdownSpeed);
        CleanUp();
        foreach(var source in activeAudioSources) {
            source.DOPitch(newSpeed, slowdownSpeed);
        }
    }

    private void CleanUp() {
        activeAudioSources.RemoveAll(x => x == null);
    }
}
