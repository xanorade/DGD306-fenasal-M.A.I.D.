using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    private int currentIndex = 0;
    
    // Add debouncing for navigation input
    private float navigationCooldown = 0.2f;
    private float lastNavigationTime = 0f;

    IEnumerator Start()
    {
        for (int i = 0; i < mapList.Count; i++)
        {
            if (i < mapIconImages.Count)
            {
                mapIconImages[i].sprite = mapList[i].mapIcon;
            }
        }

        yield return new WaitForEndOfFrame();

        UpdateSelection(0);
        
        // Disable EventSystem UI input module to prevent conflicts
        var inputModule = FindObjectOfType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (inputModule != null)
        {
            inputModule.enabled = false;
        }
        
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
        InputManager.OnUINavigate += HandleNavigation;
        InputManager.OnUISubmit += HandleSelection;
    }

    private void UnsubscribeFromInputEvents()
    {
        InputManager.OnUINavigate -= HandleNavigation;
        InputManager.OnUISubmit -= HandleSelection;
    }

    private void HandleNavigation(Vector2 navigation)
    {
        // Add debouncing to prevent rapid repeated inputs
        if (Time.time < lastNavigationTime + navigationCooldown)
        {
            return;
        }
        
        int prevIndex = currentIndex;

        if (navigation.x < -0.5f) // Left
        {
            currentIndex--;
            lastNavigationTime = Time.time;
        }
        else if (navigation.x > 0.5f) // Right
        {
            currentIndex++;
            lastNavigationTime = Time.time;
        }
        else
        {
            return;
        }
        
        // Handle wrapping
        if (currentIndex < 0) currentIndex = mapList.Count - 1;
        if (currentIndex >= mapList.Count) currentIndex = 0;
        
        // Update if index changed
        if (prevIndex != currentIndex)
        {
            UpdateSelection(currentIndex);
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySFX("ButtonSelect");
            }
        }
    }

    void UpdateSelection(int index)
    {
        currentIndex = index;
        selectionFrame.position = mapIconImages[currentIndex].rectTransform.position;
        mapNameText.text = mapList[currentIndex].mapName;
    }

    private void HandleSelection()
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

        // Clear all input event subscriptions before changing scenes
        InputManager.ClearAllEventSubscriptions();

        SceneManager.LoadScene("FightScene");
    }

    private void OnDestroy()
    {
        UnsubscribeFromInputEvents();
        
        // Re-enable EventSystem UI input module when leaving
        var inputModule = FindObjectOfType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (inputModule != null)
        {
            inputModule.enabled = true;
        }
    }
}