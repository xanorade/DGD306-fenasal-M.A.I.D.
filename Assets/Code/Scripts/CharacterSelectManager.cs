using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SelectableCharacter
{
    public Sprite characterIcon;
    public GameObject characterPrefab;
}

public class CharacterSelectManager : MonoBehaviour
{
    private enum SelectionState { P1_Choosing, P2_Choosing, SelectionComplete }
    private SelectionState currentState;

    [Header("Character List")]
    public List<SelectableCharacter> characterList;

    [Header("UI References")]
    public List<Image> characterIconImages;
    public RectTransform p1Frame;
    public RectTransform p2Frame;
    public TMP_Text instructionText;

    private int p1_currentIndex = 0;
    private int p2_currentIndex = 1;

    IEnumerator Start()
    {
        for (int i = 0; i < characterList.Count; i++)
        {
            if (i < characterIconImages.Count)
            {
                characterIconImages[i].sprite = characterList[i].characterIcon;
            }
        }

        yield return new WaitForEndOfFrame();
        
        currentState = SelectionState.P1_Choosing;
        instructionText.text = "PLAYER 1: CHOOSE YOUR MAID";
        p2Frame.gameObject.SetActive(false);
        UpdateFramePosition(1, p1_currentIndex);
        
        // Enable UI controls for menu navigation
        if (InputManager.Instance != null)
        {
            InputManager.Instance.EnableUIControls();
        }
        
        // Subscribe to input events
        SubscribeToInputEvents();
    }

    private void SubscribeToInputEvents()
    {
        // Subscribe to UI navigation for both players
        InputManager.OnUINavigate += HandleNavigation;
        InputManager.OnUISubmit += HandleSubmit;
        
        // Also subscribe to individual player inputs for selection
        InputManager.OnPlayer1Punch += HandlePlayer1Selection;
        InputManager.OnPlayer2Punch += HandlePlayer2Selection;
    }

    private void UnsubscribeFromInputEvents()
    {
        InputManager.OnUINavigate -= HandleNavigation;
        InputManager.OnUISubmit -= HandleSubmit;
        InputManager.OnPlayer1Punch -= HandlePlayer1Selection;
        InputManager.OnPlayer2Punch -= HandlePlayer2Selection;
    }

    private void HandleNavigation(Vector2 navigation)
    {
        if (currentState == SelectionState.SelectionComplete) return;

        if (currentState == SelectionState.P1_Choosing)
        {
            HandlePlayerNavigation(ref p1_currentIndex, navigation);
            UpdateFramePosition(1, p1_currentIndex);
        }
        else if (currentState == SelectionState.P2_Choosing)
        {
            HandlePlayerNavigation(ref p2_currentIndex, navigation);
            UpdateFramePosition(2, p2_currentIndex);
        }
    }

    private void HandlePlayerNavigation(ref int currentIndex, Vector2 navigation)
    {
        int prevIndex = currentIndex;

        // Convert navigation vector to grid movement
        if (navigation.y > 0.5f) currentIndex -= 2; // Up
        if (navigation.y < -0.5f) currentIndex += 2; // Down
        if (navigation.x < -0.5f) currentIndex--; // Left
        if (navigation.x > 0.5f) currentIndex++; // Right
        
        // Handle wrapping
        if (currentIndex < 0) currentIndex = characterList.Count + currentIndex;
        if (currentIndex >= characterList.Count) currentIndex = currentIndex % characterList.Count;

        if (prevIndex != currentIndex)
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX("ButtonSelect");
            }
        }
    }

    private void UpdateFramePosition(int playerIndex, int characterIndex)
    {
        RectTransform targetFrame = (playerIndex == 1) ? p1Frame : p2Frame;
        targetFrame.position = characterIconImages[characterIndex].rectTransform.position;
    }

    private void HandleSubmit()
    {
        if (currentState == SelectionState.P1_Choosing)
        {
            SelectCharacter(1, p1_currentIndex);
        }
        else if (currentState == SelectionState.P2_Choosing)
        {
            SelectCharacter(2, p2_currentIndex);
        }
    }

    private void HandlePlayer1Selection()
    {
        if (currentState == SelectionState.P1_Choosing)
        {
            SelectCharacter(1, p1_currentIndex);
        }
    }

    private void HandlePlayer2Selection()
    {
        if (currentState == SelectionState.P2_Choosing)
        {
            SelectCharacter(2, p2_currentIndex);
        }
    }

    void SelectCharacter(int playerIndex, int characterIndex)
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        if (playerIndex == 1)
        {
            GameManager.instance.player1Prefab = characterList[characterIndex].characterPrefab;
            Debug.Log("Player 1 selected: " + characterList[characterIndex].characterPrefab.name);

            currentState = SelectionState.P2_Choosing;
            instructionText.text = "PLAYER 2: CHOOSE YOUR MAID";
            p2Frame.gameObject.SetActive(true);
            UpdateFramePosition(2, p2_currentIndex);
        }
        else // Player 2
        {
            GameManager.instance.player2Prefab = characterList[characterIndex].characterPrefab;
            Debug.Log("Player 2 selected: " + characterList[characterIndex].characterPrefab.name);

            currentState = SelectionState.SelectionComplete;
            instructionText.text = "GET READY TO FIGHT!";
            StartCoroutine(StartFightCoroutine());
        }
        
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("ButtonClick");
        }
    }

    private IEnumerator StartFightCoroutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("MapSelectScene"); 
    }

    private void OnDestroy()
    {
        UnsubscribeFromInputEvents();
    }
}