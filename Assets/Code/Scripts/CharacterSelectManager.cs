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

    [Header("Karakter Listesi")]
    public List<SelectableCharacter> characterList;

    [Header("UI Referansları")]
    public List<Image> characterIconImages;
    public RectTransform p1Frame;
    public RectTransform p2Frame;
    public TMP_Text instructionText;

    private int p1_currentIndex = 0;
    private int p2_currentIndex = 1; // P2'nin farklı bir yerden başlaması için

    void Start()
    {
        // UI ikonlarını listedeki karakterlere göre doldur
        for (int i = 0; i < characterList.Count; i++)
        {
            if (i < characterIconImages.Count)
            {
                characterIconImages[i].sprite = characterList[i].characterIcon;
            }
        }

        // Başlangıç durumu
        currentState = SelectionState.P1_Choosing;
        instructionText.text = "PLAYER 1: CHOOSE YOUR FIGHTER";
        p2Frame.gameObject.SetActive(false); // Başlangıçta P2 çerçevesi gizli
        UpdateFramePosition(1, p1_currentIndex);
    }

    void Update()
    {
        // Eğer seçimler tamamlandıysa input dinleme
        if (currentState == SelectionState.SelectionComplete) return;

        // Hangi oyuncunun sırasıysa onun input'unu dinle
        if (currentState == SelectionState.P1_Choosing)
        {
            HandleNavigation(ref p1_currentIndex, 1);
            HandleSelection(1);
        }
        else // P2_Choosing
        {
            HandleNavigation(ref p2_currentIndex, 2);
            HandleSelection(2);
        }
    }

    void HandleNavigation(ref int currentIndex, int playerIndex)
    {
        // Player 1 WASD, Player 2 Yön Tuşları
        KeyCode up = (playerIndex == 1) ? KeyCode.W : KeyCode.UpArrow;
        KeyCode down = (playerIndex == 1) ? KeyCode.S : KeyCode.DownArrow;
        KeyCode left = (playerIndex == 1) ? KeyCode.A : KeyCode.LeftArrow;
        KeyCode right = (playerIndex == 1) ? KeyCode.D : KeyCode.RightArrow;
        
        int prevIndex = currentIndex;

        if (Input.GetKeyDown(up)) currentIndex -= 2;
        if (Input.GetKeyDown(down)) currentIndex += 2;
        if (Input.GetKeyDown(left)) currentIndex--;
        if (Input.GetKeyDown(right)) currentIndex++;
        
        // Index'in sınırlar içinde kalmasını ve dönmesini sağla (Wrapping)
        if (currentIndex < 0) currentIndex = characterList.Count + currentIndex;
        if (currentIndex >= characterList.Count) currentIndex = currentIndex % characterList.Count;

        // Eğer index değiştiyse çerçeveyi güncelle ve ses çal
        if (prevIndex != currentIndex)
        {
            UpdateFramePosition(playerIndex, currentIndex);
            AudioManager.instance.PlaySFX("ButtonSelect"); 
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
            Debug.LogError("GameManager bulunamadı!");
            return;
        }

        // Seçimi GameManager'a kaydet
        if (playerIndex == 1)
        {
            GameManager.instance.player1Prefab = characterList[characterIndex].characterPrefab;
            Debug.Log("Player 1 seçti: " + characterList[characterIndex].characterPrefab.name);

            // Sırayı Player 2'ye geçir
            currentState = SelectionState.P2_Choosing;
            instructionText.text = "PLAYER 2: CHOOSE YOUR FIGHTER";
            p2Frame.gameObject.SetActive(true);
            UpdateFramePosition(2, p2_currentIndex);
        }
        else // Player 2
        {
            GameManager.instance.player2Prefab = characterList[characterIndex].characterPrefab;
            Debug.Log("Player 2 seçti: " + characterList[characterIndex].characterPrefab.name);

            // Seçimler tamamlandı
            currentState = SelectionState.SelectionComplete;
            instructionText.text = "GET READY TO FIGHT!";
            StartCoroutine(StartFightCoroutine());
        }
        
        AudioManager.instance.PlaySFX("ButtonClick");
    }

    private IEnumerator StartFightCoroutine()
    {
        // "FIGHT!" demeden önce kısa bir bekleme
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("LevelSelect_Scene");
    }
}