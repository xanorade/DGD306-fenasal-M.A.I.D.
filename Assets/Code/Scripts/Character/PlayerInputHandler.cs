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
        private bool wasLeftKeyUp = true;
        private bool wasRightKeyUp = true;
        private bool wasLeftArrowUp = true;
        private bool wasRightArrowUp = true;

        // Input buffer reference
        private InputBuffer inputBuffer;

        private void Awake()
        {
            // Auto-find the fighter controller if not set
            if (fighter == null)
                fighter = GetComponent<FighterController>();

            // Create input buffer
            inputBuffer = new InputBuffer();
        }

        private void Start()
        {
            if (fighter == null)
            {
                Debug.LogError("PlayerInputHandler: No fighter controller found!");
                enabled = false;
            }
        }

        private void Update()
        {
            inputBuffer.ProcessBuffer();
            
            // Handle input based on player index
            if (playerIndex == PlayerIndex.Player1)
                HandlePlayer1Input();
            else
                HandlePlayer2Input();
        }
        
        private void HandlePlayer1Input()
        {
            // Player 1 uses WASD for movement
            float horizontal = 0f;
            float vertical = 0f;
            
            // Get key states
            bool isAPressed = Input.GetKey(KeyCode.A);
            bool isDPressed = Input.GetKey(KeyCode.D);
            bool isWPressed = Input.GetKey(KeyCode.W);
            bool isSPressed = Input.GetKey(KeyCode.S);
            
            // Set direction values
            if (isAPressed) horizontal -= 1f;
            if (isDPressed) horizontal += 1f;
            if (isWPressed) vertical += 1f;
            if (isSPressed) vertical -= 1f;
            
            // Process movement
            Vector2 moveInput = new Vector2(horizontal, vertical);
            fighter.SetMoveInput(moveInput);
            
            // Detect double-tap for dash (left)
            if (isAPressed && wasLeftKeyUp)
            {
                // A key was just pressed (after being up)
                wasLeftKeyUp = false;
                if (Time.time - lastLeftTapTime < doubleTapTimeThreshold)
                {
                    // Left double tap detected
                    fighter.OnDash(Vector2.left);
                }
                lastLeftTapTime = Time.time;
            }
            else if (!isAPressed)
            {
                // A key is up
                wasLeftKeyUp = true;
            }
            
            // Detect double-tap for dash (right)
            if (isDPressed && wasRightKeyUp)
            {
                // D key was just pressed (after being up)
                wasRightKeyUp = false;
                if (Time.time - lastRightTapTime < doubleTapTimeThreshold)
                {
                    // Right double tap detected
                    fighter.OnDash(Vector2.right);
                }
                lastRightTapTime = Time.time;
            }
            else if (!isDPressed)
            {
                // D key is up
                wasRightKeyUp = true;
            }
            
            // Handle attacks
            if (Input.GetKeyDown(KeyCode.J)) // Punch
                fighter.OnPunch();
                
            if (Input.GetKeyDown(KeyCode.K)) // Kick
                fighter.OnKick();
                
            if (Input.GetKeyDown(KeyCode.L)) // Special
                fighter.OnSpecial();
                
            if (Input.GetKeyDown(KeyCode.Space)) // Jump
                fighter.OnJump();
        }
        
        private void HandlePlayer2Input()
        {
            // Player 2 uses arrow keys for movement
            float horizontal = 0f;
            float vertical = 0f;
            
            // Get key states
            bool isLeftPressed = Input.GetKey(KeyCode.LeftArrow);
            bool isRightPressed = Input.GetKey(KeyCode.RightArrow);
            bool isUpPressed = Input.GetKey(KeyCode.UpArrow);
            bool isDownPressed = Input.GetKey(KeyCode.DownArrow);
            
            // Set direction values
            if (isLeftPressed) horizontal -= 1f;
            if (isRightPressed) horizontal += 1f;
            if (isUpPressed) vertical += 1f;
            if (isDownPressed) vertical -= 1f;
            
            // Process movement
            Vector2 moveInput = new Vector2(horizontal, vertical);
            fighter.SetMoveInput(moveInput);
            
            // Detect double-tap for dash (left)
            if (isLeftPressed && wasLeftArrowUp)
            {
                // Left Arrow key was just pressed (after being up)
                wasLeftArrowUp = false;
                if (Time.time - lastLeftTapTime < doubleTapTimeThreshold)
                {
                    // Left double tap detected
                    fighter.OnDash(Vector2.left);
                }
                lastLeftTapTime = Time.time;
            }
            else if (!isLeftPressed)
            {
                // Left Arrow key is up
                wasLeftArrowUp = true;
            }
            
            // Detect double-tap for dash (right)
            if (isRightPressed && wasRightArrowUp)
            {
                // Right Arrow key was just pressed (after being up)
                wasRightArrowUp = false;
                if (Time.time - lastRightTapTime < doubleTapTimeThreshold)
                {
                    // Right double tap detected
                    fighter.OnDash(Vector2.right);
                }
                lastRightTapTime = Time.time;
            }
            else if (!isRightPressed)
            {
                // Right Arrow key is up
                wasRightArrowUp = true;
            }
            
            // Handle attacks using numpad or number keys
            if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) // Punch
                fighter.OnPunch();
                
            if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2)) // Kick
                fighter.OnKick();
                
            if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3)) // Special
                fighter.OnSpecial();
                
            if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Alpha0)) // Jump
                fighter.OnJump();
        }
    }
} 