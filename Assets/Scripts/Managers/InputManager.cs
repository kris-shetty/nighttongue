using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions _controls;

    public Vector2 MoveInput { get; private set; }

    public delegate void JumpEvent();
    public event JumpEvent OnJumpStarted;
    public event JumpEvent OnJumpPerformed;
    public event JumpEvent OnJumpCanceled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _controls = new InputSystem_Actions();

        _controls.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        _controls.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        _controls.Player.Jump.started += ctx => { OnJumpStarted?.Invoke(); };
        _controls.Player.Jump.performed += ctx => { OnJumpPerformed?.Invoke(); };
        _controls.Player.Jump.canceled += ctx => { OnJumpCanceled?.Invoke(); };
    }

    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }
}
