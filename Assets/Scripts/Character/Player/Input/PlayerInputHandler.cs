using UnityEngine;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    public event Action OnJumpRequested;
    public event Action OnJumpHeld;
    public event Action OnJumpReleased;

    [Header("Input Properties")]
    public Vector2 MoveInput { get; private set; }
    public bool IsJumpHeld { get; private set; }

    void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpStarted += HandleJumpStarted;
            InputManager.Instance.OnJumpPerformed += HandleJumpPerformed;
            InputManager.Instance.OnJumpCanceled += HandleJumpCanceled;
        }
    }

    void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnJumpStarted -= HandleJumpStarted;
            InputManager.Instance.OnJumpPerformed -= HandleJumpPerformed;
            InputManager.Instance.OnJumpCanceled -= HandleJumpCanceled;
        }
    }

    void Update()
    {
        // Update movement input every frame
        if (InputManager.Instance != null)
        {
            MoveInput = InputManager.Instance.MoveInput;
        }
    }

    private void HandleJumpStarted()
    {
        IsJumpHeld = true;
        OnJumpRequested?.Invoke();
    }

    private void HandleJumpPerformed()
    {
        IsJumpHeld = true;
        OnJumpHeld?.Invoke();
    }

    private void HandleJumpCanceled()
    {
        IsJumpHeld = false;
        OnJumpReleased?.Invoke();
    }
}
