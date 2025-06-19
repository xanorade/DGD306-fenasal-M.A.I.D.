using UnityEngine;

public class CharacterAudio : MonoBehaviour
{
    [Header("Character SFX")]
    public AudioClip punchSound;
    public AudioClip kickSound;
    public AudioClip takeDamageSound;
    public AudioClip winSound;
    public AudioClip deathSound;

    public void PlayPunchSound()
    {
        if (AudioManager.instance != null && punchSound != null)
        {
            AudioManager.instance.PlaySFX(punchSound);
        }
    }
    
    public void PlayKickSound()
    {
        if (AudioManager.instance != null && kickSound != null)
        {
            AudioManager.instance.PlaySFX(kickSound);
        }
    }

    public void PlayTakeDamageSound()
    {
        if (AudioManager.instance != null && takeDamageSound != null)
        {
            AudioManager.instance.PlaySFX(takeDamageSound);
        }
    }
    
    public void PlayWinSound()
    {
        if (AudioManager.instance != null && winSound != null)
        {
            AudioManager.instance.PlaySFX(winSound);
        }
    }
    public void PlayDeathSound()
    {
        if (AudioManager.instance != null && deathSound != null)
        {
            AudioManager.instance.PlaySFX(deathSound);
        }
    }
}