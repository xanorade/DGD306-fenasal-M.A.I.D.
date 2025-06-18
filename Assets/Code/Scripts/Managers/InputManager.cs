using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    
    // Input Action Maps
    private InputActionMap uiActionMap;
    private InputActionMap player1ActionMap;
    private InputActionMap player2ActionMap;
    
    // UI Actions
    public static event Action<Vector2> OnUINavigate;
    public static event Action OnUISubmit;
    public static event Action OnUICancel;
    
    // Player 1 Actions
    public static event Action<Vector2> OnPlayer1Move;
    public static event Action OnPlayer1Punch;
    public static event Action OnPlayer1Kick;
    public static event Action OnPlayer1Special;
    public static event Action OnPlayer1Jump;
    
    // Player 2 Actions  
    public static event Action<Vector2> OnPlayer2Move;
    public static event Action OnPlayer2Punch;
    public static event Action OnPlayer2Kick;
    public static event Action OnPlayer2Special;
    public static event Action OnPlayer2Jump;
    
    // Pause Action
    public static event Action OnPause;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInputActions();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeInputActions()
    {
        // Get action maps
        uiActionMap = inputActions.FindActionMap("UI");
        player1ActionMap = inputActions.FindActionMap("Player1");
        player2ActionMap = inputActions.FindActionMap("Player2");
        
        // Subscribe to UI actions
        uiActionMap.FindAction("Navigate").performed += OnNavigatePerformed;
        uiActionMap.FindAction("Submit").performed += OnSubmitPerformed;
        uiActionMap.FindAction("Cancel").performed += OnCancelPerformed;
        
        // Subscribe to Player 1 actions
        player1ActionMap.FindAction("Move").performed += OnPlayer1MovePerformed;
        player1ActionMap.FindAction("Move").canceled += OnPlayer1MoveCanceled;
        player1ActionMap.FindAction("Punch").performed += OnPlayer1PunchPerformed;
        player1ActionMap.FindAction("Kick").performed += OnPlayer1KickPerformed;
        player1ActionMap.FindAction("Special").performed += OnPlayer1SpecialPerformed;
        player1ActionMap.FindAction("Jump").performed += OnPlayer1JumpPerformed;
        
        // Subscribe to Player 2 actions
        player2ActionMap.FindAction("Move").performed += OnPlayer2MovePerformed;
        player2ActionMap.FindAction("Move").canceled += OnPlayer2MoveCanceled;
        player2ActionMap.FindAction("Punch").performed += OnPlayer2PunchPerformed;
        player2ActionMap.FindAction("Kick").performed += OnPlayer2KickPerformed;
        player2ActionMap.FindAction("Special").performed += OnPlayer2SpecialPerformed;
        player2ActionMap.FindAction("Jump").performed += OnPlayer2JumpPerformed;
        
        // Subscribe to pause action
        inputActions.FindAction("Pause").performed += OnPausePerformed;
    }
    
    // Enable/Disable action maps based on game state
    public void EnableUIControls()
    {
        uiActionMap?.Enable();
        player1ActionMap?.Disable();
        player2ActionMap?.Disable();
    }
    
    public void EnableGameplayControls()
    {
        uiActionMap?.Disable();
        player1ActionMap?.Enable();
        player2ActionMap?.Enable();
    }
    
    public void EnableAllControls()
    {
        inputActions?.Enable();
    }
    
    public void DisableAllControls()
    {
        inputActions?.Disable();
    }
    
    // UI Event Handlers
    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        OnUINavigate?.Invoke(context.ReadValue<Vector2>());
    }
    
    private void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        OnUISubmit?.Invoke();
    }
    
    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        OnUICancel?.Invoke();
    }
    
    // Player 1 Event Handlers
    private void OnPlayer1MovePerformed(InputAction.CallbackContext context)
    {
        OnPlayer1Move?.Invoke(context.ReadValue<Vector2>());
    }
    
    private void OnPlayer1MoveCanceled(InputAction.CallbackContext context)
    {
        OnPlayer1Move?.Invoke(Vector2.zero);
    }
    
    private void OnPlayer1PunchPerformed(InputAction.CallbackContext context)
    {
        OnPlayer1Punch?.Invoke();
    }
    
    private void OnPlayer1KickPerformed(InputAction.CallbackContext context)
    {
        OnPlayer1Kick?.Invoke();
    }
    
    private void OnPlayer1SpecialPerformed(InputAction.CallbackContext context)
    {
        OnPlayer1Special?.Invoke();
    }
    
    private void OnPlayer1JumpPerformed(InputAction.CallbackContext context)
    {
        OnPlayer1Jump?.Invoke();
    }
    
    // Player 2 Event Handlers
    private void OnPlayer2MovePerformed(InputAction.CallbackContext context)
    {
        OnPlayer2Move?.Invoke(context.ReadValue<Vector2>());
    }
    
    private void OnPlayer2MoveCanceled(InputAction.CallbackContext context)
    {
        OnPlayer2Move?.Invoke(Vector2.zero);
    }
    
    private void OnPlayer2PunchPerformed(InputAction.CallbackContext context)
    {
        OnPlayer2Punch?.Invoke();
    }
    
    private void OnPlayer2KickPerformed(InputAction.CallbackContext context)
    {
        OnPlayer2Kick?.Invoke();
    }
    
    private void OnPlayer2SpecialPerformed(InputAction.CallbackContext context)
    {
        OnPlayer2Special?.Invoke();
    }
    
    private void OnPlayer2JumpPerformed(InputAction.CallbackContext context)
    {
        OnPlayer2Jump?.Invoke();
    }
    
    // Pause Event Handler
    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        OnPause?.Invoke();
    }
    
    private void OnEnable()
    {
        inputActions?.Enable();
    }
    
    private void OnDisable()
    {
        inputActions?.Disable();
    }
}
