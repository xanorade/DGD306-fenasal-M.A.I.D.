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
    }

    void Update()
    {
        if (currentState == SelectionState.SelectionComplete) return;

        if (currentState == SelectionState.P1_Choosing)
        {
            HandleNavigation(ref p1_currentIndex, 1);
            HandleSelection(1);
        }
        else
        {
            HandleNavigation(ref p2_currentIndex, 2);
            HandleSelection(2);
        }
    }

    void HandleNavigation(ref int currentIndex, int playerIndex)
    {
        KeyCode up = (playerIndex == 1) ? KeyCode.W : KeyCode.UpArrow;
        KeyCode down = (playerIndex == 1) ? KeyCode.S : KeyCode.DownArrow;
        KeyCode left = (playerIndex == 1) ? KeyCode.A : KeyCode.LeftArrow;
        KeyCode right = (playerIndex == 1) ? KeyCode.D : KeyCode.RightArrow;
        
        int prevIndex = currentIndex;

        if (Input.GetKeyDown(up)) currentIndex -= 2;
        if (Input.GetKeyDown(down)) currentIndex += 2;
        if (Input.GetKeyDown(left)) currentIndex--;
        if (Input.GetKeyDown(right)) currentIndex++;
        
        if (currentIndex < 0) currentIndex = characterList.Count + currentIndex;
        if (currentIndex >= characterList.Count) currentIndex = currentIndex % characterList.Count;

        if (prevIndex != currentIndex)
        {
            UpdateFramePosition(playerIndex, currentIndex);
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX("ButtonSelect");
            }
        }
    }

    void UpdateFramePosition(int playerIndex, int characterIndex)
    {
        RectTransform targetFrame = (playerIndex == 1) ? p1Frame : p2Frame;
        targetFrame.position = characterIconImages[characterIndex].rectTransform.position;
    }

    void HandleSelection(int playerIndex)
    {
        KeyCode selectKey = (playerIndex == 1) ? KeyCode.J : KeyCode.Keypad1;

        if (Input.GetKeyDown(selectKey))
        {
            SelectCharacter(playerIndex, (playerIndex == 1) ? p1_currentIndex : p2_currentIndex);
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
        SceneManager.LoadScene("LevelSelect_Scene");
    }
}