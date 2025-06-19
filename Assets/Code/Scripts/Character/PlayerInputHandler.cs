// Updated Assets/Code/Scripts/Character/PlayerInputHandler.cs
using UnityEngine;

namespace DGD306.Character
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public enum PlayerIndex
        {
            Player1,
            Player2
        }

        [Header("Player Configuration")]
        public PlayerIndex playerIndex = PlayerIndex.Player1;
        [SerializeField] private FighterController fighter;

        [Header("Dash Settings")]
        [SerializeField] private float doubleTapTimeThreshold = 0.2f;
        
        // Input tracking for double tap detection
        private float lastLeftTapTime;
        private float lastRightTapTime;
        private Vector2 lastMoveInput;
        private Vector2 currentMoveInput;

        // Input buffer reference
        private InputBuffer inputBuffer;

        private void Awake()
        {
            if (fighter == null)
                fighter = GetComponent<FighterController>();

            inputBuffer = new InputBuffer();
        }

        private void Start()
        {
            if (fighter == null)
            {
                Debug.LogError("PlayerInputHandler: No fighter controller found!");
                enabled = false;
                return;
            }
            
            // Enable gameplay controls when fighter is ready
            if (InputManager.Instance != null)
            {
                InputManager.Instance.EnableGameplayControls();
            }
            
            // Subscribe to input events based on player index
            SubscribeToInputEvents();
        }

        private void SubscribeToInputEvents()
        {
            if (playerIndex == PlayerIndex.Player1)
            {
                InputManager.OnPlayer1Move += HandleMoveInput;
                InputManager.OnPlayer1Punch += HandlePunchInput;
                InputManager.OnPlayer1Kick += HandleKickInput;
                InputManager.OnPlayer1Special += HandleSpecialInput;
                InputManager.OnPlayer1Jump += HandleJumpInput;
            }
            else
            {
                InputManager.OnPlayer2Move += HandleMoveInput;
                InputManager.OnPlayer2Punch += HandlePunchInput;
                InputManager.OnPlayer2Kick += HandleKickInput;
                InputManager.OnPlayer2Special += HandleSpecialInput;
                InputManager.OnPlayer2Jump += HandleJumpInput;
            }
        }

        private void UnsubscribeFromInputEvents()
        {
            if (playerIndex == PlayerIndex.Player1)
            {
                InputManager.OnPlayer1Move -= HandleMoveInput;
                InputManager.OnPlayer1Punch -= HandlePunchInput;
                InputManager.OnPlayer1Kick -= HandleKickInput;
                InputManager.OnPlayer1Special -= HandleSpecialInput;
                InputManager.OnPlayer1Jump -= HandleJumpInput;
            }
            else
            {
                InputManager.OnPlayer2Move -= HandleMoveInput;
                InputManager.OnPlayer2Punch -= HandlePunchInput;
                InputManager.OnPlayer2Kick -= HandleKickInput;
                InputManager.OnPlayer2Special -= HandleSpecialInput;
                InputManager.OnPlayer2Jump -= HandleJumpInput;
            }
        }
        
        public void InitializeForPlayer(PlayerIndex index)
        {
            playerIndex = index;
        }

        private void Update()
        {
            inputBuffer.ProcessBuffer();
            
            // Handle dash detection
            HandleDashDetection();
        }
        
        private void HandleMoveInput(Vector2 moveInput)
        {
            currentMoveInput = moveInput;
            fighter.SetMoveInput(moveInput);
        }
        
        private void HandlePunchInput()
        {
            fighter.OnPunch();
        }
        
        private void HandleKickInput()
        {
            fighter.OnKick();
        }
        
        private void HandleSpecialInput()
        {
            fighter.OnSpecial();
        }
        
        private void HandleJumpInput()
        {
            fighter.OnJump();
        }
        
        private void HandleDashDetection()
        {
            // Detect direction changes for dash
            if (currentMoveInput != lastMoveInput)
            {
                // Left dash detection
                if (currentMoveInput.x < -0.5f && lastMoveInput.x >= -0.5f)
                {
                    if (Time.time - lastLeftTapTime < doubleTapTimeThreshold)
                    {
                        fighter.OnDash(Vector2.left);
                    }
                    lastLeftTapTime = Time.time;
                }
                
                // Right dash detection
                if (currentMoveInput.x > 0.5f && lastMoveInput.x <= 0.5f)
                {
                    if (Time.time - lastRightTapTime < doubleTapTimeThreshold)
                    {
                        fighter.OnDash(Vector2.right);
                    }
                    lastRightTapTime = Time.time;
                }
                
                lastMoveInput = currentMoveInput;
            }
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromInputEvents();
        }
        
        /// <summary>
        /// Changes the player index dynamically and re-subscribes to the correct input events
        /// This is used by FightSceneManager to fix character control assignments
        /// </summary>
        /// <param name="newPlayerIndex">The new player index to assign</param>
        public void ChangePlayerIndex(PlayerIndex newPlayerIndex)
        {
            if (playerIndex == newPlayerIndex) return; // No change needed
            
            Debug.Log($"PlayerInputHandler: Changing player index from {playerIndex} to {newPlayerIndex} on {gameObject.name}");
            
            // Unsubscribe from current events
            UnsubscribeFromInputEvents();
            
            // Change the player index
            playerIndex = newPlayerIndex;
            
            // Re-subscribe to the correct events
            SubscribeToInputEvents();
        }
    }
}