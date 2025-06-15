using UnityEngine;
using DGD306.Character;

[System.Serializable]
public class CombatLayerSetup
{
    [Header("Layer Configuration")]
    public LayerMask hitboxLayers = 1 << 8; // Default layer 8 for hitboxes
    public LayerMask hurtboxLayers = 1 << 9; // Default layer 9 for hurtboxes
    public LayerMask playerLayers = 1 << 10; // Default layer 10 for players
    
    [Header("Debug Info")]
    [SerializeField] private bool showLayerInfo = true;
    
    public void LogLayerSetup()
    {
        if (!showLayerInfo) return;
        
        Debug.Log($"Hitbox Layers: {hitboxLayers.value} (Binary: {System.Convert.ToString(hitboxLayers.value, 2)})");
        Debug.Log($"Hurtbox Layers: {hurtboxLayers.value} (Binary: {System.Convert.ToString(hurtboxLayers.value, 2)})");
        Debug.Log($"Player Layers: {playerLayers.value} (Binary: {System.Convert.ToString(playerLayers.value, 2)})");
    }
}

namespace DGD306.Character
{
    public class CombatSystemSetup : MonoBehaviour
    {
        [Header("Combat System Setup")]
        [SerializeField] private bool autoSetupOnAwake = true;
        [SerializeField] private CombatLayerSetup layerSetup = new CombatLayerSetup();
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        private void Awake()
        {
            if (autoSetupOnAwake)
            {
                SetupCombatSystem();
            }
        }
        
        [ContextMenu("Setup Combat System")]
        public void SetupCombatSystem()
        {
            // First log the layer setup for debugging
            layerSetup.LogLayerSetup();
            
            SetupHitboxController();
            SetupHurtboxController();
            
            if (showDebugInfo)
            {
                Debug.Log($"Combat system setup complete for {gameObject.name}");
                Debug.Log($"GameObject layer: {gameObject.layer} ({LayerMask.LayerToName(gameObject.layer)})");
            }
        }
        
