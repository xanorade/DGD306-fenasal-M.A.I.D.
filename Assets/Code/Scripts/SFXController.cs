using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;
    
    public AudioSource selectSound;
    public AudioSource clickSound;

    public void PlaySelectSound() { 
        selectSound.Play();
    }

    public void PlayClickSound() { 
        clickSound.Play();
    }
}