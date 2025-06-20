using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// SelectableMap class'ı aynı kalıyor.
[System.Serializable]
public class SelectableMap
{
    public string mapName;
    public Sprite mapIcon;
    public GameObject mapPrefab;
}

public class MapSelectManager : MonoBehaviour
{
    [Header("Map List")]
    public List<SelectableMap> mapList;

    [Header("UI References")]
    public List<Image> mapIconImages;
    public RectTransform selectionFrame;
    public TMP_Text mapNameText;
    
    [Header("Input Settings")]
    [Tooltip("Yön tuşuna basılı tutulduğunda ne kadar hızlı seçim yapılacağı")]
    public float navigationCooldown = 0.2f;

    private int currentIndex = 0;
    private float navTimer = 0f;

    // Bu script aktif olduğunda InputManager olaylarına abone ol
    private void OnEnable()
    {
        // Sadece Player 1'in girdilerini dinliyoruz
        InputManager.OnPlayer1Move += HandleNavigation;
        InputManager.OnPlayer1Punch += HandleSelection; // P1'in Punch'ı burada Onaylama tuşu
    }

    // Bu script deaktif olduğunda abonelikleri iptal et
    private void OnDisable()
    {
        InputManager.OnPlayer1Move -= HandleNavigation;
        InputManager.OnPlayer1Punch -= HandleSelection;
    }

    IEnumerator Start()
    {
        if (InputManager.Instance != null)
        {
            // P2'nin bu ekranda bir şey yapmasına gerek olmadığı için SplitScreen yerine normal UI modu yeterli olabilir.
            // Ancak tutarlılık için SplitScreen de kalabilir, bir zararı olmaz.
            InputManager.Instance.EnableSplitScreenUIControls();
        }

        for (int i = 0; i < mapList.Count; i++)
        {
            if (i < mapIconImages.Count)
            {
                mapIconImages[i].sprite = mapList[i].mapIcon;
            }
        }

        yield return new WaitForEndOfFrame();
        UpdateSelectionUI(0);
    }

    private void Update()
    {
        // Sadece navigasyon zamanlayıcısını güncellemek için kullanılıyor
        if (navTimer > 0)
        {
            navTimer -= Time.deltaTime;
        }
    }
    
    // P1'in hareket girdisini işleyen fonksiyon
    private void HandleNavigation(Vector2 moveInput)
    {
        // Zamanlayıcı dolmadan yeni bir hareket algılama
        if (navTimer > 0) return;

        int prevIndex = currentIndex;

        // Sadece yatay hareketi dikkate al
        if (moveInput.x > 0.5f) currentIndex++;
        else if (moveInput.x < -0.5f) currentIndex--;
        else return; // Yatay girdi yoksa çık

        // Index'in sınırlar içinde dönmesini sağla (wrapping)
        if (currentIndex < 0) currentIndex = mapList.Count - 1;
        if (currentIndex >= mapList.Count) currentIndex = mapList.Count > 0 ? currentIndex % mapList.Count : 0;
        
        if (prevIndex != currentIndex)
        {
            UpdateSelectionUI(currentIndex);
            // Zamanlayıcıyı sıfırla ki ardışık hızlı geçişler olmasın
            navTimer = navigationCooldown;
        }
    }
    
    // UI elemanlarını güncelleyen ortak fonksiyon
    private void UpdateSelectionUI(int index)
    {
        currentIndex = index;
        selectionFrame.position = mapIconImages[currentIndex].rectTransform.position;
        mapNameText.text = mapList[currentIndex].mapName;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("ButtonSelect");
        }
    }
    
    // P1'in onaylama (Punch) girdisini işleyen fonksiyon
    private void HandleSelection()
    {
        SelectMapAndStartFight();
    }

    void SelectMapAndStartFight()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        GameManager.instance.selectedMapPrefab = mapList[currentIndex].mapPrefab;
        
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("ButtonClick");
        }

        // Sahne değiştirmeden önce event aboneliklerini temizle
        if(InputManager.Instance != null) InputManager.ClearAllEventSubscriptions();

        SceneManager.LoadScene("FightScene");
    }
}