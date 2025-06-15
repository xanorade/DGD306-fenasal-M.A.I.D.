using UnityEngine;
using DGD306.Character;

public class FightSceneManager : MonoBehaviour
{
    [Header("Manager References")]
    public RoundManager roundManager;

    // YENİDEN EKLENMESİ GEREKEN UI REFERANSLARI
    [Header("UI References")]
    public HealthBarUI player1HealthBarUI;
    public HealthBarUI player2HealthBarUI;

    void Start()
    {
        if (GameManager.instance == null || GameManager.instance.selectedMapPrefab == null)
        {
            Debug.LogError("GameManager not found or no map selected! Please start from the TitleScreen.");
            return;
        }

        // 1. Haritayı Yarat
        GameObject mapInstance = Instantiate(GameManager.instance.selectedMapPrefab, Vector3.zero, Quaternion.identity);
        mapInstance.name = GameManager.instance.selectedMapPrefab.name + " (Instance)";

        // 2. Spawn Noktalarını Bul
        Transform p1Spawn = mapInstance.transform.Find("Player1_SpawnPoint");
        Transform p2Spawn = mapInstance.transform.Find("Player2_SpawnPoint");
        if (p1Spawn == null || p2Spawn == null)
        {
            Debug.LogError("Map prefab'ı içinde spawn noktaları bulunamadı!");
            return;
        }

        // 3. Karakterleri Yarat
        GameObject p1Object = Instantiate(GameManager.instance.player1Prefab, p1Spawn.position, Quaternion.identity);
        GameObject p2Object = Instantiate(GameManager.instance.player2Prefab, p2Spawn.position, Quaternion.identity);
        p1Object.name = "Player1";
        p2Object.name = "Player2";
        FighterController p1Controller = p1Object.GetComponent<FighterController>();
        FighterController p2Controller = p2Object.GetComponent<FighterController>();
        
        // 4. Rakip ve UI Bağlantılarını Kur
        p1Controller.SetOpponent(p2Object.transform);
        p2Controller.SetOpponent(p1Object.transform);

        // --- İŞTE EKLENMESİ GEREKEN EKSİK KOD BURASI ---
        // HealthBar UI'larına hangi dövüşçüyü takip edeceklerini söylüyoruz
        if (player1HealthBarUI != null)
        {
            player1HealthBarUI.Initialize(p1Controller);
        }
        if (player2HealthBarUI != null)
        {
            player2HealthBarUI.Initialize(p2Controller);
        }
        // ---------------------------------------------
        
        if (roundManager != null)
        {
            roundManager.Initialize(p1Controller, p2Controller, p1Spawn, p2Spawn);
        }
        else
        {
            Debug.LogError("FightSceneManager'a RoundManager referansı atanmamış!");
        }
    }
}