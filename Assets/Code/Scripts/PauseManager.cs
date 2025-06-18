using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;
    public GameObject pauseMenuFirstButton;

    public static bool IsGamePaused = false;

    void Start()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
        
        // Subscribe to pause input
        SubscribeToInputEvents();
    }

    private void SubscribeToInputEvents()
    {
        InputManager.OnPause += HandlePauseInput;
    }

    private void UnsubscribeFromInputEvents()
    {
        InputManager.OnPause -= HandlePauseInput;
    }

    private void HandlePauseInput()
    {
        if (IsGamePaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        IsGamePaused = false;
        
        // Re-enable gameplay controls
        if (InputManager.Instance != null)
        {
            InputManager.Instance.EnableGameplayControls();
        }
    }

    void Pause()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        IsGamePaused = true;

        // Enable UI controls for pause menu
        if (InputManager.Instance != null)
        {
            InputManager.Instance.EnableUIControls();
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(pauseMenuFirstButton);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        IsGamePaused = false;
        
        SceneManager.LoadScene("TitleScene"); 
    }

    private void OnDestroy()
    {
        UnsubscribeFromInputEvents();
    }
}