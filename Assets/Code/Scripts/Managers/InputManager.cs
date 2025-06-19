using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    
    // Input Action Maps
    private InputActionMap uiActionMap;
    private InputActionMap player1ActionMap;
    private InputActionMap player2ActionMap;
    
    // Device Management
    private InputDevice player1Device;
    private InputDevice player2Device;
    private List<InputDevice> availableDevices = new List<InputDevice>();
    
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

    // Input action references
    private InputAction pauseAction;

    private void Awake()
    {
        Debug.Log("InputManager: Awake called");
        
        // Handle singleton logic more robustly
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("InputManager: Instance created and set to DontDestroyOnLoad");
            
            // Initialize everything for the first instance
            InitializeManager();
        }
        else if (Instance == this)
        {
            // This is the same instance being reactivated (shouldn't happen but just in case)
            Debug.Log("InputManager: Same instance reactivated");
            return;
        }
        else
        {
            // Check if the existing instance is still valid and functional
            if (Instance != null && Instance.gameObject != null)
            {
                Debug.Log("InputManager: Instance already exists and is valid, destroying duplicate");
                Destroy(gameObject);
                return;
            }
            else
            {
                // The existing instance is broken/destroyed, replace it
                Debug.LogWarning("InputManager: Existing instance was broken, replacing with new one");
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManager();
            }
        }
    }
    
    private void InitializeManager()
    {
        // Initialize available devices
        RefreshAvailableDevices();
        
        // Subscribe to device change events
        InputSystem.onDeviceChange += OnDeviceChange;
        
        // Initialize input actions
        InitializeInputActions();
    }
    
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed)
        {
            Debug.Log($"Device {change}: {device.name}");
            RefreshAvailableDevices();
        }
    }
    
    private void RefreshAvailableDevices()
    {
        availableDevices.Clear();
        
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad || device is Joystick)
            {
                availableDevices.Add(device);
                Debug.Log($"Available device: {device.name} ({device.GetType().Name}) - Path: {device.path}");
            }
        }
        
        Debug.Log($"Total joysticks/gamepads found: {availableDevices.Count}");
        
        // Assign devices to players if available
        AssignDevicesToPlayers();
    }
    
    private void AssignDevicesToPlayers()
    {
        // Reset device assignments
        player1Device = null;
        player2Device = null;
        
        // Assign first available joystick to Player 1
        if (availableDevices.Count > 0)
        {
            player1Device = availableDevices[0];
            Debug.Log($"Assigned Player 1: {player1Device.name} (Path: {player1Device.path})");
        }
        
        // Assign second available joystick to Player 2
        if (availableDevices.Count > 1)
        {
            player2Device = availableDevices[1];
            Debug.Log($"Assigned Player 2: {player2Device.name} (Path: {player2Device.path})");
        }
        else
        {
            Debug.LogWarning("Only one joystick found - Player 2 will not have controls");
        }
        
        // Update action maps with device assignments
        UpdateActionMaps();
    }
    
    private void UpdateActionMaps()
    {
        // Disable action maps first
        player1ActionMap?.Disable();
        player2ActionMap?.Disable();
        
        // Set up device restrictions for Player 1
        if (player1ActionMap != null && player1Device != null)
        {
            Debug.Log($"Setting up Player 1 controls for device: {player1Device.name}");
            
            // Create device array for Player 1
            InputDevice[] player1Devices = { player1Device };
            player1ActionMap.devices = player1Devices;
            player1ActionMap.Enable();
        }
        
        // Set up device restrictions for Player 2  
        if (player2ActionMap != null && player2Device != null)
        {
            Debug.Log($"Setting up Player 2 controls for device: {player2Device.name}");
            
            // Create device array for Player 2
            InputDevice[] player2Devices = { player2Device };
            player2ActionMap.devices = player2Devices;
            player2ActionMap.Enable();
        }
    }
    
    private void InitializeInputActions()
    {
        Debug.Log("InputManager: InitializeInputActions called");
        
        if (inputActions == null)
        {
            Debug.LogError("InputManager: No Input Actions asset assigned! Please assign the GameInputActions asset in the inspector.");
            return;
        }
        
        try
        {
            // Get action maps
            uiActionMap = inputActions.FindActionMap("UI");
            player1ActionMap = inputActions.FindActionMap("Player1");
            player2ActionMap = inputActions.FindActionMap("Player2");
            pauseAction = inputActions.FindAction("Pause");
            
            if (uiActionMap == null || player1ActionMap == null || player2ActionMap == null)
            {
                Debug.LogError("InputManager: One or more action maps not found!");
                return;
            }
            
            Debug.Log($"InputManager: Found action maps - UI: {uiActionMap != null}, Player1: {player1ActionMap != null}, Player2: {player2ActionMap != null}");
            
            // Enable UI action map only - player maps will be enabled with device restrictions
            uiActionMap.Enable();
            
            // Subscribe to events
            SubscribeToEvents();
            
            Debug.Log("InputManager: All actions initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"InputManager: Error initializing input actions: {e.Message}");
        }
    }
    
    private void SubscribeToEvents()
    {
        // UI Actions
        var navigate = uiActionMap.FindAction("Navigate");
        var submit = uiActionMap.FindAction("Submit");
        var cancel = uiActionMap.FindAction("Cancel");
        
        if (navigate != null) navigate.performed += OnNavigatePerformed;
        if (submit != null) submit.performed += OnSubmitPerformed;
        if (cancel != null) cancel.performed += OnCancelPerformed;
        
        // Player 1 Actions
        var p1Move = player1ActionMap.FindAction("Move");
        var p1Punch = player1ActionMap.FindAction("Punch");
        var p1Kick = player1ActionMap.FindAction("Kick");
        var p1Special = player1ActionMap.FindAction("Special");
        var p1Jump = player1ActionMap.FindAction("Jump");

        if (p1Move != null)
        {
            p1Move.performed += OnPlayer1MovePerformed;
            p1Move.canceled += OnPlayer1MoveCanceled;
        }
        if (p1Punch != null) p1Punch.performed += OnPlayer1PunchPerformed;
        if (p1Kick != null) p1Kick.performed += OnPlayer1KickPerformed;
        if (p1Special != null) p1Special.performed += OnPlayer1SpecialPerformed;
        if (p1Jump != null) p1Jump.performed += OnPlayer1JumpPerformed;
        
        // Player 2 Actions
        var p2Move = player2ActionMap.FindAction("Move");
        var p2Punch = player2ActionMap.FindAction("Punch");
        var p2Kick = player2ActionMap.FindAction("Kick");
        var p2Special = player2ActionMap.FindAction("Special");
        var p2Jump = player2ActionMap.FindAction("Jump");

        if (p2Move != null)
        {
            p2Move.performed += OnPlayer2MovePerformed;
            p2Move.canceled += OnPlayer2MoveCanceled;
        }
        if (p2Punch != null) p2Punch.performed += OnPlayer2PunchPerformed;
        if (p2Kick != null) p2Kick.performed += OnPlayer2KickPerformed;
        if (p2Special != null) p2Special.performed += OnPlayer2SpecialPerformed;
        if (p2Jump != null) p2Jump.performed += OnPlayer2JumpPerformed;
        
        // Subscribe to pause action
        if (pauseAction != null)
        {
            pauseAction.performed += OnPausePerformed;
            Debug.Log("InputManager: Pause action subscribed");
        }
    }
    
    // Enable/Disable action maps based on game state
    public void EnableUIControls()
    {
        Debug.Log("InputManager: EnableUIControls called");
        
        uiActionMap?.Enable();
        player1ActionMap?.Disable();
        player2ActionMap?.Disable();
        
        Debug.Log($"InputManager: UI enabled: {uiActionMap?.enabled}, Player1 disabled: {!player1ActionMap?.enabled}, Player2 disabled: {!player2ActionMap?.enabled}");
    }
    
    public void EnableGameplayControls()
    {
        Debug.Log("InputManager: EnableGameplayControls called");
        
        uiActionMap?.Disable();
        
        // Don't enable action maps directly - let UpdateActionMaps handle it with device restrictions
        UpdateActionMaps();
        
        Debug.Log($"InputManager: UI disabled: {!uiActionMap?.enabled}, Player1 enabled: {player1ActionMap?.enabled}, Player2 enabled: {player2ActionMap?.enabled}");
    }
    
    public void EnableAllControls()
    {
        Debug.Log("InputManager: EnableAllControls called");
        inputActions?.Enable();
    }
    
    public void DisableAllControls()
    {
        Debug.Log("InputManager: DisableAllControls called");
        inputActions?.Disable();
    }
    
    // UI Event Handlers
    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        Vector2 navigation = context.ReadValue<Vector2>();
        OnUINavigate?.Invoke(navigation);
    }
    
    private void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Submit performed");
        OnUISubmit?.Invoke();
    }
    
    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Cancel performed");
        OnUICancel?.Invoke();
    }
    
    // Player 1 Event Handlers
    private void OnPlayer1MovePerformed(InputAction.CallbackContext context)
    {
        Vector2 movement = context.ReadValue<Vector2>();
        Debug.Log($"InputManager: Player1 Move performed: {movement}");
        OnPlayer1Move?.Invoke(movement);
    }
    
    private void OnPlayer1MoveCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Player1 Move canceled");
        OnPlayer1Move?.Invoke(Vector2.zero);
    }
    
    private void OnPlayer1PunchPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Player1 Punch performed");
        OnPlayer1Punch?.Invoke();
    }
    
    private void OnPlayer1KickPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Player1 Kick performed");
        OnPlayer1Kick?.Invoke();
    }
    
    private void OnPlayer1SpecialPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Player1 Special performed");
        OnPlayer1Special?.Invoke();
    }
    
    private void OnPlayer1JumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Player1 Jump performed");
        OnPlayer1Jump?.Invoke();
    }
    
    // Player 2 Event Handlers
    private void OnPlayer2MovePerformed(InputAction.CallbackContext context)
    {
        Vector2 movement = context.ReadValue<Vector2>();
        Debug.Log($"InputManager: Player2 Move performed: {movement}");
        OnPlayer2Move?.Invoke(movement);
    }
    
    private void OnPlayer2MoveCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Player2 Move canceled");
        OnPlayer2Move?.Invoke(Vector2.zero);
    }
    
    private void OnPlayer2PunchPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Player2 Punch performed");
        OnPlayer2Punch?.Invoke();
    }
    
    private void OnPlayer2KickPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Player2 Kick performed");
        OnPlayer2Kick?.Invoke();
    }
    
    private void OnPlayer2SpecialPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Player2 Special performed");
        OnPlayer2Special?.Invoke();
    }
    
    private void OnPlayer2JumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Player2 Jump performed");
        OnPlayer2Jump?.Invoke();
    }
    
    // Pause Event Handler
    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        Debug.Log("InputManager: Pause performed");
        OnPause?.Invoke();
    }
    
    private void OnEnable()
    {
        Debug.Log("InputManager: OnEnable called");
        inputActions?.Enable();
    }
    
    private void OnDisable()
    {
        Debug.Log("InputManager: OnDisable called");
        inputActions?.Disable();
    }
    
    private void OnDestroy()
    {
        Debug.Log("InputManager: OnDestroy called");
        inputActions?.Disable();
    }
}
