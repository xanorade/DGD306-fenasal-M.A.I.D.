using System.Collections.Generic;
using UnityEngine;
using System;

namespace DGD306.Character
{
    public class HitboxController : MonoBehaviour
    {
        [Header("Hitbox Settings")]
        [SerializeField] private LayerMask hurtboxLayers = -1;
        [SerializeField] private bool showDebugBoxes = true;
        [SerializeField] private float frameRate = 12f;
        
        [Header("State Hitbox Data")]
        [SerializeField] private StateHitboxData[] stateHitboxes = new StateHitboxData[]
        {
            new StateHitboxData("Punch"),
            new StateHitboxData("Kick"), 
            new StateHitboxData("CrouchPunch"),
            new StateHitboxData("CrouchKick"),
            new StateHitboxData("JumpPunch"),
            new StateHitboxData("JumpKick"),
            new StateHitboxData("Special")
        };
        
        // Runtime data
        private FighterController fighter;
        private string currentState = "";
        private float currentStateTime = 0f;
        private List<Collider2D> currentHitTargets = new List<Collider2D>();
        private Dictionary<string, StateHitboxData> hitboxDataDict = new Dictionary<string, StateHitboxData>();
        
        // Active hitboxes for current frame
        private List<BoxData> activeHitboxes = new List<BoxData>();
        
        private void Awake()
        {
            fighter = GetComponent<FighterController>();
            
            // Initialize hitbox data dictionary
            foreach (var stateData in stateHitboxes)
            {
                hitboxDataDict[stateData.stateName] = stateData;
            }
            
            // Initialize default hitbox data for states that don't have custom data
            InitializeDefaultHitboxes();
        }
        
        private void InitializeDefaultHitboxes()
        {
            // Punch state default
            if (!hitboxDataDict.ContainsKey("Punch"))
            {
                var punchData = new StateHitboxData("Punch");
                punchData.hitboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(0.8f, 0f),
                        size = new Vector2(100f, 100f),
                        startFrame = 3f,
                        endFrame = 80f,
                        damage = 15f,
                        debugColor = Color.red
                    }
                };
                hitboxDataDict["Punch"] = punchData;
            }
            
