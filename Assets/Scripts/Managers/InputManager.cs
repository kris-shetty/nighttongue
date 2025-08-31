using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions _controls;

    public Vector2 MoveInput { get; private set; }

    public event Action OnJumpStarted;
    public event Action OnJumpPerformed;
    public event Action OnJumpCanceled;

    // Store delegates
    private Action<InputAction.CallbackContext> _onMovePerformed;
    private Action<InputAction.CallbackContext> _onMoveCanceled;
    private Action<InputAction.CallbackContext> _onJumpStarted;
    private Action<InputAction.CallbackContext> _onJumpPerformed;
    private Action<InputAction.CallbackContext> _onJumpCanceled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _controls = new InputSystem_Actions();

        // Create delegates
        _onMovePerformed = ctx => MoveInput = ctx.ReadValue<Vector2>();
        _onMoveCanceled = ctx => MoveInput = Vector2.zero;

        _onJumpStarted = ctx => OnJumpStarted?.Invoke();
        _onJumpPerformed = ctx => OnJumpPerformed?.Invoke();
        _onJumpCanceled = ctx => OnJumpCanceled?.Invoke();
    }

    private void OnEnable()
    {
        // Subscribe
        _controls.Player.Move.performed += _onMovePerformed;
        _controls.Player.Move.canceled += _onMoveCanceled;

        _controls.Player.Jump.started += _onJumpStarted;
        _controls.Player.Jump.performed += _onJumpPerformed;
        _controls.Player.Jump.canceled += _onJumpCanceled;

        _controls.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe
        _controls.Player.Move.performed -= _onMovePerformed;
        _controls.Player.Move.canceled -= _onMoveCanceled;

        _controls.Player.Jump.started -= _onJumpStarted;
        _controls.Player.Jump.performed -= _onJumpPerformed;
        _controls.Player.Jump.canceled -= _onJumpCanceled;

        _controls.Disable();
    }
}
