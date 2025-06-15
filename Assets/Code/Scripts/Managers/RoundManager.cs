using UnityEngine;
using TMPro;
using System.Collections;
using DGD306.Character;

public class RoundManager : MonoBehaviour
{
    [Header("Raunt Ayarları")]
    public int totalRounds = 3;
    public float timeBetweenRounds = 5f;

    [Header("Referanslar")]
    public RoundTimer roundTimer;
    public TMP_Text roundAnnouncerText;

    private int currentRound = 0;
    private int p1Wins = 0;
    private int p2Wins = 0;
    private FighterController player1;
    private FighterController player2;
    private Transform p1StartPos;
    private Transform p2StartPos;
    private bool isRoundOver = false;

    public void Initialize(FighterController p1, FighterController p2, Transform p1Spawn, Transform p2Spawn)
    {
        player1 = p1;
        player2 = p2;
        player1.roundManager = this;
        player2.roundManager = this;
        p1StartPos = p1Spawn;
        p2StartPos = p2Spawn;

        StartCoroutine(GameFlowCoroutine());
    }

    private IEnumerator GameFlowCoroutine()
    {
        while (currentRound < totalRounds)
        {
            yield return StartCoroutine(RoundStartCoroutine());
            yield return StartCoroutine(RoundPlayCoroutine());
            yield return StartCoroutine(RoundEndCoroutine());
        }
        Debug.Log("GAME OVER!");
    }

    private IEnumerator RoundStartCoroutine()
    {
        currentRound++;
        isRoundOver = false;
        
        player1.ResetHealth();
        player2.ResetHealth();
        player1.transform.position = p1StartPos.position;
        player2.transform.position = p2StartPos.position;
        player1.FlipCharacter(true); 

        roundAnnouncerText.gameObject.SetActive(true);
        if (currentRound == totalRounds)
        {
            roundAnnouncerText.text = "Final Round";
            AudioManager.instance.PlaySFX("AnnouncerFinalRound");
        }
        else
        {
            roundAnnouncerText.text = "Round " + currentRound;
            if(currentRound == 1) AudioManager.instance.PlaySFX("AnnouncerRoundOne");
            else if (currentRound == 2) AudioManager.instance.PlaySFX("AnnouncerRoundTwo");
        }
        
        yield return new WaitForSeconds(2f);
        roundAnnouncerText.gameObject.SetActive(false);

        if (currentRound == totalRounds)
        {
            AudioManager.instance.PlayMusic("FinalRoundMusic");
        }
        else
        {
            AudioManager.instance.PlayMusic("NormalRoundMusic");
        }

        roundTimer.StartCountdown();
    }

    private IEnumerator RoundPlayCoroutine()
    {
        while (roundTimer.IsTimerRunning() && !isRoundOver)
        {
            yield return null;
        }

        if (isRoundOver) yield break;

        isRoundOver = true;
        roundTimer.StopTimer();
        AudioManager.instance.StopMusic();
        Debug.Log("Time is over. Checking for healths...");

        if (player1.CurrentHealth > player2.CurrentHealth)
        {
            player1.TriggerWin();
            p1Wins++;
        }
        else if (player2.CurrentHealth > player1.CurrentHealth)
        {
            player2.TriggerWin();
            p2Wins++;
        }
        else
        {
            Debug.Log("DRAW!");
        }
    }

    private IEnumerator RoundEndCoroutine()
    {
        yield return new WaitForSeconds(timeBetweenRounds);
    }

    public void OnFighterDefeated(FighterController defeatedFighter)
    {
        if (isRoundOver) return;

        isRoundOver = true;
        roundTimer.StopTimer();
        AudioManager.instance.StopMusic();

        if (defeatedFighter == player1)
        {
            player2.TriggerWin();
            p2Wins++;
        }
        else
        {
            player1.TriggerWin();
            p1Wins++;
        }
    }
}