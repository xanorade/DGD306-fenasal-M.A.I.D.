using UnityEngine;

public class FightSceneManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;
    
    [Header("Managers")]
    public RoundManager roundManager;

    [Header("UI References")]
    public HealthBarUI player1HealthBarUI;
    public HealthBarUI player2HealthBarUI;

    void Start()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("Start the game from TitleScreen");
            return;
        }

        GameObject p1Object = Instantiate(GameManager.instance.player1Prefab, player1SpawnPoint.position, Quaternion.identity);
        GameObject p2Object = Instantiate(GameManager.instance.player2Prefab, player2SpawnPoint.position, Quaternion.identity);

        p1Object.name = "Player1";
        p2Object.name = "Player2";

        FighterController p1Controller = p1Object.GetComponent<FighterController>();
        FighterController p2Controller = p2Object.GetComponent<FighterController>();

        p1Controller.SetOpponent(p2Object.transform);
        p2Controller.SetOpponent(p1Object.transform);
        
        if (roundManager != null)
        {
            roundManager.Initialize(p1Controller, p2Controller, player1SpawnPoint, player2SpawnPoint);
        }
        else
        {
            Debug.LogError("No RoundManager assigned to FightSceneManager");
        }

        if (player1HealthBarUI != null)
        {
            player1HealthBarUI.targetFighter = p1Controller;
            player1HealthBarUI.Initialize();
        }
        if (player2HealthBarUI != null)
        {
            player2HealthBarUI.targetFighter = p2Controller;
            player2HealthBarUI.Initialize();
        }
    }
}