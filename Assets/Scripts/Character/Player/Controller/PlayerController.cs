using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

public class PlayerController : StateMachine, IForceReceiver
{
    [Header("State Machine Context")]
    public JumpActionSO JumpAction;
    public MoveActionSO MoveAction;
    public StateMultiplierSO StateMultiplier;
    public PlayerTimeSettingsSO TimeSettings;
    public GlobalPhysicsSettingsSO Settings;
    [SerializeField] private bool _usePrecisionMovement = false;

    // Input Handler
    private PlayerInputHandler _inputHandler;

    // Flags
    public bool RequestedJump = false;
    public bool HasCoyoteBuffered = false;
    public bool HasJumpBuffered = false;
    public bool RequestedGrapple = false;


    // Buffer Timers
    private float _elapsedJumpBufferTime = 0f;
    private float _elapsedCoyoteTime = 0f;

    // Gravity Fields
    public float JumpGravity;
    public float FastFallGravity;
    public float ConstantGravityLateralDistance;

    // Grapple Mask
    public LayerMask WhatIsGrappable;

    // Swing data
    public LayerMask WhatIsSwingable;

    public float2 Velocity;
    public bool IsJumpHeld { get; private set; }

    public float HorizontalInput;
    public GroundDetector GroundDetector;
    private CollideSlideCharacterCollisionResolver _collider;
    private PushOffOverhang _pushOff;
    private Rigidbody _rigidbody;

    private readonly List<IForceSource> activeForces = new ();

    private void HandleJumpRequested()
    {
        if (GroundDetector.IsGrounded || HasCoyoteBuffered)
        {
            RequestedJump = true;
        }
        else
        {
            HasJumpBuffered = true;
            _elapsedJumpBufferTime = 0f;
        }
    }

    private void HandleJumpReleased()
    {
        IsJumpHeld = false;
    }

    private void HandleJumpHeld()
    {
        IsJumpHeld = true;
    }

    public void UpdateBuffers()
    {
        if (HasJumpBuffered)
        {
            _elapsedJumpBufferTime += Time.fixedDeltaTime;
            if (_elapsedJumpBufferTime >= TimeSettings.JumpBufferTime)
            {
                HasJumpBuffered = false;
                _elapsedJumpBufferTime = 0f;
            }
        }

        if (HasCoyoteBuffered)
        {
            _elapsedCoyoteTime += Time.fixedDeltaTime;
            if (_elapsedCoyoteTime >= TimeSettings.CoyoteTime)
            {
                HasCoyoteBuffered = false;
                _elapsedCoyoteTime = 0f;
            }
        }
    }

    public void ActivateCoyoteTime()
    {
        if (GroundDetector.WasGrounded)
        {
            HasCoyoteBuffered = true;
            _elapsedCoyoteTime = 0f;
        }
    }

    public void ApplyVerticalMovement(float gravity)
    {
        Velocity.y += gravity * Time.fixedDeltaTime;
    }

    public void ApplyPrecisionMovement(MoveActionSO move)
    {
        float targetSpeed = HorizontalInput * move.MaxHorizontalSpeed;
        Velocity.x = targetSpeed;
    }

    public void ApplyMomentumMovement(MoveActionSO move)
    {
        float horizontalTargetSpeed = HorizontalInput * move.MaxHorizontalSpeed;
        float currentSpeed = Velocity.x;

        // Determine if player is counter-strafing
        bool isCounterStrafing = (HorizontalInput > Settings.FloatPrecisionThreshold && currentSpeed < 0)
                                 || (HorizontalInput < -Settings.FloatPrecisionThreshold && currentSpeed > 0);

        float acceleration = isCounterStrafing ? move.Deceleration : move.Acceleration;

        // Calculate intended speed change this frame
        float speedDifference = horizontalTargetSpeed - currentSpeed;
        float accelerationChange = acceleration * Time.fixedDeltaTime;

        if (Mathf.Abs(HorizontalInput) > Settings.FloatPrecisionThreshold)
        {
            if (Mathf.Sign(currentSpeed) == Mathf.Sign(HorizontalInput))
            {
                // Moving in same direction as input
                if (Mathf.Abs(currentSpeed) < Mathf.Abs(horizontalTargetSpeed))
                {
                    // Accelerate up to target speed
                    if (Mathf.Abs(speedDifference) <= accelerationChange)
                    {
                        Velocity.x = horizontalTargetSpeed;
                    }
                    else
                    {
                        Velocity.x += Mathf.Sign(speedDifference) * accelerationChange;
                    }
                }
                // Already at or above max input speed: do not add further input-based speed
            }
            else
            {
                // Counter-strafe to reverse direction
                if (Mathf.Abs(speedDifference) <= accelerationChange)
                {
                    Velocity.x = horizontalTargetSpeed;
                }
                else
                {
                    Velocity.x += Mathf.Sign(speedDifference) * accelerationChange;
                }
            }
        }
    }

