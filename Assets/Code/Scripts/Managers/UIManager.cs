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
    public GameObject instructionsPanel; // YENİ: Talimatlar paneli referansı

    [Header("First Buttons for Navigation")]
    public GameObject mainMenuFirstButton;
    public GameObject playerSelectFirstButton;
    public GameObject optionsPanelFirstButton;
    public GameObject creditsPanelFirstButton;
    public GameObject instructionsPanelFirstButton; // YENİ: Talimatlar panelinin ilk (ve tek) butonu
    public GameObject instructionsButtonOnPlayerSelect; // YENİ: Player Select'teki Talimatlar butonu

    void Start()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.EnableUIControls();
        }
        
        EventSystem.current.SetSelectedGameObject(mainMenuFirstButton);
    }

    public void PlayButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        playerSelectPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(playerSelectFirstButton);
    }

    // YENİ FONKSİYON: Talimatlar butonuna basıldığında
    public void InstructionsButtonClicked()
    {
        playerSelectPanel.SetActive(false);
        instructionsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(instructionsPanelFirstButton);
    }

    public void OptionsButtonClicked()
    {
        // Bu fonksiyonun hangi panelden çağrıldığına göre mantık değişebilir,
        // şimdilik ana menüden geldiğini varsayıyoruz.
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
    
    // YENİ FONKSİYON: Talimatlar panelinden Player Select'e geri dönmek için
    public void BackToPlayerSelect()
    {
        instructionsPanel.SetActive(false);
        playerSelectPanel.SetActive(true);
        // Geri döndüğümüzde kontrolcünün odağını Talimatlar butonuna geri getir
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(instructionsButtonOnPlayerSelect);
    }

    public void BackToMainMenu(GameObject buttonToSelectOnMain)
    {
        mainMenuPanel.SetActive(true);
        playerSelectPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        instructionsPanel.SetActive(false); // Yeni paneli de burada gizlediğimizden emin olalım
        
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonToSelectOnMain);
    }
    
    public void SelectTwoPlayerMode()
    {
        InputManager.ClearAllEventSubscriptions();
        SceneManager.LoadScene("CharacterSelectScene");
    } 
    public void QuitGame()
    {
        Application.Quit();
    } 
}