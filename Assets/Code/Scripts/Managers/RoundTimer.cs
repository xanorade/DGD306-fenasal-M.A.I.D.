using UnityEngine;
using TMPro;
using System.Collections;

public class RoundTimer : MonoBehaviour
{
    public TMP_Text roundTimerText;
    public TMP_Text bigCountdownText;
    public int countdownTime = 99;
    public Color startColor = Color.yellow;
    public Color endColor = Color.red;
    [Range(0f, 1f)]
    public float startAlpha = 0.5f;

    private int currentTime;
    private bool isTimerRunning = false; 
    private Coroutine countdownCoroutine;


    public void StartCountdown()
    {
        countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    public bool IsTimerRunning()
    {
        return isTimerRunning;
    }
    public void StopTimer()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        isTimerRunning = false;
    }

    private IEnumerator CountdownCoroutine()
    {
        isTimerRunning = true;
        currentTime = countdownTime;
        
        roundTimerText.gameObject.SetActive(true); 
        bigCountdownText.gameObject.SetActive(false);
        roundTimerText.text = currentTime.ToString();

        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime--;
            roundTimerText.text = currentTime.ToString();

            if (currentTime == 5)
            {
                if (AudioManager.instance != null) AudioManager.instance.StopMusic();
                if(roundTimerText.gameObject.activeSelf) roundTimerText.gameObject.SetActive(false);
            }
            
            if (currentTime <= 5 && currentTime > 0)
            {
                StartCoroutine(PulseEffectCoroutine(currentTime));
            }
        }
        
        Debug.Log("Time is over.");
        isTimerRunning = false;
    }

    private IEnumerator PulseEffectCoroutine(int number)
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("CountdownTick");
        bigCountdownText.gameObject.SetActive(true);
        bigCountdownText.text = number.ToString();
        float colorLerpT = (5f - number) / 4f; 
        Color baseColor = Color.Lerp(startColor, endColor, colorLerpT);
        float effectDuration = 0.9f; 
        float timer = 0f;
        while (timer < effectDuration)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, 0f, timer / effectDuration);
            bigCountdownText.color = new Color(baseColor.r, baseColor.g, baseColor.b, currentAlpha);
            yield return null;
        }
        bigCountdownText.gameObject.SetActive(false);
    }
}