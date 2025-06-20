using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System;
using System.Collections.Generic;
using System.Linq;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    
    private InputActionMap uiActionMap;
    private InputActionMap player1ActionMap;
    private InputActionMap player2ActionMap;
    
    // --- Olaylar (Events) kısmı aynı kalıyor ---
    public static event Action<Vector2> OnUINavigate;
    public static event Action OnUISubmit, OnUICancel, OnPause;
    public static event Action<Vector2> OnPlayer1Move, OnPlayer2Move;
    public static event Action OnPlayer1Punch, OnPlayer1Kick, OnPlayer1Special, OnPlayer1Jump;
    public static event Action OnPlayer2Punch, OnPlayer2Kick, OnPlayer2Special, OnPlayer2Jump;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DebugAllDevices(); // YENİ EKLENEN SATIR
            InitializeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeManager()
    {
        InitializeInputActions();
        InputSystem.onDeviceChange += OnDeviceChange;
        AssignDevicesAndSchemes();
    
        // YENİ EKLENEN SATIR: Başlangıç durumu olarak UI kontrollerini aktif et.
        EnableUIControls();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed || change == InputDeviceChange.Reconnected)
        {
            Debug.Log($"Device {change}: {device.name}");
            AssignDevicesAndSchemes();
        }
    }
    
    private void AssignDevicesAndSchemes()
    {
        var gamepads = Gamepad.all;
        var keyboard = Keyboard.current;

        player1ActionMap.Disable();
        player2ActionMap.Disable();

        Debug.Log($"Found {gamepads.Count} gamepads.");

        if (gamepads.Count >= 2)
        {
            Debug.Log("Assigning Gamepad to P1 and Gamepad to P2.");
            player1ActionMap.devices = new InputDevice[] { gamepads[0] };
            player2ActionMap.devices = new InputDevice[] { gamepads[1] };
        }
        else if (gamepads.Count == 1)
        {
            Debug.Log("Assigning Keyboard to P1 and Gamepad to P2."); // Log mesajını da güncelledim
            player1ActionMap.devices = new InputDevice[] { keyboard };   // P1 Klavyeyi Alır
            player2ActionMap.devices = new InputDevice[] { gamepads[0] }; // P2 Kolu Alır
        }
        else // 0 gamepad
        {
            Debug.Log("No gamepads found. Assigning Keyboard to P1 and P2.");
            player1ActionMap.devices = new InputDevice[] { keyboard };
            player2ActionMap.devices = new InputDevice[] { keyboard };
        }
        
        EnableGameplayControls();
    }
    
    private void InitializeInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputManager: Input Actions asset is not assigned!");
            return;
        }
        
        uiActionMap = inputActions.FindActionMap("UI");
        player1ActionMap = inputActions.FindActionMap("Player1");
        player2ActionMap = inputActions.FindActionMap("Player2");

        SubscribeToEvents();
    }
    
    private void SubscribeToEvents()
    {
        // UI Actions
        uiActionMap.FindAction("Navigate").performed += ctx => OnUINavigate?.Invoke(ctx.ReadValue<Vector2>());
        uiActionMap.FindAction("Submit").performed += _ => OnUISubmit?.Invoke();
        uiActionMap.FindAction("Cancel").performed += _ => OnUICancel?.Invoke();
        inputActions.FindAction("Pause").performed += _ => OnPause?.Invoke();

        // Player 1 Actions
        player1ActionMap.FindAction("Move").performed += ctx => OnPlayer1Move?.Invoke(ctx.ReadValue<Vector2>());
        player1ActionMap.FindAction("Move").canceled += _ => OnPlayer1Move?.Invoke(Vector2.zero);
        player1ActionMap.FindAction("Punch").performed += _ => OnPlayer1Punch?.Invoke();
        player1ActionMap.FindAction("Kick").performed += _ => OnPlayer1Kick?.Invoke();
        player1ActionMap.FindAction("Special").performed += _ => OnPlayer1Special?.Invoke();
        player1ActionMap.FindAction("Jump").performed += _ => OnPlayer1Jump?.Invoke();
        
        // Player 2 Actions
        player2ActionMap.FindAction("Move").performed += ctx => OnPlayer2Move?.Invoke(ctx.ReadValue<Vector2>());
        player2ActionMap.FindAction("Move").canceled += _ => OnPlayer2Move?.Invoke(Vector2.zero);
        player2ActionMap.FindAction("Punch").performed += _ => OnPlayer2Punch?.Invoke();
        player2ActionMap.FindAction("Kick").performed += _ => OnPlayer2Kick?.Invoke();
        player2ActionMap.FindAction("Special").performed += _ => OnPlayer2Special?.Invoke();
        player2ActionMap.FindAction("Jump").performed += _ => OnPlayer2Jump?.Invoke();
    }
    
    public void EnableUIControls()
    {
        player1ActionMap?.Disable();
        player2ActionMap?.Disable();
        uiActionMap?.Enable();
    }
    
    public void EnableSplitScreenUIControls()
    {
        uiActionMap?.Enable();
        player1ActionMap?.Enable();
        player2ActionMap?.Enable();
    }
    
    public void EnableGameplayControls()
    {
        uiActionMap?.Disable();
        player1ActionMap?.Enable();
        player2ActionMap?.Enable();
    }
    
    // ... (ClearAllEventSubscriptions, OnDestroy vb. aynı kalabilir) ...
    public static void ClearAllEventSubscriptions()
    {
        // ...
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }
    }
    // YENİ FONKSİYON
    void DebugAllDevices()
    {
        Debug.Log("---------- DETECTED INPUT DEVICES ----------");
        foreach (var device in InputSystem.devices)
        {
            // Sadece klavye ve gamepad/joystickleri listeyelelim
            if(device is Keyboard || device is Gamepad || device is Joystick)
            {
                Debug.Log($"Device: {device.name}, DisplayName: {device.displayName}, Path: {device.path}");
            }
        }
        Debug.Log("-------------------------------------------");
    }
}