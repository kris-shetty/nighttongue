using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerController : StateMachine
{
    [Header("State Machine Context")]
    public JumpActionSO JumpAction;
    public MoveActionSO MoveAction;
    public GrappleAbilitySO GrappleAbility;
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
    public float Gravity;
    public float JumpGravity;
    public float FastFallGravity;
    public float ConstantGravityLateralDistance;

    // Grapple data
    public Vector3 GrappleTargetPoint { get; private set; }
    public LayerMask WhatIsGrappable;
    public void SetGrappleTarget(Vector3 grappleTarget) { GrappleTargetPoint = grappleTarget; }

    public float2 Velocity;
    public bool IsJumpHeld { get; private set; }

    private float _horizontalInput;
    public GroundDetector GroundDetector;
    private CollideSlideCharacterCollisionResolver _collider;
    private PushOffOverhang _pushOff;
    private Rigidbody _rigidbody;

    private void HandleJumpRequested()
    {
        Debug.Log($"HandleJumpRequested :: IsGrounded={GroundDetector.IsGrounded}, HasCoyoteBuffered={HasCoyoteBuffered}");

        if (GroundDetector.IsGrounded || HasCoyoteBuffered)
        {
            Debug.Log("PlayerController :: Jump requested. Transitioning to JumpingState.");
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
            Debug.Log($"PlayerController :: Coyote Time elapsed: {_elapsedCoyoteTime}");
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
            Debug.Log("PlayerController :: GroundDetector detected a loss of ground. Coyote Buffered");
            HasCoyoteBuffered = true;
            _elapsedCoyoteTime = 0f;
        }
    }

    public void ApplyVerticalMovement()
    {
        Velocity.y += Gravity * Time.fixedDeltaTime;
    }

    public void ApplyPrecisionMovement()
    {
        float targetSpeed = _horizontalInput * MoveAction.MaxHorizontalSpeed;
        Velocity.x = targetSpeed;
    }

    public void ApplyMomentumMovement()
    {
        float horizontalTargetSpeed = _horizontalInput * MoveAction.MaxHorizontalSpeed;
        float currentSpeed = Velocity.x;

        // Determine if player is counter-strafing
        bool isCounterStrafing = (_horizontalInput > Settings.FloatPrecisionThreshold && currentSpeed < 0)
                                 || (_horizontalInput < -Settings.FloatPrecisionThreshold && currentSpeed > 0);

        float acceleration = isCounterStrafing ? MoveAction.Deceleration : MoveAction.Acceleration;

        // Calculate intended speed change this frame
        float speedDifference = horizontalTargetSpeed - currentSpeed;
        float accelerationChange = acceleration * Time.fixedDeltaTime;

        if (Mathf.Abs(_horizontalInput) > Settings.FloatPrecisionThreshold)
        {
            if (Mathf.Sign(currentSpeed) == Mathf.Sign(_horizontalInput))
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

    public void ApplySpeedDecay()
    {
        float currentSpeed = Mathf.Abs(Velocity.x);

        if (currentSpeed > MoveAction.MaxHorizontalSpeed)
        {
            float excessSpeed = currentSpeed - MoveAction.MaxHorizontalSpeed;
            float decayAmt = excessSpeed * MoveAction.SpeedDecayRate * Time.fixedDeltaTime;

            if (Velocity.x > 0f)
            {
                Velocity.x = Mathf.Max(Velocity.x - decayAmt, MoveAction.MaxHorizontalSpeed);
            }
            else if (Velocity.x < 0f)
            {
                Velocity.x = Mathf.Min(Velocity.x + decayAmt, -MoveAction.MaxHorizontalSpeed);
            }
        }
    }
    public void ApplyFriction()
    {
        if (Mathf.Abs(_horizontalInput) < Settings.FloatPrecisionThreshold)
        {
            if (Mathf.Abs(Velocity.x) < Settings.FloatPrecisionThreshold)
            {
                Velocity.x = 0f;
            }
            else
            {
                float delta = MoveAction.GroundFrictionForce * Time.fixedDeltaTime;

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

    public void ClampVerticalVelocity()
    {
        Velocity.y = Mathf.Clamp(Velocity.y, -MoveAction.MaxVerticalSpeed, MoveAction.MaxVerticalSpeed);
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

    public void HandleGroundedMovementLogic()
    {
        ApplyVerticalMovement();

        if (_usePrecisionMovement)
            ApplyPrecisionMovement();
        else
        {
            ApplyMomentumMovement();
            ApplySpeedDecay();
            ApplyFriction();
        }

        ClampVerticalVelocity();
        HandleOverhangPushOff();
    }

    public void HandleAirMovementLogic()
    {
        ApplyVerticalMovement();

        if (_usePrecisionMovement)
            ApplyPrecisionMovement();
        else
        {
            ApplyMomentumMovement();
            ApplyFriction();
        }

        ClampVerticalVelocity();
        HandleOverhangPushOff();
    }

    public void HandleFrictionlessMovementLogic()
    {
        ApplyVerticalMovement();

        if (_usePrecisionMovement)
            ApplyPrecisionMovement();
        else
        {
            ApplyMomentumMovement();
        }

        ClampVerticalVelocity();
        HandleOverhangPushOff();
    }

    private void Awake()
    {
        ConstantGravityLateralDistance = (2 * JumpAction.MaxJumpLateralDistance * Mathf.Sqrt(JumpAction.FastFallMultiplier)) / (1 + Mathf.Sqrt(JumpAction.FastFallMultiplier));
        JumpGravity = (-2f * JumpAction.MaxJumpHeight * Mathf.Pow(MoveAction.MaxHorizontalSpeed, 2.0f)) / (Mathf.Pow((ConstantGravityLateralDistance / 2.0f), 2.0f));
        FastFallGravity = JumpGravity * JumpAction.FastFallMultiplier;
        Vector3 gravityVector = new Vector3(0f, FastFallGravity, 0f);
        PhysicsManager.Instance.SetGravity(gravityVector);

        // Initialize starting state
        CurrentState = new GroundedState(this);
    }

    void Start()
    {
        GroundDetector = GetComponent<GroundDetector>();
        _rigidbody = GetComponent<Rigidbody>();
        _pushOff = GetComponent<PushOffOverhang>();
        _collider = GetComponent<CollideSlideCharacterCollisionResolver>();
        _inputHandler = GetComponent<PlayerInputHandler>();
        GrappleAbility = GetComponent<GrappleHandler>().ActiveAbility;
        WhatIsGrappable = GetComponent<GrappleHandler>().WhatIsGrappable;

        _rigidbody.isKinematic = true;
        _rigidbody.freezeRotation = true;
        RequestedGrapple = false;


        _horizontalInput = _inputHandler.MoveInput.x;

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
        _horizontalInput = _inputHandler.MoveInput.x;
        IsJumpHeld = _inputHandler.IsJumpHeld;
    }
}