using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    public Image healthBarFillImage;

    public FighterController targetFighter;

    public float healthDropSpeed = 0.5f;

    private Coroutine healthUpdateCoroutine;
    
    public void Initialize()
    {
        if (targetFighter == null)
        {
            gameObject.SetActive(false);
            return;
        }

        targetFighter.OnHealthChanged += UpdateHealthBar;

        UpdateHealthBar(targetFighter.CurrentHealth, targetFighter.MaxHealth);
    }
    
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float targetFillAmount = currentHealth / maxHealth;

        if (healthUpdateCoroutine != null)
        {
            StopCoroutine(healthUpdateCoroutine);
        }
        healthUpdateCoroutine = StartCoroutine(SmoothHealthChange(targetFillAmount));
    }

    private IEnumerator SmoothHealthChange(float newFillAmount)
    {
        float currentFill = healthBarFillImage.fillAmount;
        float timer = 0f;

        while (timer < healthDropSpeed)
        {
            timer += Time.deltaTime;
            healthBarFillImage.fillAmount = Mathf.Lerp(currentFill, newFillAmount, timer / healthDropSpeed);
            yield return null;
        }
        
        healthBarFillImage.fillAmount = newFillAmount;
    }
    
    private void OnDestroy()
    {
        if (targetFighter != null)
        {
            targetFighter.OnHealthChanged -= UpdateHealthBar;
        }
    }
}