    public void ApplySpeedDecay(MoveActionSO move)
    {
        float currentSpeed = Mathf.Abs(Velocity.x);

        if (currentSpeed > move.MaxHorizontalSpeed)
        {
            float excessSpeed = currentSpeed - move.MaxHorizontalSpeed;
            float decayAmt = excessSpeed * move.SpeedDecayRate * Time.fixedDeltaTime;

            if (Velocity.x > 0f)
            {
                Velocity.x = Mathf.Max(Velocity.x - decayAmt, move.MaxHorizontalSpeed);
            }
            else if (Velocity.x < 0f)
            {
                Velocity.x = Mathf.Min(Velocity.x + decayAmt, -move.MaxHorizontalSpeed);
            }
        }
    }
    public void ApplyFriction(MoveActionSO move)
    {
        if (Mathf.Abs(HorizontalInput) < Settings.FloatPrecisionThreshold)
        {
            if (Mathf.Abs(Velocity.x) < Settings.FloatPrecisionThreshold)
            {
                Velocity.x = 0f;
            }
            else
            {
                float delta = move.GroundFrictionForce * Time.fixedDeltaTime;

                if (Velocity.x > 0f)
                {
                    Velocity.x = Mathf.Max(Velocity.x - delta, 0f);
                }
                else if (Velocity.x < 0f)
                {
                    Velocity.x = Mathf.Min(Velocity.x + delta, 0f);
                }
            }
        }
    }

    public void ClampVerticalVelocity(MoveActionSO move)
    {
        Velocity.y = Mathf.Clamp(Velocity.y, -move.MaxVerticalSpeed, move.MaxVerticalSpeed);
    }

    public void HandleOverhangPushOff()
    {
        if (_pushOff != null)
        {
            _pushOff.TryPushOffLedge(Velocity.y, Time.fixedDeltaTime);
        }
        else
        {
            Debug.LogWarning("PushOffOverhang component is missing on PlayerMovement object. Please add it to enable overhang push-off functionality.");
        }
    }

    public void SimulateStep()
    {
        Vector3 horizontalDisplacement = new Vector3(Velocity.x, 0f, 0f) * Time.fixedDeltaTime;
        Vector3 verticalDisplacement = new Vector3(0f, Velocity.y, 0f) * Time.fixedDeltaTime;

        horizontalDisplacement = _collider.ResolveCollideAndSlide(horizontalDisplacement, 0, false);
        Velocity.x = horizontalDisplacement.x / Time.fixedDeltaTime;
        this.transform.position += horizontalDisplacement;


        verticalDisplacement = _collider.ResolveCollideAndSlide(verticalDisplacement, 0, true);
        Velocity.y = verticalDisplacement.y / Time.fixedDeltaTime;
        this.transform.position += verticalDisplacement;
        ResetFlags();
    }

    public void ApplyPhysics()
    {
        GroundDetector.Refresh();
    }

    public void ResetFlags()
    {
        RequestedJump = false;
    }

    public void HandleGroundedMovementLogic(MoveActionSO move, float gravity)
    {
        ApplyVerticalMovement(gravity);

        if (_usePrecisionMovement)
            ApplyPrecisionMovement(move);
        else
        {
            ApplyMomentumMovement(move);
            ApplySpeedDecay(move);
            ApplyFriction(move);
        }

        ClampVerticalVelocity(move);
        HandleOverhangPushOff();
    }

    public void HandleAirMovementLogic(MoveActionSO move, float gravity)
    {
        ApplyVerticalMovement(gravity);

        if (_usePrecisionMovement)
            ApplyPrecisionMovement(move);
        else
        {
            ApplyMomentumMovement(move);
            ApplyFriction(move);
        }

        ClampVerticalVelocity(move);
        HandleOverhangPushOff();
    }

