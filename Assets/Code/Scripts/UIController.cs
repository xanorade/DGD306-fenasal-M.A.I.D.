using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public GameObject titleScreen;
    public GameObject optionsScreen;
    public GameObject creditsScreen;
    
    public void GameStart() {
        SceneManager.LoadScene("Game");
    }

    public void OptionsButtonClicked() {
        titleScreen.SetActive(false);
        optionsScreen.SetActive(true);
    }

    public void CreditsButtonClicked() {
        titleScreen.SetActive(false);
        creditsScreen.SetActive(true);
    }

    public void QuitGame() {
        Application.Quit();
    }
}
