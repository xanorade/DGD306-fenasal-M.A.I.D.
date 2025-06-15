using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsManager : MonoBehaviour
{
    public AudioMixer gameAudioMixer;

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider uiSlider;
    
    public void SetMasterVolume(float volume)
    {
        float dbValue = (volume > 0.0001f) ? Mathf.Log10(volume) * 20 : -80f;
        gameAudioMixer.SetFloat("Master_Volume", dbValue);
        PlayerPrefs.SetFloat("SavedMasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        float dbValue = (volume > 0.0001f) ? Mathf.Log10(volume) * 20 : -80f;
        gameAudioMixer.SetFloat("BGM_Volume", dbValue);
        PlayerPrefs.SetFloat("SavedMusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        float dbValue = (volume > 0.0001f) ? Mathf.Log10(volume) * 20 : -80f;
        gameAudioMixer.SetFloat("SFX_Volume", dbValue);
        PlayerPrefs.SetFloat("SavedSFXVolume", volume);
    }

    public void SetUIVolume(float volume)
    {
        float dbValue = (volume > 0.0001f) ? Mathf.Log10(volume) * 20 : -80f;
        gameAudioMixer.SetFloat("UI_Volume", dbValue);
        PlayerPrefs.SetFloat("SavedUIVolume", volume);
    }

    
}