            // Kick state default
            if (!hitboxDataDict.ContainsKey("Kick"))
            {
                var kickData = new StateHitboxData("Kick");
                kickData.hitboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(0.9f, -0.2f),
                        size = new Vector2(0.7f, 0.5f),
                        startFrame = 4f,
                        endFrame = 10f,
                        damage = 20f,
                        debugColor = Color.blue
                    }
                };
                hitboxDataDict["Kick"] = kickData;
            }
            
            // Crouch Punch default
            if (!hitboxDataDict.ContainsKey("CrouchPunch"))
            {
                var crouchPunchData = new StateHitboxData("CrouchPunch");
                crouchPunchData.hitboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(0.7f, -0.5f),
                        size = new Vector2(0.5f, 0.3f),
                        startFrame = 2f,
                        endFrame = 7f,
                        damage = 12f,
                        debugColor = Color.yellow
                    }
                };
                hitboxDataDict["CrouchPunch"] = crouchPunchData;
            }
            
            // Crouch Kick default
            if (!hitboxDataDict.ContainsKey("CrouchKick"))
            {
                var crouchKickData = new StateHitboxData("CrouchKick");
                crouchKickData.hitboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(1.0f, -0.7f),
                        size = new Vector2(0.8f, 0.3f),
                        startFrame = 3f,
                        endFrame = 9f,
                        damage = 18f,
                        debugColor = Color.cyan
                    }
                };
                hitboxDataDict["CrouchKick"] = crouchKickData;
            }
            
            // Jump Punch default
            if (!hitboxDataDict.ContainsKey("JumpPunch"))
            {
                var jumpPunchData = new StateHitboxData("JumpPunch");
                jumpPunchData.hitboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(0.6f, 0.2f),
                        size = new Vector2(0.5f, 0.4f),
                        startFrame = 3f,
                        endFrame = 8f,
                        damage = 16f,
                        debugColor = Color.magenta
                    }
                };
                hitboxDataDict["JumpPunch"] = jumpPunchData;
            }
            
            // Jump Kick default
            if (!hitboxDataDict.ContainsKey("JumpKick"))
            {
                var jumpKickData = new StateHitboxData("JumpKick");
                jumpKickData.hitboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(0.7f, -0.3f),
                        size = new Vector2(0.6f, 0.5f),
                        startFrame = 4f,
                        endFrame = 10f,
                        damage = 22f,
                        debugColor = Color.green
                    }
                };
                hitboxDataDict["JumpKick"] = jumpKickData;
            }
            
            // Special Move default
            if (!hitboxDataDict.ContainsKey("Special"))
            {
                var specialData = new StateHitboxData("Special");
                specialData.hitboxes = new BoxData[]
                {
                    new BoxData
                    {
                        offset = new Vector2(1.2f, 0f),
                        size = new Vector2(1.0f, 0.8f),
                        startFrame = 8f,
                        endFrame = 15f,
                        damage = 35f,
                        debugColor = Color.white
                    }
                };
                hitboxDataDict["Special"] = specialData;
            }
        }
        
        private void Update()
        {
            if (fighter == null) return;
            
            // Don't process hitboxes if fighter is dead, defeated, or has won
            if (!fighter.IsAlive || fighter.IsDead || fighter.HasWon) 
            {
                activeHitboxes.Clear();
                currentHitTargets.Clear();
                return;
            }
            
            string newState = fighter.CurrentStateName;
            
            // Check if state changed
            if (newState != currentState)
            {
                OnStateChanged(newState);
            }
            
            // Update current state time
            currentStateTime += Time.deltaTime;
            
            // Update active hitboxes for current frame
            UpdateActiveHitboxes();
            
            // Check for collisions
            CheckHitboxCollisions();
        }
        
        private void OnStateChanged(string newState)
        {
            currentState = newState;
            currentStateTime = 0f;
            currentHitTargets.Clear();
            activeHitboxes.Clear();
        }
        
        private void UpdateActiveHitboxes()
        {
            activeHitboxes.Clear();
            
            if (!hitboxDataDict.ContainsKey(currentState)) return;
            
            StateHitboxData stateData = hitboxDataDict[currentState];
            float currentFrame = currentStateTime * frameRate;
            
            foreach (var hitbox in stateData.hitboxes)
            {
                if (currentFrame >= hitbox.startFrame && currentFrame <= hitbox.endFrame)
                {
                    activeHitboxes.Add(hitbox);
                }
            }
        }
        
        private void CheckHitboxCollisions()
        {
            foreach (var hitbox in activeHitboxes)
            {
                Vector2 worldPos = GetWorldPosition(hitbox.offset);
                Vector2 worldSize = GetWorldSize(hitbox.size);
                
                Collider2D[] hits = Physics2D.OverlapBoxAll(worldPos, worldSize, 0f, hurtboxLayers);
                
                foreach (var hit in hits)
                {
                    // Don't hit ourselves
                    if (hit.transform.root == transform.root) continue;
                    
                    // Don't hit the same target twice in one attack
                    if (currentHitTargets.Contains(hit)) continue;
                    
                    // Check if this is a valid hurtbox
                    HurtboxController hurtboxController = hit.GetComponentInParent<HurtboxController>();
                    if (hurtboxController != null)
                    {
                        hurtboxController.TakeHit(hitbox.damage, transform, currentState);
                        currentHitTargets.Add(hit);
                    }
                }
            }
        }
        
        private Vector2 GetWorldPosition(Vector2 localOffset)
        {
            Vector3 worldOffset = transform.TransformDirection(localOffset);
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
        
        private void OnDrawGizmos()
        {
            if (!showDebugBoxes) return;
            
            foreach (var hitbox in activeHitboxes)
            {
                Gizmos.color = hitbox.debugColor;
                Vector2 worldPos = GetWorldPosition(hitbox.offset);
                Vector2 worldSize = GetWorldSize(hitbox.size);
                Gizmos.DrawWireCube(worldPos, worldSize);
            }
        }
        
        // Public method to get current active hitboxes (for debugging)
        public List<BoxData> GetActiveHitboxes()
        {
            return new List<BoxData>(activeHitboxes);
        }
        
        // Public method to get hitbox data for a specific state
        public StateHitboxData GetHitboxData(string stateName)
        {
            return hitboxDataDict.ContainsKey(stateName) ? hitboxDataDict[stateName] : null;
        }
    }
} 