using UnityEngine;
using DGD306.Character;

public class FightSceneManager : MonoBehaviour
{
    [Header("Manager References")]
    public RoundManager roundManager;

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

        GameObject mapInstance = Instantiate(GameManager.instance.selectedMapPrefab, Vector3.zero, Quaternion.identity);
        mapInstance.name = GameManager.instance.selectedMapPrefab.name + " (Instance)";

        Transform p1Spawn = mapInstance.transform.Find("Player1_SpawnPoint");
        Transform p2Spawn = mapInstance.transform.Find("Player2_SpawnPoint");
        if (p1Spawn == null || p2Spawn == null)
        {
            Debug.LogError("Map prefab'ı içinde spawn noktaları bulunamadı!");
            return;
        }

        GameObject p1Object = Instantiate(GameManager.instance.player1Prefab, p1Spawn.position, Quaternion.identity);
        GameObject p2Object = Instantiate(GameManager.instance.player2Prefab, p2Spawn.position, Quaternion.identity);
        
        p1Object.name = "Player1";
        p2Object.name = "Player2";

        FighterController p1Controller = p1Object.GetComponent<FighterController>();
        FighterController p2Controller = p2Object.GetComponent<FighterController>();

        PlayerInputHandler p1Input = p1Object.GetComponent<PlayerInputHandler>();
        PlayerInputHandler p2Input = p2Object.GetComponent<PlayerInputHandler>();
        
        // 4. CRITICAL FIX: Set correct player indices regardless of prefab settings
        SetupPlayerControl(p1Object, 1); // Player 1 gets index 1
        SetupPlayerControl(p2Object, 2); // Player 2 gets index 2

        p1Controller.SetOpponent(p2Object.transform);
        p2Controller.SetOpponent(p1Object.transform);

        p1Input.InitializeForPlayer(PlayerInputHandler.PlayerIndex.Player1);
        p2Input.InitializeForPlayer(PlayerInputHandler.PlayerIndex.Player2);

        if (player1HealthBarUI != null)
        {
            player1HealthBarUI.Initialize(p1Controller);
        }
        if (player2HealthBarUI != null)
        {
            player2HealthBarUI.Initialize(p2Controller);
        }
        
        if (roundManager != null)
        {
            roundManager.Initialize(p1Controller, p2Controller, p1Spawn, p2Spawn);
        }
        else
        {
            Debug.LogError("FightSceneManager'a RoundManager referansı atanmamış!");
        }
    }
    
    /// <summary>
    /// Sets up player control by assigning correct player index to both FighterController and PlayerInputHandler
    /// </summary>
    /// <param name="playerObject">The spawned player GameObject</param>
    /// <param name="correctPlayerIndex">The correct player index (1 or 2)</param>
    private void SetupPlayerControl(GameObject playerObject, int correctPlayerIndex)
    {
        // Update FighterController player index
        FighterController fighterController = playerObject.GetComponent<FighterController>();
        if (fighterController != null)
        {
            fighterController.SetPlayerIndex(correctPlayerIndex);
            Debug.Log($"Set FighterController player index to {correctPlayerIndex} for {playerObject.name}");
        }
        
        // Update PlayerInputHandler player index
        PlayerInputHandler inputHandler = playerObject.GetComponent<PlayerInputHandler>();
        if (inputHandler != null)
        {
            // Set the correct player index using the new method
            var playerIndexEnum = (correctPlayerIndex == 1) ? 
                PlayerInputHandler.PlayerIndex.Player1 : 
                PlayerInputHandler.PlayerIndex.Player2;
            
            inputHandler.ChangePlayerIndex(playerIndexEnum);
            Debug.Log($"Set PlayerInputHandler player index to {playerIndexEnum} for {playerObject.name}");
        }
        else
        {
            Debug.LogWarning($"No PlayerInputHandler found on {playerObject.name}");
        }
    }
}