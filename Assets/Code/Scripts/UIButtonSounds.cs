using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSounds : MonoBehaviour, ISelectHandler
{
    [Header("Sound Names")]
    public string selectSoundName = "ButtonSelect"; 
    public string clickSoundName = "ButtonClick";   

    public void PlayClickSound()
    {
        if (!string.IsNullOrEmpty(clickSoundName))
        {
            AudioManager.instance.PlaySFX(clickSoundName);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!string.IsNullOrEmpty(selectSoundName))
        {
            AudioManager.instance.PlaySFX(selectSoundName);
        }
    }
}