        private void SetupHitboxController()
        {
            HitboxController hitboxController = GetComponent<HitboxController>();
            if (hitboxController == null)
            {
                hitboxController = gameObject.AddComponent<HitboxController>();
                if (showDebugInfo)
                {
                    Debug.Log($"Added HitboxController to {gameObject.name}");
                }
            }
            
            // Set up layer configuration via reflection since the fields are private
            var hitboxField = typeof(HitboxController).GetField("hurtboxLayers", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hitboxField != null)
            {
                if (layerSetup.hurtboxLayers.value > 0)
                {
                    hitboxField.SetValue(hitboxController, layerSetup.hurtboxLayers);
                    if (showDebugInfo)
                    {
                        Debug.Log($"Set HitboxController hurtboxLayers to {layerSetup.hurtboxLayers.value} for {gameObject.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Cannot set hurtbox layers for HitboxController on {gameObject.name} - LayerMask value is {layerSetup.hurtboxLayers.value}");
                }
            }
            else
            {
                Debug.LogError($"Could not find hurtboxLayers field in HitboxController via reflection!");
            }
        }
        
        private void SetupHurtboxController()
        {
            HurtboxController hurtboxController = GetComponent<HurtboxController>();
            if (hurtboxController == null)
            {
                hurtboxController = gameObject.AddComponent<HurtboxController>();
                if (showDebugInfo)
                {
                    Debug.Log($"Added HurtboxController to {gameObject.name}");
                }
            }
            
            // Set the layer for hurtbox colliders
            if (layerSetup.hurtboxLayers.value > 0)
            {
                int hurtboxLayer = (int)Mathf.Log(layerSetup.hurtboxLayers.value, 2);
                gameObject.layer = hurtboxLayer;
                
                if (showDebugInfo)
                {
                    Debug.Log($"Set {gameObject.name} to layer {hurtboxLayer} ({LayerMask.LayerToName(hurtboxLayer)})");
                }
            }
            else
            {
                Debug.LogWarning($"Hurtbox layers not configured properly for {gameObject.name}! LayerMask value: {layerSetup.hurtboxLayers.value}");
            }
        }
        
        [ContextMenu("Test Hit System")]
        public void TestHitSystem()
        {
            // Find another fighter to test with
            FighterController[] allFighters = FindObjectsOfType<FighterController>();
            FighterController otherFighter = null;
            
            foreach (var fighter in allFighters)
            {
                if (fighter.gameObject != this.gameObject)
                {
                    otherFighter = fighter;
                    break;
                }
            }
            
            if (otherFighter != null)
            {
                HurtboxController otherHurtbox = otherFighter.GetComponent<HurtboxController>();
                if (otherHurtbox != null)
                {
                    otherHurtbox.TakeHit(25f, transform, "Test Attack");
                    Debug.Log($"Simulated hit from {gameObject.name} to {otherFighter.name}");
                }
            }
            else
            {
                Debug.LogWarning("No other fighter found to test with!");
            }
        }
        
        [ContextMenu("Reset All Health")]
        public void ResetAllHealth()
        {
            FighterController[] allFighters = FindObjectsOfType<FighterController>();
            foreach (var fighter in allFighters)
            {
                fighter.ResetHealth();
            }
            Debug.Log("Reset health for all fighters");
        }
        
        [ContextMenu("Show Combat Info")]
        public void ShowCombatInfo()
        {
            FighterController fighter = GetComponent<FighterController>();
            HitboxController hitbox = GetComponent<HitboxController>();
            HurtboxController hurtbox = GetComponent<HurtboxController>();
            
            Debug.Log($"=== Combat Info for {gameObject.name} ===");
            Debug.Log($"Health: {fighter.CurrentHealth}/{fighter.MaxHealth}");
            Debug.Log($"State: {fighter.CurrentStateName}");
            Debug.Log($"Hitbox Controller: {(hitbox != null ? "Present" : "Missing")}");
            Debug.Log($"Hurtbox Controller: {(hurtbox != null ? "Present" : "Missing")}");
            
            if (hitbox != null)
            {
                var activeHitboxes = hitbox.GetActiveHitboxes();
                Debug.Log($"Active Hitboxes: {activeHitboxes.Count}");
            }
            
            if (hurtbox != null)
            {
                var activeHurtboxes = hurtbox.GetActiveHurtboxes();
                Debug.Log($"Active Hurtboxes: {activeHurtboxes.Count}");
                Debug.Log($"Current Hurtbox State: {hurtbox.GetCurrentHurtboxStateName()}");
            }
        }
        
        [ContextMenu("Trigger Win")]
        public void TriggerWin()
        {
            FighterController fighter = GetComponent<FighterController>();
            if (fighter != null)
            {
                fighter.TriggerWinForTesting();
                Debug.Log($"Triggered win for {gameObject.name}");
            }
        }
        
        [ContextMenu("Trigger Death")]
        public void TriggerDeath()
        {
            FighterController fighter = GetComponent<FighterController>();
            if (fighter != null)
            {
                // Set health to 0 and trigger death naturally
                HurtboxController hurtbox = GetComponent<HurtboxController>();
                if (hurtbox != null)
                {
                    hurtbox.TakeHit(fighter.CurrentHealth + 10f, transform, "Test Death");
                    Debug.Log($"Triggered death for {gameObject.name}");
                }
            }
        }
        
        [ContextMenu("Test Win Condition - Kill Opponent")]
        public void TestWinCondition()
        {
            FighterController fighter = GetComponent<FighterController>();
            if (fighter == null) return;
            
            // Find opponent
            FighterController[] allFighters = FindObjectsOfType<FighterController>();
            FighterController opponent = null;
            
            foreach (var otherFighter in allFighters)
            {
                if (otherFighter.gameObject != this.gameObject)
                {
                    opponent = otherFighter;
                    break;
                }
            }
            
            if (opponent != null)
            {
                // Deal massive damage to opponent to trigger win condition
                HurtboxController opponentHurtbox = opponent.GetComponent<HurtboxController>();
                if (opponentHurtbox != null)
                {
                    opponentHurtbox.TakeHit(opponent.CurrentHealth + 10f, transform, "Test Win Condition");
                    Debug.Log($"Dealt lethal damage to {opponent.name} to test win condition for {gameObject.name}");
                }
            }
            else
            {
                Debug.LogWarning("No opponent found to test win condition!");
            }
        }
    }
} 