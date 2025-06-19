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

    [Header("Sound Lists")]
    public Sound[] musicTracks;
    public Sound[] sfxSounds;
    public Sound[] uiSounds;
    
    private AudioSource currentMusicSource;

    // YENİ EKLENEN SATIR: CharacterAudio gibi dış scriptlerden gelen klipleri çalmak için genel bir kaynak
    private AudioSource sfxSource;

    void Awake()
    {
        #region Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        #endregion

        // YENİ EKLENEN SATIR: Genel SFX kaynağını oluştur ve ayarla
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        // İsterseniz tüm SFX'lerin çıkacağı varsayılan bir mixer grubu atayabilirsiniz
        // sfxSource.outputAudioMixerGroup = ... ; 

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
            s.source.pitch = 1f; // Pitch'i kaldırmıştık, varsayılan olarak 1 ayarlayalım
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.output;
            s.source.playOnAwake = false;
        }
    }

    void Start()
    {
        // Başlangıçta çalacak bir müziğiniz olduğundan emin olun, yoksa bu satır hata verebilir
         PlayMusic("MainMenuMusic");
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicTracks, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("AudioManager: Could not find music track: " + name);
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

    // DÜZELTİLMİŞ FONKSİYON: İsme göre SFX çalar
    public void PlaySFX(string name)
    {
        // Hem sfxSounds hem de uiSounds dizilerinde arama yap
        Sound s = Array.Find(sfxSounds, sound => sound.name == name);
        if (s == null)
        {
            s = Array.Find(uiSounds, sound => sound.name == name);
        }
        
        if (s == null)
        {
            Debug.LogWarning("AudioManager: SFX sound not found in lists: " + name);
            return;
        }
        
        // Bu sese özel olarak oluşturulmuş AudioSource'u kullan ve PlayOneShot ile çal
        s.source.PlayOneShot(s.source.clip);
    }
    
    // DÜZELTİLMİŞ FONKSİYON: Dışarıdan gelen AudioClip'i çalar
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            // Bu tür genel sesler için oluşturduğumuz sfxSource'u kullan
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("AudioManager: Attempted to play a null audio clip!");
        }
    }
}