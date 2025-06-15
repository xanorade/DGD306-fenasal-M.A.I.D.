using System.Collections;
using UnityEngine;
using System;
using TMPro;
using DGD306.Character;

public class FighterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;
    
    [Header("Combat Settings")]
    [SerializeField] private float specialBarMax = 100f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    
    [Header("Target Settings")]
    [SerializeField] private Transform opponentTransform;
    
    [Header("State Display")]
    [SerializeField] private string currentStateName = "Idle";
    [SerializeField] private bool showStateText = true;
    [SerializeField] private Color stateTextColor = Color.white;
    private GameObject stateTextObj;
    private TextMeshPro stateText;
    
    [Header("Direction Indicator")]
    [SerializeField] private bool showDirectionArrow = true;
    [SerializeField] private Color directionArrowColor = Color.yellow;
    private GameObject directionArrowObj;
    private TextMeshPro directionArrow;
    
    [Header("Player Settings")]
    [SerializeField] private int playerIndex = 1; // 1 for Player1, 2 for Player2
    
    // Flag indicating whether a PlayerInputHandler is controlling this fighter
    [HideInInspector] public bool isControlledExternally = false;
    
    // Public property to access current state name
    public string CurrentStateName => currentStateName;
    
    // Public property to access crouching state
    public bool IsCrouching => isCrouching;
    
    // Public property to access grounded state
    public bool IsGrounded => isGrounded;
    
    // Public property to access Rigidbody2D
    public Rigidbody2D rb { get; private set; }
    
    // Public properties for health system
    public float MaxHealth => maxHealth;
    public event Action<float, float> OnHealthChanged;  
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0f;
    public bool HasWon => stateMachine?.CurrentStateType == typeof(WinState);
    public bool IsDead => stateMachine?.CurrentStateType == typeof(DeathState);
    
    [HideInInspector] public RoundManager roundManager;
    
    // State machine reference
    private FighterStateMachine stateMachine;
    
    // Input buffer system
    private InputBuffer inputBuffer;
    
    // Movement variables
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private float lastDirectionChangeTime = 0f;
    private float lastMoveInputX = 0f;
    private bool isGrounded;
    private bool isCrouching;
    private bool isBlocking;
    private bool canDash = true;
    private float dashTimer;
    private float lastDashTime;
    
    // Double tap detection
    private float doubleTapTimeThreshold = 0.2f;
    private float lastLeftTapTime;
    private float lastRightTapTime;
    private bool wasLeftKeyUp = true;
    private bool wasRightKeyUp = true;
    
    // Combat variables
    private float specialBarCurrent;
    
    // Components
    private Animator animator;
    private HurtboxController hurtboxController;
    
    // Health and damage tracking
    private string lastAttackTaken = "";
    private float lastDamageTaken = 0f;
    private Transform lastAttacker;
    
    // Input handling
    private bool useDirectInput = true;
    
    // Fixed movement values for consistent feel
    private const float MOVE_ACCELERATION = 80f;
    private const float MOVE_DECELERATION = 60f;
    
    // Last known positions for pass-by detection
    private float lastPlayerPosX;
    private float lastOpponentPosX;
    
    // Flip handling
    private float lastFlipTime = 0f;
    private float flipCooldown = 0.25f; // Prevent flipping more than once every 0.25 seconds
    
    private void Awake(){
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        hurtboxController = GetComponent<HurtboxController>();
        
        stateMachine = new FighterStateMachine(this);
        inputBuffer = new InputBuffer();
        
        // Auto-detect player index from name if not set
        if (gameObject.name.Contains("2"))
            playerIndex = 2;
        else if (gameObject.name.Contains("1"))
            playerIndex = 1;
            
        CreateStateText();
        CreateDirectionArrow();
        
        UpdateStateText("Idle");
        
        // Initialize health
        currentHealth = maxHealth;
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Subscribe to hurtbox events
        if (hurtboxController != null)
        {
            hurtboxController.OnHitTaken += OnHitTaken;
        }
        
        // Check if this fighter has a PlayerInputHandler attached
        isControlledExternally = GetComponent<DGD306.Character.PlayerInputHandler>() != null;
    }
    
    private void Start() 
    {
        // Find opponent if not set
        if (opponentTransform == null)
        {
            if (playerIndex == 1)
                opponentTransform = GameObject.Find("Player2")?.transform;
            else if (playerIndex == 2)
                opponentTransform = GameObject.Find("Player1")?.transform;
                
            if (opponentTransform == null)
            {
                FighterController[] fighters = FindObjectsOfType<FighterController>();
                foreach (FighterController fighter in fighters)
                {
                    if (fighter != this)
                    {
                        opponentTransform = fighter.transform;
                        break;
                    }
                }
            }
        }
        
        // Set fixed initial facing direction - Player1 faces right, Player2 faces left
        if (playerIndex == 1 && transform.localScale.x < 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (playerIndex == 2 && transform.localScale.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        
        // Store initial positions for pass-by detection
        if (opponentTransform != null)
        {
            lastPlayerPosX = transform.position.x;
            lastOpponentPosX = opponentTransform.position.x;
        }
    }
    
    private void Update(){
        inputBuffer.ProcessBuffer();
        
        if (!canDash && Time.time > lastDashTime + dashCooldown)
            canDash = true;
        
        // Only process direct input if not controlled by PlayerInputHandler
        if (!isControlledExternally)    
            HandleDirectInput();
            
        stateMachine.UpdateState();
        
        // Check if players passed by each other - only when not already in the process of being flipped
        if (opponentTransform != null)
        {
            CheckPlayerPassBy();
        }

        // Always keep the state text above the player and maintain world orientation
        UpdateStateTextPosition();
        
        // Keep direction arrow updated
        UpdateDirectionArrow();
    }
    
    // Check if the players have passed by each other and handle orientation
    private void CheckPlayerPassBy()
    {
        if (opponentTransform == null) return;
        
        // Don't check if we flipped too recently
        if (Time.time - lastFlipTime < flipCooldown) return;
        
        float currentPlayerPosX = transform.position.x;
        float currentOpponentPosX = opponentTransform.position.x;
        
        // Get current facing direction
        bool isFacingRight = transform.localScale.x > 0;
        
        // Determine if player should be facing right based on relative position
        bool shouldFaceRight = currentPlayerPosX < currentOpponentPosX;
        
        // If facing direction is incorrect, flip the character
        if ((isFacingRight && !shouldFaceRight) || (!isFacingRight && shouldFaceRight))
        {
            // Allow flipping if not blocking and either can move OR is in a jump state
            bool canFlip = !isBlocking && (stateMachine.CanMove() || IsInJumpState());
            
            if (canFlip)
            {
                FlipCharacter(true);
                lastFlipTime = Time.time;
            }
        }
        
        // Store current positions for next frame
        lastPlayerPosX = currentPlayerPosX;
        lastOpponentPosX = currentOpponentPosX;
    }
    
    // Make the fighter's orientation flip
    public void FlipCharacter(bool notifyOpponent = true){
        // Don't flip if on cooldown unless forced by direct call
        if (notifyOpponent && Time.time - lastFlipTime < flipCooldown) return;
        
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        
        // Record flip time
        lastFlipTime = Time.time;
        
        // Update direction arrow when character flips
        UpdateDirectionArrow();
        
        // If this character flipped, also check if opponent needs to flip to face this character
        // But only if notifyOpponent is true to prevent infinite recursion
        if (notifyOpponent && opponentTransform != null)
        {
            FighterController opponentController = opponentTransform.GetComponent<FighterController>();
            if (opponentController != null)
            {
                // Check if opponent is facing the wrong way after this flip
                bool opponentFacingRight = opponentTransform.localScale.x > 0;
                bool opponentShouldFaceRight = opponentTransform.position.x < transform.position.x;
                
                if ((opponentFacingRight && !opponentShouldFaceRight) || 
                    (!opponentFacingRight && opponentShouldFaceRight))
                {
                    // Allow opponent flipping if not blocking and either can move OR is in a jump state
                    bool opponentCanFlip = !opponentController.isBlocking && 
                                          (opponentController.stateMachine.CanMove() || opponentController.IsInJumpState());
                    
                    if (opponentCanFlip)
                    {
                        // Pass false for notifyOpponent to prevent infinite recursion
                        opponentController.FlipCharacter(false);
                    }
                }
            }
        }
    }
    
    private void CreateStateText()
    {
        // Create a game object for the state text that's completely independent of character's transform
        stateTextObj = new GameObject($"{gameObject.name}_StateText");
        stateTextObj.transform.SetParent(null); // Not parented to character to avoid scaling/rotation issues
        stateTextObj.transform.position = transform.position + new Vector3(0, 2.5f, 0);
        
        // Add TextMeshPro component
        stateText = stateTextObj.AddComponent<TextMeshPro>();
        stateText.alignment = TextAlignmentOptions.Center;
        stateText.fontSize = 3;
        stateText.color = stateTextColor;
        stateText.text = "Idle";
        
        // Set the layer for proper rendering
        stateText.sortingOrder = 100;
        
        // Set visibility based on setting
        stateText.enabled = showStateText;
    }
    
    private void CreateDirectionArrow()
    {
        // Create a game object for the direction arrow that's independent of character's transform
        directionArrowObj = new GameObject($"{gameObject.name}_DirectionArrow");
        directionArrowObj.transform.SetParent(null);
        directionArrowObj.transform.position = transform.position + new Vector3(0, 1.5f, 0);
        
        // Add TextMeshPro component
        directionArrow = directionArrowObj.AddComponent<TextMeshPro>();
        directionArrow.alignment = TextAlignmentOptions.Center;
        directionArrow.fontSize = 5;
        directionArrow.color = directionArrowColor;
        
        // Set arrow based on initial direction
        UpdateDirectionArrow();
        
        // Set the layer for proper rendering
        directionArrow.sortingOrder = 100;
        
        // Set visibility based on setting
        directionArrow.enabled = showDirectionArrow;
    }
    
    private void UpdateStateTextPosition()
    {
        if (stateTextObj != null)
        {
            // Position text above player's current position (not parented)
            stateTextObj.transform.position = transform.position + new Vector3(0, 2.5f, 0);
            
            // Ensure text is always upright in world space
            stateTextObj.transform.rotation = Quaternion.identity;
            
            // Ensure text is always properly sized
            stateTextObj.transform.localScale = Vector3.one;
        }
        
        if (directionArrowObj != null)
        {
            // Position arrow above player's current position but below state text
            directionArrowObj.transform.position = transform.position + new Vector3(0, 1.5f, 0);
            
            // Ensure arrow is always upright in world space
            directionArrowObj.transform.rotation = Quaternion.identity;
            
            // Ensure arrow is always properly sized
            directionArrowObj.transform.localScale = Vector3.one;
        }
    }
    
    public void UpdateStateText(string stateName){
        currentStateName = stateName;
        
        // Update the floating text display
        if (stateText != null)
        {
            // Include health information in the state text
            string healthText = $" ({currentHealth:F0}/{maxHealth:F0})";
            
            // Add damage information if recently hit
            if (!string.IsNullOrEmpty(lastAttackTaken) && Time.time - Time.fixedTime < 2f)
            {
                healthText += $"\nHit by {lastAttackTaken}: -{lastDamageTaken:F1}";
            }
            
            stateText.text = stateName + healthText;
            
            // Optionally change color based on state type
            if (stateName.Contains("Punch") || stateName.Contains("Kick"))
                stateText.color = Color.red;
            else if (stateName.Contains("Block") || stateName.Contains("Hit"))
                stateText.color = Color.blue;
            else if (stateName.Contains("Jump"))
                stateText.color = Color.green;
            else if (!IsAlive)
                stateText.color = Color.gray;
            else
                stateText.color = stateTextColor;
        }
    }
    
    private void UpdateDirectionArrow()
    {
        if (directionArrow != null)
        {
            // Set arrow to point in the direction the character is facing
            directionArrow.text = transform.localScale.x > 0 ? "→" : "←";
        }
    }
    
    // Toggle state text visibility
    public void ToggleStateText(bool show)
    {
        showStateText = show;
        if (stateText != null)
            stateText.enabled = show;
    }
    
    // Toggle direction arrow visibility
    public void ToggleDirectionArrow(bool show)
    {
        showDirectionArrow = show;
        if (directionArrow != null)
            directionArrow.enabled = show;
    }
    

    
    private void HandleDirectInput() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        ProcessMoveInput(new Vector2(horizontal, vertical));
        
        if (stateMachine.CurrentStateType != typeof(DashState)) {
            if (Input.GetButtonDown("Fire1")) {
                OnPunch();
            }
            
            if (Input.GetButtonDown("Fire2")) {
                OnKick();
            }
            
            if (Input.GetButtonDown("Fire3")) {
                OnSpecial();
            }
        }
        
        if (Input.GetButtonDown("Jump")) {
            OnJump();
        }
        
        // Manual dash input removed - now uses double-tap detection
    }
    
    // Called by PlayerInputHandler or HandleDirectInput to process movement
    public void SetMoveInput(Vector2 newMoveInput) {
        useDirectInput = false;
        ProcessMoveInput(newMoveInput);
    }
    
    // Process move input from any source
    private void ProcessMoveInput(Vector2 newMoveInput) {
        // Check for direction change
        if (Mathf.Sign(newMoveInput.x) != Mathf.Sign(lastMoveInputX) && 
            Mathf.Abs(newMoveInput.x) > 0.1f && Mathf.Abs(lastMoveInputX) > 0.1f) 
        {
            lastDirectionChangeTime = Time.time;
        }
        
        // Double-tap dash detection for direct input
        if (useDirectInput)
        {
            // For left direction
            if (newMoveInput.x < -0.5f && wasLeftKeyUp)
            {
                // Left key was just pressed (after being up)
                wasLeftKeyUp = false;
                if (Time.time - lastLeftTapTime < doubleTapTimeThreshold)
                {
                    // Left double tap detected
                    TryDash(Vector2.left);
                }
                lastLeftTapTime = Time.time;
            }
            else if (newMoveInput.x >= -0.5f)
            {
                // Left key is up
                wasLeftKeyUp = true;
            }
            
            // For right direction
            if (newMoveInput.x > 0.5f && wasRightKeyUp)
            {
                // Right key was just pressed (after being up)
                wasRightKeyUp = false;
                if (Time.time - lastRightTapTime < doubleTapTimeThreshold)
                {
                    // Right double tap detected
                    TryDash(Vector2.right);
                }
                lastRightTapTime = Time.time;
            }
            else if (newMoveInput.x <= 0.5f)
            {
                // Right key is up
                wasRightKeyUp = true;
            }
        }
        
        lastMoveInputX = newMoveInput.x;
        
        if (newMoveInput != moveInput) {
            moveInput = newMoveInput;
            
            // Check for blocking (holding back)
            // This properly accounts for character's facing direction
            isBlocking = (transform.localScale.x > 0 && moveInput.x < -0.5f) || 
                        (transform.localScale.x < 0 && moveInput.x > 0.5f);
                        
            bool wasCrouching = isCrouching;
            isCrouching = moveInput.y < -0.5f;
            
            if (isCrouching && !wasCrouching && stateMachine.CanMove()) {
                stateMachine.ChangeState(new CrouchState());
            } else if (!isCrouching && wasCrouching && stateMachine.CurrentStateType == typeof(CrouchState)) {
                stateMachine.ChangeState(new IdleState());
            }
            
            // Add directional inputs to buffer separately for better sequence detection
            if (moveInput.x < -0.5f)
                inputBuffer.AddInput(new InputCommand(InputType.Left));
            else if (moveInput.x > 0.5f)
                inputBuffer.AddInput(new InputCommand(InputType.Right));
            else if (Mathf.Abs(lastMoveInputX) > 0.5f) // Was moving, now neutral
                inputBuffer.AddInput(new InputCommand(InputType.Neutral));
                
            // Remove or comment this line to prevent up movement from interfering
            // if (moveInput.y > 0.5f)
            //     inputBuffer.AddInput(new InputCommand(InputType.Up));
        }
    }
    
    private void FixedUpdate(){
        if (stateMachine.CanMove())
            HandleMovement();
    }
    
    public void OnJump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            
            // Change to jump start state
            stateMachine.ChangeState(new JumpStartState());
            
            // Add a small delay before allowing grounding again to prevent immediate re-grounding
            StartCoroutine(PreventImmediateGrounding());
            
            inputBuffer.AddInput(new InputCommand(InputType.Jump));
        }
    }
    
    // Coroutine to prevent immediate re-grounding after jump
    private IEnumerator PreventImmediateGrounding()
    {
        yield return new WaitForSeconds(0.1f); // Small delay to allow jump to initiate properly
    }
    
    public void OnPunch(){
        inputBuffer.AddInput(new InputCommand(InputType.Punch));
        
        if (!IsInAttackState()) {
            // Clear any movement velocity immediately for responsive attacks
            rb.velocity = new Vector2(0f, rb.velocity.y);
            
            // Force movement input to neutral to prevent interference
            moveInput = Vector2.zero;
                
            if (isCrouching)
                stateMachine.ChangeState(new CrouchPunchState());
            else if (!isGrounded)
                stateMachine.ChangeState(new JumpPunchState());
            else
                stateMachine.ChangeState(new PunchState());
        }
    }
    
    public void OnKick(){
        inputBuffer.AddInput(new InputCommand(InputType.Kick));
        
        if (!IsInAttackState()) {
            // Stop horizontal movement for any attack
            rb.velocity = new Vector2(0f, rb.velocity.y);
                
            if (isCrouching)
                stateMachine.ChangeState(new CrouchKickState());
            else if (!isGrounded)
                stateMachine.ChangeState(new JumpKickState());
            else
                stateMachine.ChangeState(new KickState());
        }
    }
    
    public void OnSpecial(){
        inputBuffer.AddInput(new InputCommand(InputType.Special));
        
        if (!IsInAttackState() && specialBarCurrent >= specialBarMax){
            // Stop horizontal movement for any attack
            rb.velocity = new Vector2(0f, rb.velocity.y);
                
            stateMachine.ChangeState(new SpecialMoveState());
            specialBarCurrent = 0;
        }
    }
    
    // Exposed method for dashing that can be called by PlayerInputHandler
    public void TryDash(Vector2 direction) {
        // Can't dash while attacking or stunned
        if (!canDash || !stateMachine.CanMove() || IsInAttackState() || stateMachine.CurrentStateType == typeof(HitState))
            return;
        
        // Always ensure dash is in left or right direction
        Vector2 dashDir = new Vector2(Mathf.Sign(direction.x), 0);
        
        // If we're moving in our facing direction, just dash in that direction
        // If we're moving opposite to facing, we might need to flip
        if (Mathf.Sign(dashDir.x) != Mathf.Sign(transform.localScale.x)) {
            // We're dashing backward - no need to flip
            dashDir.x = -Mathf.Sign(transform.localScale.x);
        } else {
            // We're dashing forward
            dashDir.x = Mathf.Sign(transform.localScale.x);
        }
        
        // Stop horizontal momentum before dashing
        rb.velocity = new Vector2(0f, rb.velocity.y);
        
        // Start dash
        StartCoroutine(DashCoroutine(dashDir));
        inputBuffer.AddInput(new InputCommand(InputType.Dash));
    }
    
    // Called by external input handlers to trigger a dash
    public void OnDash(Vector2 direction) {
        TryDash(direction);
    }
    
    private IEnumerator DashCoroutine(Vector2 direction) {
        canDash = false;
        lastDashTime = Time.time;
        
        // Always make crouch character stand up when dashing
        bool wasCrouching = isCrouching;
        isCrouching = false;
        
        // Change state to dash
        stateMachine.ChangeState(new DashState());
        
        // Apply dash velocity
        rb.velocity = new Vector2(direction.x * dashSpeed, rb.velocity.y);
        
        // Track dash duration
        dashTimer = dashDuration;
        
        // Wait for dash to complete
        yield return new WaitForSeconds(dashDuration);
        
        // Return to appropriate state
        if (stateMachine.CurrentStateType == typeof(DashState)) {
            stateMachine.ChangeState(new IdleState());
        }
        
        // Apply dash cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    
    private void HandleMovement(){
        if (dashTimer > 0){
            dashTimer -= Time.deltaTime;
            return;
        }
        
        float direction = moveInput.x;
        float currentHorizontalVelocity = rb.velocity.x;
        float targetVelocity = direction * moveSpeed;
        
        if (isCrouching) 
            targetVelocity *= 0.5f;
        
        if (isBlocking)
            targetVelocity *= 0.3f;
        
        float acceleration = MOVE_ACCELERATION;
        
        if (Mathf.Abs(direction) < 0.1f || Mathf.Sign(direction) != Mathf.Sign(currentHorizontalVelocity))
            acceleration = MOVE_DECELERATION;
        
        float newVelocity = Mathf.MoveTowards(currentHorizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        
        rb.velocity = new Vector2(newVelocity, rb.velocity.y);
            
        UpdateAnimatorMovement();
    }
    
    private void UpdateAnimatorMovement(){
        animator.SetFloat("HorizontalSpeed", Mathf.Abs(moveInput.x));
        animator.SetBool("IsCrouching", isCrouching);
        
        // Handle IsGrounded parameter carefully for jump states
        // Only update IsGrounded if we're not in JumpLoopState, or if we actually became grounded
        if (stateMachine.CurrentStateType != typeof(JumpLoopState) || isGrounded)
        {
            animator.SetBool("IsGrounded", isGrounded);
        }
        
        animator.SetBool("IsBlocking", isBlocking);
    }
    
    private bool IsInAttackState() {
        Type currentStateType = stateMachine.CurrentStateType;
        
        return 
            currentStateType == typeof(PunchState) ||
            currentStateType == typeof(KickState) ||
            currentStateType == typeof(CrouchPunchState) ||
            currentStateType == typeof(CrouchKickState) ||
            currentStateType == typeof(JumpPunchState) ||
            currentStateType == typeof(JumpKickState) ||
            currentStateType == typeof(SpecialMoveState) ||
            currentStateType == typeof(DashState);
    }
    
    private bool IsInJumpState() {
        Type currentStateType = stateMachine.CurrentStateType;
        
        return 
            currentStateType == typeof(JumpStartState) ||
            currentStateType == typeof(JumpLoopState) ||
            currentStateType == typeof(JumpPunchState) ||
            currentStateType == typeof(JumpKickState);
    }
    
    // Set the opponent transform referenc
    public void SetOpponent(Transform opponent)
    {
        opponentTransform = opponent;
        
        // Initialize pass-by detection
        if (opponentTransform != null)
        {
            lastPlayerPosX = transform.position.x;
            lastOpponentPosX = opponentTransform.position.x;
        }
    }
    
    // Method to trigger animation by state type
    public void TriggerAnimationByState(Type stateType)
    {
        // Get the animator component
        if (animator == null) return;
        
        // Trigger animations based on state type
        if (stateType == typeof(KickState))
        {
            animator.SetTrigger("Kick");
        }
        else if (stateType == typeof(PunchState))
        {
            animator.SetTrigger("Punch");
        }
        else if (stateType == typeof(CrouchKickState))
        {
            animator.SetTrigger("CrouchKick");
        }
        else if (stateType == typeof(CrouchPunchState))
        {
            animator.SetTrigger("CrouchPunch");
        }
        else if (stateType == typeof(JumpKickState))
        {
            animator.SetTrigger("Kick");
        }
        else if (stateType == typeof(JumpPunchState))
        {
            animator.SetTrigger("Punch");
        }
        else if (stateType == typeof(SpecialMoveState))
        {
            animator.SetTrigger("Special");
        }
        else if (stateType == typeof(DashState))
        {
            animator.SetTrigger("Dash");
        }
        else if (stateType == typeof(JumpStartState))
        {
            animator.SetTrigger("JumpStart");
            // Reset JumpLoop trigger to prevent conflicts
            animator.ResetTrigger("JumpLoop");
        }
        else if (stateType == typeof(JumpLoopState))
        {
            // For jump loop, we need to make sure we're properly in the jump loop state
            // Reset any competing triggers first
            animator.ResetTrigger("JumpStart");
            
            // Trigger the jump loop animation
            animator.SetTrigger("JumpLoop");
            
            // Force the animator to be in the correct state by ensuring IsGrounded is false
            // This prevents the animator from immediately transitioning back to idle
            animator.SetBool("IsGrounded", false);
        }
        else if (stateType == typeof(HitState))
        {
            animator.SetTrigger("Hit");
        }
        else if (stateType == typeof(WinState))
        {
            animator.SetTrigger("Win");
        }
        else if (stateType == typeof(DeathState))
        {
            animator.SetTrigger("Death");
        }
       
    }
    
    public void GainSpecialMeter(float amount){
        specialBarCurrent = Mathf.Min(specialBarCurrent + amount, specialBarMax);
    }
    
    private void OnCollisionEnter2D(Collision2D collision){
        // Check if colliding with ground and if the collision is from above
        if (collision.gameObject.CompareTag("Ground") && 
            collision.contacts.Length > 0 && 
            collision.contacts[0].normal.y > 0.7f) {
            isGrounded = true;
            Debug.Log("Grounded via collision enter");
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision) {
        // Check if staying on ground and if the collision is from above
        if (collision.gameObject.CompareTag("Ground") && 
            collision.contacts.Length > 0 && 
            collision.contacts[0].normal.y > 0.7f) {
            isGrounded = true;
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Ground")) {
            isGrounded = false;
            Debug.Log("Left ground via collision exit");
        }
    }
    
    // Debug helper method to track current animation state
    public void LogCurrentAnimationState()
    {
        if (animator != null)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"Current Animation: {currentState.shortNameHash}, " +
                      $"IsGrounded: {isGrounded}, " +
                      $"State Machine: {stateMachine.CurrentStateType.Name}, " +
                      $"Velocity: {rb.velocity}");
        }
    }
    
    // Method to force jump loop for testing
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void ForceJumpLoop()
    {
        if (!isGrounded)
        {
            stateMachine.ChangeState(new JumpLoopState());
        }
    }
    
    // Health and damage system methods
    private void OnHitTaken(float damage, Transform attacker, string attackType)
    {
        if (!IsAlive) return; // Don't take damage if already dead
        
        // Store hit information for debug display
        lastDamageTaken = damage;
        lastAttackTaken = attackType;
        lastAttacker = attacker;
        
        // Apply damage
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Debug log
        string attackerName = attacker != null ? attacker.name : "Unknown";
        Debug.Log($"{gameObject.name} took {damage:F1} damage from {attackerName}'s {attackType}. Health: {currentHealth:F1}/{maxHealth:F1}");
        
        // Update state text immediately to show new health
        UpdateStateText(currentStateName);
        
        // Check if character died
        if (!IsAlive)
        {
            OnDeath();
        }
        else
        {
            // Trigger hit state if not already in one and not blocking
            if (stateMachine.CurrentStateType != typeof(HitState) && !isBlocking)
            {
                stateMachine.ChangeState(new HitState());
            }
        }
    }
    
    private void OnDeath()
    {
        Debug.Log($"{gameObject.name} has been defeated!");
        
        // Stop all movement
        rb.velocity = Vector2.zero;
        
        // Transition to death state instead of just disabling
        stateMachine.ChangeState(new DeathState());
        
        // Update state text to show death
        if (stateText != null)
        {
            stateText.text = "DEFEATED";
            stateText.color = Color.red;
        }
        
        // Check if opponent should win
        CheckOpponentWinCondition();
        if (roundManager != null)
        {
            roundManager.OnFighterDefeated(this);
        }
    }
    
    private void CheckOpponentWinCondition()
    {
        if (opponentTransform != null)
        {
            FighterController opponent = opponentTransform.GetComponent<FighterController>();
            if (opponent != null && opponent.IsAlive)
            {
                opponent.TriggerWin();
            }
        }
    }
    
    public void TriggerWin()
    {
        if (stateMachine.CurrentStateType == typeof(WinState)) return; // Already in win state
        
        Debug.Log($"{gameObject.name} has won the match!");
        
        // Trigger win state
        stateMachine.ChangeState(new WinState());
        
        // Update state text to show victory
        if (stateText != null)
        {
            stateText.text = "VICTORY!";
            stateText.color = Color.yellow;
        }
        
        // Disable input for the winner (they're celebrating)
        enabled = false;
    }
    
    // Public method to heal (useful for testing or special abilities)
    public void Heal(float amount)
    {
        if (!IsAlive) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        UpdateStateText(currentStateName);
        
        Debug.Log($"{gameObject.name} healed for {amount:F1}. Health: {currentHealth:F1}/{maxHealth:F1}");
    }
    
    // Public method to reset health (useful for round resets)
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        lastAttackTaken = "";
        lastDamageTaken = 0f;
        lastAttacker = null;
        enabled = true; // Re-enable if was disabled due to death
        
        // Reset from win state if necessary
        if (stateMachine.CurrentStateType == typeof(WinState))
        {
            stateMachine.ChangeState(new IdleState());
        }
        
        // Reset from death state if necessary
        if (stateMachine.CurrentStateType == typeof(DeathState))
        {
            stateMachine.ChangeState(new IdleState());
        }
        
        UpdateStateText(currentStateName);
        
        Debug.Log($"{gameObject.name} health reset to {maxHealth:F1}");
    }
    
    // Manual win trigger for testing
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void TriggerWinForTesting()
    {
        TriggerWin();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (hurtboxController != null)
        {
            hurtboxController.OnHitTaken -= OnHitTaken;
        }
        
        // Cleanup - destroy the state text and direction arrow objects when fighter is destroyed
        if (stateTextObj != null)
            Destroy(stateTextObj);
            
        if (directionArrowObj != null)
            Destroy(directionArrowObj);
    }
}