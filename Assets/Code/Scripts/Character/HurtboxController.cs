using System.Collections.Generic;
using UnityEngine;
using System;

namespace DGD306.Character
{
    public class HurtboxController : MonoBehaviour
    {
        [Header("Hurtbox Settings")]
        [SerializeField] private bool showDebugBoxes = true;
        [SerializeField] private float frameRate = 60f;
        
        [Header("State Hurtbox Data")]
        [SerializeField] private StateHurtboxData[] stateHurtboxes = new StateHurtboxData[]
        {
            new StateHurtboxData("Idle"),
            new StateHurtboxData("Crouch"),
            new StateHurtboxData("Jump"),
            new StateHurtboxData("Walk"),
            new StateHurtboxData("Block")
        };
        
        // Runtime data
        private FighterController fighter;
        private string currentState = "";
        private float currentStateTime = 0f;
        private Dictionary<string, StateHurtboxData> hurtboxDataDict = new Dictionary<string, StateHurtboxData>();
        
        // Active hurtboxes for current frame
        private List<BoxData> activeHurtboxes = new List<BoxData>();
        private List<BoxCollider2D> hurtboxColliders = new List<BoxCollider2D>();
        
        // Events
        public System.Action<float, Transform, string> OnHitTaken;
        
        private void Awake()
        {
            fighter = GetComponent<FighterController>();
            
            // Initialize hurtbox data dictionary
            foreach (var stateData in stateHurtboxes)
            {
                hurtboxDataDict[stateData.stateName] = stateData;
            }
            
            // Initialize default hurtbox data for states that don't have custom data
            InitializeDefaultHurtboxes();
            
            // Create colliders for hurtboxes
            CreateHurtboxColliders();
        }
        
        private void InitializeDefaultHurtboxes()
        {
            // Idle/Standing state default - full body hurtbox
            if (!hurtboxDataDict.ContainsKey("Idle"))
            {
                var idleData = new StateHurtboxData("Idle");
                idleData.hurtboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(0f, 0f),
                        size = new Vector2(0.8f, 1.6f),
                        startFrame = 0f,
                        endFrame = 999f, // Always active
                        damage = 0f, // Hurtboxes don't deal damage
                        debugColor = Color.green
                    }
                };
                hurtboxDataDict["Idle"] = idleData;
            }
            
