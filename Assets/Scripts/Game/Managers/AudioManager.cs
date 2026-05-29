using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton managing the sound in the game.
/// Any class is free to call its methods
/// TODO make appear in class on its own? Or should it be in every scene?
/// </summary>
public class AudioManager : SmartSingleton<AudioManager>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Tooltip("How long does the fading music take")]
    [SerializeField] private float musicFadeTime = 1.0f;

    [Tooltip("Pause after original music fades and before new musics starts playing")]
    [SerializeField] private float betweenMusicTime = 0.5f;

    [Tooltip("How fast does the sound slow down during special effects")]
    [SerializeField] private float slowdownSpeed;

    private List<AudioSource> activeAudioSources = new(); // Use for effects

    //[SerializeField] private AudioSource musicSource2; // Second source for switching music
    //private AudioSource currentMusicSource; // Which of the music sources is currently playing.


    /// <summary>
    /// Plays the music with smooth transition
    /// Smooth transitions will be added (TODO)
    /// </summary>
    /// <param name="music">The music to play</param>
    public void PlayMusic(AudioClip music) {
        //musicSource.clip = music;
        //musicSource.Play();

        // TODO test properly if the fading sounds nice
        StartCoroutine(SwitchMusic(music));
    }

    private IEnumerator SwitchMusic(AudioClip music) {
        musicSource.DOFade(0, musicFadeTime);
        yield return new WaitForSeconds(musicFadeTime + betweenMusicTime); // Since DOFade is in normal unity time, normal wait for seconds is used here.
        musicSource.clip = music;
        musicSource.DOFade(1, musicFadeTime);
        musicSource.Play();
    }

    //private IEnumerator SwitchMusic(AudioClip newMusic) {
    //    if (currentMusicSource != null) {
    //        var oldSource = currentMusicSource;
    //        currentMusicSource = currentMusicSource == musicSource? musicSource2 : musicSource;
    //        oldSource.DOFade(0, musicSwitchTime);
    //    }
    //    else {
    //        currentMusicSource = musicSource;
    //    }
    //    currentMusicSource.volume = 0;
    //    currentMusicSource.clip = newMusic;
    //    currentMusicSource.Play();
    //    currentMusicSource.DOFade(1.0f, musicSwitchTime);
    //}

    /// <summary>
    /// Plays SFX
    /// </summary>
    /// <param name="sfx">The sfx to play</param>
    //public void PlaySFX(AudioClip sfx) {
    //    if (sfx == null) return;
    //    sfxSource.PlayOneShot(sfx);
    //}

    //public void PlaySFX(SoundData sfx) {
    //    if (sfx == null || sfx.clip == null) return;
    //    sfxSource.PlayOneShot(sfx.clip, sfx.volume);
    //}

    /// <summary>
    /// Do not use!
    /// Plays clip at a specific point in space
    /// We cannot do PlayClipAtPoint, because we want to use mixer, so we create a new object ourselves
    /// Returns the created audio source.
    /// TODO add object pooling if performance issues
    /// </summary>
    public AudioSource PlaySFX(AudioClip clip, Vector3 position = new Vector3(), float duration = -1, Transform parent = null) {
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

    public AudioSource PlaySFX(SoundData clip, Vector3 position = new Vector3(), float duration = -1, Transform parent = null) {
        if (clip == null || clip.clip == null) return null;
        var source = PlaySFX(clip.clip, position, duration, parent);
        source.volume = clip.RandomizedVolume;
        source.pitch = clip.RandomizedPitch;

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

    public AudioSource CreateSFXAudioSource(GameObject target, SoundData sound) {
        var audioSource = CreateSFXAudioSource(target);
        audioSource.clip = sound.clip;
        audioSource.volume = sound.RandomizedVolume;
        audioSource.pitch = sound.RandomizedPitch;
        return audioSource;
    }

    /// <summary>
    /// Do not use
    /// Plays sound using the sound data
    /// </summary>
    public void PlayUISound(SoundData sound) {
        var temp = sfxSource.volume;
        sfxSource.volume = sound.volume;
        PlaySFX(sound.clip);
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
