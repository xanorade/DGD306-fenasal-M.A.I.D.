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
    }

    void Update()
    {
        HandleNavigation();
        HandleSelection();
    }

    void HandleNavigation()
    {
        int prevIndex = currentIndex;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex++;
        }
        
        if (prevIndex != currentIndex)
        {
            if (currentIndex < 0) currentIndex = mapList.Count - 1;
            if (currentIndex >= mapList.Count) currentIndex = 0;
            
            UpdateSelection(currentIndex);
            AudioManager.instance.PlaySFX("ButtonSelect");
        }
    }

    void UpdateSelection(int index)
    {
        currentIndex = index;
        selectionFrame.position = mapIconImages[currentIndex].rectTransform.position;
        mapNameText.text = mapList[currentIndex].mapName;
    }

    void HandleSelection()
    {
        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Return))
        {
            SelectMapAndStartFight();
        }
    }

    void SelectMapAndStartFight()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }

        GameManager.instance.selectedMapPrefab = mapList[currentIndex].mapPrefab;

        // Bu iki debug satırı çok önemli
        Debug.Log("MAP SELECT SCENE: GameManager'a atanan prefab: " + (GameManager.instance.selectedMapPrefab != null ? GameManager.instance.selectedMapPrefab.name : "NULL"));
        Debug.Log("MAP SELECT SCENE: GameManager instance ID: " + GameManager.instance.GetInstanceID());

        AudioManager.instance.PlaySFX("ButtonClick");
        SceneManager.LoadScene("FightScene");
    }
}