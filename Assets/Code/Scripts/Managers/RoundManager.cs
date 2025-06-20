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
    
    // Game Over UI state
    private bool isGameOverUIActive = false;
    private int gameOverButtonIndex = 0; // 0 = Rematch, 1 = Quit to Title
    private const int totalGameOverButtons = 2;

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

    private void Start()
    {
        // Subscribe to input events for game over UI
        SubscribeToInputEvents();
    }

    private void SubscribeToInputEvents()
    {
        Debug.Log("RoundManager: Subscribing to input events");
        
        // Subscribe to UI navigation and submit events
        InputManager.OnUINavigate += HandleGameOverNavigation;
        InputManager.OnUISubmit += HandleGameOverSubmit;
        
        // Also subscribe to individual player inputs for game over selection
        InputManager.OnPlayer1Punch += HandlePlayer1GameOverSelection;
        InputManager.OnPlayer2Punch += HandlePlayer2GameOverSelection;
        
        Debug.Log("RoundManager: Input events subscribed");
    }

    private void UnsubscribeFromInputEvents()
    {
        Debug.Log("RoundManager: Unsubscribing from input events");
        
        InputManager.OnUINavigate -= HandleGameOverNavigation;
        InputManager.OnUISubmit -= HandleGameOverSubmit;
        InputManager.OnPlayer1Punch -= HandlePlayer1GameOverSelection;
        InputManager.OnPlayer2Punch -= HandlePlayer2GameOverSelection;
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
        
        if (p1Wins == 1 && p2Wins == 1)
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
            if (p1Wins == 1 && p2Wins == 1)
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
            p2Wins++;
            player1WinDisplay.UpdateWinIcons(p1Wins);
            player2WinDisplay.UpdateWinIcons(p2Wins);
            Debug.Log("DRAW!");
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

    void HandleMatchOver(int winnerPlayerIndex)
    {
        Debug.Log("MATCH OVER!");
        Debug.Log("PLAYER " + winnerPlayerIndex + " WINS - Going to main menu");
        
        // Skip the game over UI and go directly to main menu after a short delay
        StartCoroutine(GoToMainMenuAfterDelay());
    }
    
    private IEnumerator GoToMainMenuAfterDelay()
    {
        // Wait a short moment to let players see the victory animation
        yield return new WaitForSeconds(1f);
        
        // Clear all input event subscriptions before changing scenes
        InputManager.ClearAllEventSubscriptions();
        
        // Ensure input system is properly reset for UI navigation
        if (InputManager.Instance != null)
        {
            InputManager.Instance.EnableUIControls();
        }
        
        // Go directly to main menu
        SceneManager.LoadScene("TitleScene");
    }

    private void HandleGameOverNavigation(Vector2 navigation)
    {
        if (!isGameOverUIActive) return;
        
        Debug.Log($"RoundManager: HandleGameOverNavigation called with {navigation}");
        
        // Use higher threshold to avoid accidental navigation
        float threshold = 0.7f;
        int prevIndex = gameOverButtonIndex;

        // Handle vertical navigation (up/down)
        if (navigation.y > threshold) // Up
        {
            gameOverButtonIndex--;
        }
        else if (navigation.y < -threshold) // Down
        {
            gameOverButtonIndex++;
        }
        
        // Wrap around
        if (gameOverButtonIndex < 0) gameOverButtonIndex = totalGameOverButtons - 1;
        if (gameOverButtonIndex >= totalGameOverButtons) gameOverButtonIndex = 0;
        
        if (prevIndex != gameOverButtonIndex)
        {
            UpdateGameOverButtonHighlight();
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX("ButtonSelect");
            }
        }
    }

    private void UpdateGameOverButtonHighlight()
    {
        // Update EventSystem selection based on current index
        GameObject buttonToSelect = (gameOverButtonIndex == 0) ? rematchButton : 
                                   rematchButton.transform.parent.Find("Quit To Main Menu")?.gameObject;
        
        if (buttonToSelect != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buttonToSelect);
        }
    }

    private void HandleGameOverSubmit()
    {
        if (!isGameOverUIActive) return;
        
        Debug.Log("RoundManager: HandleGameOverSubmit called");
        
        ExecuteGameOverSelection();
    }

    private void HandlePlayer1GameOverSelection()
    {
        if (!isGameOverUIActive) return;
        
        Debug.Log("RoundManager: HandlePlayer1GameOverSelection called");
        
        ExecuteGameOverSelection();
    }

    private void HandlePlayer2GameOverSelection()
    {
        if (!isGameOverUIActive) return;
        
        Debug.Log("RoundManager: HandlePlayer2GameOverSelection called");
        
        ExecuteGameOverSelection();
    }

    private void ExecuteGameOverSelection()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("ButtonClick");
        }
        
        if (gameOverButtonIndex == 0)
        {
            Rematch();
        }
        else
        {
            QuitToTitle();
        }
    }
    
    public void Rematch()
    {
        isGameOverUIActive = false;
        
        // Clear all input event subscriptions before changing scenes
        InputManager.ClearAllEventSubscriptions();
        
        SceneManager.LoadScene("CharacterSelectScene");
    }

    public void QuitToTitle()
    {
        isGameOverUIActive = false;
        
        // Clear all input event subscriptions before changing scenes
        InputManager.ClearAllEventSubscriptions();
        
        SceneManager.LoadScene("TitleScene");
    }

    private void OnDestroy()
    {
        UnsubscribeFromInputEvents();
    }
}