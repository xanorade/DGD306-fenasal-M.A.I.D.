using UnityEngine;
using System.Collections;

public class AutoScrollCredits : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform creditsTextTransform;
    
    [Header("Scroll Settings")]
    public float timeBetweenSteps = 0.016f;
    public float stepAmount = 1f;
    public Vector2 startPosition;

    public UIManager uiManager;
    public GameObject buttonToSelectOnMain; 

    private Coroutine scrollCoroutine;

    void OnEnable()
    {
        creditsTextTransform.anchoredPosition = startPosition;
        scrollCoroutine = StartCoroutine(ScrollText());
    }

    void OnDisable()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            GoBackToMenu();
        }
    }

    private IEnumerator ScrollText()
    {
        float endYPosition = GetComponent<RectTransform>().rect.height + (creditsTextTransform.rect.height / 2);
        float timer = 0f;

        while (creditsTextTransform.anchoredPosition.y < endYPosition)
        {
            timer += Time.deltaTime;

            if (timer >= timeBetweenSteps)
            {
                creditsTextTransform.Translate(Vector3.up * stepAmount);
                
                timer -= timeBetweenSteps;
            }

            yield return null;
        }
        
        GoBackToMenu();
    }
    
    public void GoBackToMenu()
    {
        if (uiManager != null)
        {
            uiManager.BackToMainMenu(buttonToSelectOnMain);
        }
    }
}