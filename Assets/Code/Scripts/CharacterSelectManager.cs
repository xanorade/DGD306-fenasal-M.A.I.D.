using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// SelectableCharacter class'ı aynı kalıyor
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

    [Header("Input Settings")]
    [Tooltip("Yön tuşuna basılı tutulduğunda ne kadar hızlı seçim yapılacağı")]
    public float navigationCooldown = 0.25f;

    private int p1_currentIndex = 0;
    private int p2_currentIndex = 1; 
    private float p1_navTimer = 0f;
    private float p2_navTimer = 0f;

    // Bu script aktif olduğunda olaylara abone ol
    private void OnEnable()
    {
        InputManager.OnPlayer1Move += HandlePlayer1Navigation;
        InputManager.OnPlayer1Punch += HandlePlayer1Selection; // Punch = Onaylama olarak kullanıyoruz

        InputManager.OnPlayer2Move += HandlePlayer2Navigation;
        InputManager.OnPlayer2Punch += HandlePlayer2Selection; // Punch = Onaylama olarak kullanıyoruz
    }

    // Bu script deaktif olduğunda abonelikleri iptal et
    private void OnDisable()
    {
        InputManager.OnPlayer1Move -= HandlePlayer1Navigation;
        InputManager.OnPlayer1Punch -= HandlePlayer1Selection;
        
        InputManager.OnPlayer2Move -= HandlePlayer2Navigation;
        InputManager.OnPlayer2Punch -= HandlePlayer2Selection;
    }
    
    IEnumerator Start()
    {
        if (InputManager.Instance != null)
        {
            // Artık bu yeni fonksiyonu çağırıyoruz
            InputManager.Instance.EnableSplitScreenUIControls();
        }
        // UI ikonlarını listedeki karakterlere göre doldur
        for (int i = 0; i < characterList.Count; i++)
        {
            if (i < characterIconImages.Count)
                characterIconImages[i].sprite = characterList[i].characterIcon;
        }

        yield return new WaitForEndOfFrame();
        
        // Başlangıç durumu
        currentState = SelectionState.P1_Choosing;
        instructionText.text = "PLAYER 1: CHOOSE YOUR MAID";
        p2Frame.gameObject.SetActive(false);
        UpdateFramePosition(1, p1_currentIndex);
    }
    
    // P1 Navigasyon Olayını İşleyen Fonksiyon
    private void HandlePlayer1Navigation(Vector2 moveInput)
    {
        if (currentState != SelectionState.P1_Choosing) return;
        
        // Zamanlayıcı kullanarak hızlı geçişleri engelle
        if (p1_navTimer <= 0)
        {
            if(UpdateIndexFromInput(ref p1_currentIndex, moveInput))
            {
                UpdateFramePosition(1, p1_currentIndex);
                p1_navTimer = navigationCooldown;
            }
        }
    }
    
    // P2 Navigasyon Olayını İşleyen Fonksiyon
    private void HandlePlayer2Navigation(Vector2 moveInput)
    {
        if (currentState != SelectionState.P2_Choosing) return;

        if (p2_navTimer <= 0)
        {
            if(UpdateIndexFromInput(ref p2_currentIndex, moveInput))
            {
                UpdateFramePosition(2, p2_currentIndex);
                p2_navTimer = navigationCooldown;
            }
        }
    }

    // Update fonksiyonu sadece zamanlayıcıları güncellemek için var
    private void Update()
    {
        if (p1_navTimer > 0) p1_navTimer -= Time.deltaTime;
        if (p2_navTimer > 0) p2_navTimer -= Time.deltaTime;
    }

    // Ortak index güncelleme mantığı
    private bool UpdateIndexFromInput(ref int currentIndex, Vector2 moveInput)
    {
        int prevIndex = currentIndex;
        
        if (moveInput.x > 0.5f) currentIndex++;
        else if (moveInput.x < -0.5f) currentIndex--;
        else if (moveInput.y > 0.5f) currentIndex -= 2; // 2x2 grid için
        else if (moveInput.y < -0.5f) currentIndex += 2; // 2x2 grid için
        else return false; // Girdi yoksa çık

        // Index'in sınırlar içinde kalmasını sağla (wrapping)
        if (currentIndex < 0) currentIndex = characterList.Count - 1;
        if (currentIndex >= characterList.Count) currentIndex = 0;
        
        if (prevIndex != currentIndex && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("ButtonSelect");
        }
        
        return prevIndex != currentIndex;
    }

    private void UpdateFramePosition(int playerIndex, int characterIndex)
    {
        RectTransform targetFrame = (playerIndex == 1) ? p1Frame : p2Frame;
        targetFrame.position = characterIconImages[characterIndex].rectTransform.position;
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
        // ... (Bu fonksiyonun içeriği aynı kalıyor)
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        if (playerIndex == 1)
        {
            GameManager.instance.player1Prefab = characterList[characterIndex].characterPrefab;
            currentState = SelectionState.P2_Choosing;
            instructionText.text = "PLAYER 2: CHOOSE YOUR MAID";
            p2Frame.gameObject.SetActive(true);
            UpdateFramePosition(2, p2_currentIndex);
        }
        else
        {
            GameManager.instance.player2Prefab = characterList[characterIndex].characterPrefab;
            currentState = SelectionState.SelectionComplete;
            instructionText.text = "GET READY TO FIGHT!";
            StartCoroutine(StartFightCoroutine());
        }
        
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX("ButtonClick");
    }

    private IEnumerator StartFightCoroutine()
    {
        yield return new WaitForSeconds(2f);
        if(InputManager.Instance != null) InputManager.ClearAllEventSubscriptions();
        SceneManager.LoadScene("MapSelectScene");
    }
}