    public void HandleFrictionlessMovementLogic(MoveActionSO move, float gravity)
    {
        ApplyVerticalMovement(gravity);

        if (_usePrecisionMovement)
            ApplyPrecisionMovement(move);
        else
        {
            ApplyMomentumMovement(move);
        }

        ClampVerticalVelocity(move);
        HandleOverhangPushOff();
    }

    public float CalculateGravityLateralDistance(MoveActionSO move, JumpActionSO jump)
    {
        float constantGravityLateralDistance = (2 * jump.MaxJumpLateralDistance * Mathf.Sqrt(jump.FastFallMultiplier)) / (1 + Mathf.Sqrt(jump.FastFallMultiplier));
        return constantGravityLateralDistance;
    }

    public float CalculateJumpGravity(MoveActionSO move, JumpActionSO jump)
    {
        float constantGravityLateralDistance = CalculateGravityLateralDistance(move, jump);
        float jumpGravity = (-2f * jump.MaxJumpHeight * Mathf.Pow(move.MaxHorizontalSpeed, 2.0f)) / (Mathf.Pow((constantGravityLateralDistance / 2.0f), 2.0f));
        return jumpGravity;
    }

    public float CalculateFastFallGravity(MoveActionSO move, JumpActionSO jump)
    {
        float jumpGravity = CalculateJumpGravity(move, jump);
        float fastFallGravity = jumpGravity * jump.FastFallMultiplier;
        return fastFallGravity;
    }

    public float CalculateInitialJumpSpeed(MoveActionSO move, JumpActionSO jump)
    {
        float constantGravityLateralDistance = CalculateGravityLateralDistance(move, jump);
        float initialJumpSpeed = (2.0f * jump.MaxJumpHeight * move.MaxHorizontalSpeed) / (constantGravityLateralDistance / 2.0f);
        return initialJumpSpeed;
    }

    public void RegisterForceSource(IForceSource source)
    {
        if (!activeForces.Contains(source))
        {
            activeForces.Add(source);
        }
    }

    public void UnregisterForceSource(IForceSource source)
    {
        if (activeForces.Contains(source))
        {
            activeForces.Remove(source);
        }
    }

    private Vector3 GetTotalExternalForce()
    {
        Vector3 totalForce = Vector3.zero;
        foreach (var source in activeForces)
        {
            totalForce += source.GetForce();
        }
        return totalForce;
    }

    public void ApplyExternalForces()
    {
        Vector3 totalForce = GetTotalExternalForce();
        Velocity += (float2) new Vector2(totalForce.x, totalForce.y) * Time.fixedDeltaTime;
    }

    private void Awake()
    {
        JumpGravity = CalculateJumpGravity(MoveAction, JumpAction);
        FastFallGravity = CalculateFastFallGravity(MoveAction, JumpAction);
        Vector3 gravityVector = new Vector3(0f, FastFallGravity, 0f);
        PhysicsManager.Instance.SetGravity(gravityVector);

        // Initialize starting state
        CurrentState = new GroundedState(this);
        TransitionToState(CurrentState);
    }

    void Start()
    {
        GroundDetector = GetComponent<GroundDetector>();
        _rigidbody = GetComponent<Rigidbody>();
        _pushOff = GetComponent<PushOffOverhang>();
        _collider = GetComponent<CollideSlideCharacterCollisionResolver>();
        _inputHandler = GetComponent<PlayerInputHandler>();
        WhatIsGrappable = GetComponent<GrappleHandler>().WhatIsGrappable;

        _rigidbody.isKinematic = true;
        _rigidbody.freezeRotation = true;
        RequestedGrapple = false;


        HorizontalInput = _inputHandler.MoveInput.x;

        // Subscribe to input events ONCE
        _inputHandler.OnJumpRequested += HandleJumpRequested;
        _inputHandler.OnJumpHeld += HandleJumpHeld;
        _inputHandler.OnJumpReleased += HandleJumpReleased;
    }

    void OnDestroy()
    {
        // Unsubscribe from input events
        if (_inputHandler != null)
        {
            _inputHandler.OnJumpRequested -= HandleJumpRequested;
            _inputHandler.OnJumpHeld -= HandleJumpHeld;
            _inputHandler.OnJumpReleased -= HandleJumpReleased;
        }
    }

    private void Update()
    {
        HorizontalInput = _inputHandler.MoveInput.x;
        IsJumpHeld = _inputHandler.IsJumpHeld;
    }
}