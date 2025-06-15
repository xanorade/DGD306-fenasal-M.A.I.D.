using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject playerSelectPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("First Buttons for Navigation")]
    public GameObject mainMenuFirstButton;
    public GameObject playerSelectFirstButton;
    public GameObject optionsPanelFirstButton;
    public GameObject creditsPanelFirstButton;
    
    void Start()
    {
        EventSystem.current.SetSelectedGameObject(mainMenuFirstButton);
    }

    public void PlayButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        playerSelectPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(playerSelectFirstButton);
    }

    public void SelectOnePlayerMode()
    {
        SceneManager.LoadScene("LevelSelect_Scene");
    }

    public void SelectTwoPlayerMode()
    {
        SceneManager.LoadScene("LevelSelect_Scene");
    }

    public void OptionsButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsPanelFirstButton);
    }

    public void CreditsButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(creditsPanelFirstButton);
    }
    
    public void BackToMainMenu(GameObject buttonToSelectOnMain)
    {
        mainMenuPanel.SetActive(true);
        playerSelectPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonToSelectOnMain);
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}