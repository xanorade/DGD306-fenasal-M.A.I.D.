using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    public bool loop = false;
    public AudioMixerGroup output;
    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] musicTracks;
    public Sound[] sfxSounds;
    public Sound[] uiSounds;
    
    private AudioSource currentMusicSource;

    void Awake()
    {

        #region Singleton
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion

        InitializeSounds(musicTracks);
        InitializeSounds(sfxSounds);
        InitializeSounds(uiSounds);
    }

    void InitializeSounds(Sound[] sounds)
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.output;
            s.source.playOnAwake = false;
        }
    }

    void Start()
    {
        PlayMusic("MainMenuMusic");
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicTracks, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("AudioManager: Could not find" + name );
            return;
        }

        if(currentMusicSource != null && currentMusicSource.isPlaying)
        {
            currentMusicSource.Stop();
        }

        s.source.Play();
        currentMusicSource = s.source;
    }

    public void StopMusic()
    {
        if (currentMusicSource != null && currentMusicSource.isPlaying)
        {
            StartCoroutine(FadeOutMusicCoroutine(currentMusicSource, 1f));
        }
    }

    private IEnumerator FadeOutMusicCoroutine(AudioSource musicSource, float fadeTime)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, sound => sound.name == name);
        if (s == null) s = Array.Find(uiSounds, sound => sound.name == name);
        
        if (s == null)
        {
            Debug.LogWarning("AudioManager: Could not find" + name);
            return;
        }
        
        s.source.PlayOneShot(s.source.clip);
    }
}