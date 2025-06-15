using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RoundWinDisplay : MonoBehaviour
{
    [Header("UI References")]
    public List<GameObject> winIconObjects;

    void Start()
    {
        ResetIcons();
    }

    public void UpdateWinIcons(int wins)
    {
        for (int i = 0; i < winIconObjects.Count; i++)
        {
            if (i < wins)
            {
                winIconObjects[i].SetActive(true);
            }
            else
            {
                winIconObjects[i].SetActive(false);
            }
        }
    }

    public void ResetIcons()
    {
        UpdateWinIcons(0);
    }
}