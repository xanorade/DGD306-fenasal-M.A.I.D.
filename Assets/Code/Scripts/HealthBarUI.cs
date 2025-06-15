using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    [Tooltip("Doldurulacak olan Image bileşeni")]
    public Image healthBarFillImage;
    [Tooltip("Can azaldığında ne kadar sürede yumuşakça düşeceği")]
    public float healthDropSpeed = 0.5f;

    // Artık public değil, dışarıdan atanacak
    private FighterController targetFighter;
    private Coroutine healthUpdateCoroutine;

    // Dışarıdan çağrılacak olan başlatma fonksiyonu
    public void Initialize(FighterController fighter)
    {
        targetFighter = fighter;
        
        if (targetFighter == null)
        {
            Debug.LogError("HealthBarUI için bir hedef karakter (targetFighter) atanamadı!", this);
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
        // ... (Bu fonksiyon aynı kalıyor)
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