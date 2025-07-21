using UnityEngine;
using System;
using System.Collections;

public class PlayerInputHandler : MonoBehaviour
{
    public event Action OnJumpRequested;
    public event Action OnJumpHeld;
    public event Action OnJumpReleased;

    [Header("Input Properties")]
    public Vector2 MoveInput { get; private set; }
    public bool IsJumpHeld { get; private set; }

    private bool _isFrozen = false;

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
            MoveInput = _isFrozen ? Vector2.zero : InputManager.Instance.MoveInput;
        }
    }

    private void ProcessInputWrapper(Action stateUpdate, Action inputEvent)
    {
        if (_isFrozen)
            return;

        stateUpdate?.Invoke();
        inputEvent?.Invoke();
    }

    private void HandleJumpStarted()
    {
        ProcessInputWrapper(() => IsJumpHeld = true, OnJumpRequested);
    }

    private void HandleJumpPerformed()
    {
        ProcessInputWrapper(() => IsJumpHeld = true, OnJumpHeld);
 
    }

    private void HandleJumpCanceled()
    {
        ProcessInputWrapper(() => IsJumpHeld = false, OnJumpReleased);
    }
    public void SetFrozen(bool frozen)
    {
        _isFrozen = frozen;
        if (frozen)
            MoveInput = Vector2.zero; // Clear input when frozen
    }
    public void FreezeInput(float duration)
    {
        SetFrozen(true);
        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        SetFrozen(false);
    }

}
