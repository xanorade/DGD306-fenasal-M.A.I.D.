using UnityEngine;
using TMPro;
using System.Collections;
using DGD306.Character;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class RoundManager : MonoBehaviour
{
    [Header("Round Settings")]
    public int totalRoundsToWin = 2;
    public float timeBetweenRounds = 5f;

    [Header("References")]
    public RoundTimer roundTimer;
    public TMP_Text roundAnnouncerText;
    
    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    public GameObject rematchButton;
    
    [Header("Win Display UI")]
    public RoundWinDisplay player1WinDisplay;
    public RoundWinDisplay player2WinDisplay;

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

        gameOverPanel.SetActive(false);
        
        player1WinDisplay.ResetIcons();
        player2WinDisplay.ResetIcons();
        
        StartCoroutine(StartRoundCoroutine());
    }

    private IEnumerator StartRoundCoroutine()
    {
        currentRound++;
        isRoundOver = false;

        player1.ResetHealth();
        player2.ResetHealth();
        player1.transform.position = p1StartPos.position;
        player2.transform.position = p2StartPos.position;
        player1.FlipCharacter(true);

        roundAnnouncerText.gameObject.SetActive(true);
        if (currentRound == totalRoundsToWin)
        {
            roundAnnouncerText.text = "Final Round";
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX("AnnouncerFinalRound");
        }
        else
        {
            roundAnnouncerText.text = "Round " + currentRound;
            if (AudioManager.instance != null)
            {
                if (currentRound == 1) AudioManager.instance.PlaySFX("AnnouncerRoundOne");
                else if (currentRound == 2) AudioManager.instance.PlaySFX("AnnouncerRoundTwo");
            }
        }
        
        yield return new WaitForSeconds(2f);
        roundAnnouncerText.gameObject.SetActive(false);

        if (AudioManager.instance != null)
        {
            if (currentRound == totalRoundsToWin)
            {
                AudioManager.instance.PlayMusic("FinalRoundMusic");
            }
            else
            {
                AudioManager.instance.PlayMusic("NormalRoundMusic");
            }
        }

        roundTimer.StartCountdown();
        StartCoroutine(PlayRoundCoroutine());
    }

    private IEnumerator PlayRoundCoroutine()
    {
        while (!isRoundOver)
        {
            if (!roundTimer.IsTimerRunning())
            {
                HandleTimeUp();
                break;
            }
            yield return null;
        }
    }

    private IEnumerator EndRoundSequence()
    {
        yield return new WaitForSeconds(timeBetweenRounds);

        if (p1Wins >= totalRoundsToWin || p2Wins >= totalRoundsToWin)
        {
            HandleMatchOver(p1Wins > p2Wins ? 1 : 2);
        }
        else
        {
            StartCoroutine(StartRoundCoroutine());
        }
    }

    private void HandleTimeUp()
    {
        if (isRoundOver) return;
        isRoundOver = true;
        roundTimer.StopTimer();
        if (AudioManager.instance != null) AudioManager.instance.StopMusic();

        Debug.Log("Time is up, checking health...");

        if (player1.CurrentHealth > player2.CurrentHealth)
        {
            p1Wins++;
            player1WinDisplay.UpdateWinIcons(p1Wins);
            player1.TriggerWin();
        }
        else if (player2.CurrentHealth > player1.CurrentHealth)
        {
            p2Wins++;
            player2WinDisplay.UpdateWinIcons(p2Wins); 
            player2.TriggerWin();
        }
        else
        {
            p1Wins++;
            player1WinDisplay.UpdateWinIcons(p1Wins);
            player1.TriggerWin();
        }
        StartCoroutine(EndRoundSequence());
    }

    public void OnFighterDefeated(FighterController defeatedFighter)
    {
        if (isRoundOver) return;
        isRoundOver = true;
        roundTimer.StopTimer();
        if (AudioManager.instance != null) AudioManager.instance.StopMusic();

        if (defeatedFighter == player1)
        {
            p2Wins++;
            player2WinDisplay.UpdateWinIcons(p2Wins); // YENİ
            player2.TriggerWin();
        }
        else
        {
            p1Wins++;
            player1WinDisplay.UpdateWinIcons(p1Wins); // YENİ
            player1.TriggerWin();   
        }
        StartCoroutine(EndRoundSequence());
    }

    void HandleMatchOver(int winnerPlayerIndex)
    {
        Debug.Log("MATCH OVER!");
        winnerText.text = "PLAYER " + winnerPlayerIndex + " WINS";
        gameOverPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(rematchButton);
    }
    
    public void Rematch()
    {
        SceneManager.LoadScene("CharacterSelectScene");
    }

    public void QuitToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}