            // Crouching state - smaller hurtbox
            if (!hurtboxDataDict.ContainsKey("Crouch"))
            {
                var crouchData = new StateHurtboxData("Crouch");
                crouchData.hurtboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(0f, -0.4f),
                        size = new Vector2(0.8f, 0.8f),
                        startFrame = 0f,
                        endFrame = 999f,
                        damage = 0f,
                        debugColor = Color.yellow
                    }
                };
                hurtboxDataDict["Crouch"] = crouchData;
            }
            
            // Jump state - airborne hurtbox
            if (!hurtboxDataDict.ContainsKey("Jump"))
            {
                var jumpData = new StateHurtboxData("Jump");
                jumpData.hurtboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(0f, 0.1f),
                        size = new Vector2(0.7f, 1.4f),
                        startFrame = 0f,
                        endFrame = 999f,
                        damage = 0f,
                        debugColor = Color.cyan
                    }
                };
                hurtboxDataDict["Jump"] = jumpData;
            }
            
            // Walking state - same as idle
            if (!hurtboxDataDict.ContainsKey("Walk"))
            {
                var walkData = new StateHurtboxData("Walk");
                walkData.hurtboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(0f, 0f),
                        size = new Vector2(0.8f, 1.6f),
                        startFrame = 0f,
                        endFrame = 999f,
                        damage = 0f,
                        debugColor = Color.green
                    }
                };
                hurtboxDataDict["Walk"] = walkData;
            }
            
            // Block state - slightly reduced hurtbox
            if (!hurtboxDataDict.ContainsKey("Block"))
            {
                var blockData = new StateHurtboxData("Block");
                blockData.hurtboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(-0.1f, 0f),
                        size = new Vector2(0.7f, 1.6f),
                        startFrame = 0f,
                        endFrame = 999f,
                        damage = 0f,
                        debugColor = Color.blue
                    }
                };
                hurtboxDataDict["Block"] = blockData;
            }
        }
        
        private void CreateHurtboxColliders()
        {
            // Clear existing colliders
            foreach (var collider in hurtboxColliders)
            {
                if (collider != null) DestroyImmediate(collider.gameObject);
            }
            hurtboxColliders.Clear();
            
            // Create a collider for each potential hurtbox
            int maxHurtboxes = 0;
            foreach (var stateData in hurtboxDataDict.Values)
            {
                maxHurtboxes = Mathf.Max(maxHurtboxes, stateData.hurtboxes.Length);
            }
            
            for (int i = 0; i < maxHurtboxes; i++)
            {
                GameObject hurtboxObj = new GameObject($"Hurtbox_{i}");
                hurtboxObj.transform.SetParent(transform);
                hurtboxObj.transform.localPosition = Vector3.zero;
                hurtboxObj.layer = gameObject.layer;
                
                BoxCollider2D collider = hurtboxObj.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.enabled = false; // Start disabled
                
                hurtboxColliders.Add(collider);
            }
        }
        
        private void Update()
        {
            if (fighter == null) return;
            
            string newState = GetCurrentHurtboxState();
            
            // Check if state changed
            if (newState != currentState)
            {
                OnStateChanged(newState);
            }
            
            // Update current state time
            currentStateTime += Time.deltaTime;
            
            // Update active hurtboxes for current frame
            UpdateActiveHurtboxes();
            
            // Update collider positions and sizes
            UpdateHurtboxColliders();
        }
        
        private string GetCurrentHurtboxState()
        {
            // Map fighter states to hurtbox states
            string fighterState = fighter.CurrentStateName;
            
            // Check for crouching first (highest priority for hurtbox changes)
            if (fighter.IsCrouching)
                return "Crouch";
            
            // Check for jumping states
            if (fighterState.Contains("Jump") || !fighter.IsGrounded)
                return "Jump";
            
            // Check for blocking
            if (fighterState == "Block")
                return "Block";
            
            // Check for walking
            if (fighterState == "Walk" || fighter.GetComponent<Rigidbody2D>().velocity.x != 0)
                return "Walk";
            
            // Default to idle
            return "Idle";
        }
        
        private void OnStateChanged(string newState)
        {
            currentState = newState;
            currentStateTime = 0f;
            activeHurtboxes.Clear();
        }
        
        private void UpdateActiveHurtboxes()
        {
            activeHurtboxes.Clear();
            
            if (!hurtboxDataDict.ContainsKey(currentState)) return;
            
            StateHurtboxData stateData = hurtboxDataDict[currentState];
            float currentFrame = currentStateTime * frameRate;
            
            foreach (var hurtbox in stateData.hurtboxes)
            {
                if (currentFrame >= hurtbox.startFrame && currentFrame <= hurtbox.endFrame)
                {
                    activeHurtboxes.Add(hurtbox);
                }
            }
        }
        
        private void UpdateHurtboxColliders()
        {
            // Disable all colliders first
            foreach (var collider in hurtboxColliders)
            {
                collider.enabled = false;
            }
            
            // Enable and position active hurtboxes
            for (int i = 0; i < activeHurtboxes.Count && i < hurtboxColliders.Count; i++)
            {
                var hurtbox = activeHurtboxes[i];
                var collider = hurtboxColliders[i];
                
                // Enable the collider
                collider.enabled = true;
                
                // Set position and size
                Vector2 localPos = hurtbox.offset;
                // Account for character facing direction
                if (transform.localScale.x < 0)
                {
                    localPos.x *= -1;
                }
                
                collider.transform.localPosition = localPos;
                collider.size = hurtbox.size;
            }
        }
        
        public void TakeHit(float damage, Transform attacker, string attackType)
        {
            // Check if we're blocking and the attack is from the front
            if (IsBlocking(attacker))
            {
                damage *= 0.25f; // Reduce damage when blocking
                Debug.Log($"{gameObject.name} blocked {attackType} from {attacker.name} for {damage:F1} damage!");
            }
            else
            {
                Debug.Log($"{gameObject.name} hit by {attackType} from {attacker.name} for {damage:F1} damage!");
            }
            
            // Notify the fighter controller about the hit
            OnHitTaken?.Invoke(damage, attacker, attackType);
            
            // You can add hitstun, knockback, or other effects here
            // For now, we'll just trigger a hit state if the fighter isn't already in one
            if (fighter.CurrentStateName != "Hit")
            {
                // This would require adding a TakeHit method to FighterController
                // We'll add that in the next step
            }
        }
        
        private bool IsBlocking(Transform attacker)
        {
            // Check if the fighter is in a blocking state
            if (fighter.CurrentStateName != "Block") return false;
            
            // Check if the attacker is in front of us
            Vector3 toAttacker = attacker.position - transform.position;
            bool attackerOnRight = toAttacker.x > 0;
            bool facingRight = transform.localScale.x > 0;
            
            // We're blocking if the attacker is in the direction we're facing
            return attackerOnRight == facingRight;
        }
        
        private void OnDrawGizmos()
        {
            if (!showDebugBoxes) return;
            
            foreach (var hurtbox in activeHurtboxes)
            {
                Gizmos.color = hurtbox.debugColor;
                Vector2 worldPos = GetWorldPosition(hurtbox.offset);
                Vector2 worldSize = GetWorldSize(hurtbox.size);
                Gizmos.DrawWireCube(worldPos, worldSize);
            }
        }
        
        private Vector2 GetWorldPosition(Vector2 localOffset)
        {
            Vector3 worldOffset = localOffset;
            // Account for character facing direction
            if (transform.localScale.x < 0)
            {
                worldOffset.x *= -1;
            }
            return (Vector2)transform.position + (Vector2)worldOffset;
        }
        
        private Vector2 GetWorldSize(Vector2 localSize)
        {
            return new Vector2(
                localSize.x * Mathf.Abs(transform.localScale.x),
                localSize.y * Mathf.Abs(transform.localScale.y)
            );
        }
        
        // Public method to get current active hurtboxes (for debugging)
        public List<BoxData> GetActiveHurtboxes()
        {
            return new List<BoxData>(activeHurtboxes);
        }
        
        // Public method to get hurtbox data for a specific state
        public StateHurtboxData GetHurtboxData(string stateName)
        {
            return hurtboxDataDict.ContainsKey(stateName) ? hurtboxDataDict[stateName] : null;
        }
        
        // Public method to get current hurtbox state
        public string GetCurrentHurtboxStateName()
        {
            return currentState;
        }
    }
} 