using Unity.Mathematics;
using UnityEngine;

public class PlayerController : StateMachine
{
    [Header("State Machine Context")]
    public JumpActionSO JumpAction;
    public MoveActionSO MoveAction;
    public PlayerTimeSettingsSO TimeSettings;
    public GlobalPhysicsSettingsSO _settings;
    [SerializeField] private bool _usePrecisionMovement = false;

    // Input Handler
    private PlayerInputHandler _inputHandler;

    // Flags
    public bool RequestedJump = false;
    public bool HasCoyotedBuffered = false;
    public bool HasJumpBuffered = false;

    // Buffer Timers
    private float _elapsedJumpBufferTime = 0f;
    private float _elapsedCoyoteTime = 0f;

    // Gravity Fields
    public float Gravity;
    public float JumpGravity;
    public float FastFallGravity;
    public float ConstantGravityLateralDistance;

    public float2 Velocity;
    public bool IsJumpHeld { get; private set; }

    private float _horizontalInput;
    public GroundDetector GroundDetector;
    private CollideSlideCharacterCollisionResolver _collider;
    private PushOffOverhang _pushOff;
    private Rigidbody _rigidbody;

    private void HandleJumpRequested()
    {
        if (GroundDetector.IsGrounded)
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

    private void UpdateBuffers()
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

        if (HasCoyotedBuffered)
        {
            _elapsedCoyoteTime += Time.fixedDeltaTime;
            if (_elapsedCoyoteTime >= TimeSettings.CoyoteTime)
            {
                HasCoyotedBuffered = false;
                _elapsedCoyoteTime = 0f;
            }
        }

        if (GroundDetector.WasGrounded && !GroundDetector.IsGrounded && !HasCoyotedBuffered)
        {
            HasCoyotedBuffered = true;
            _elapsedCoyoteTime = 0f;
        }
    }

    private void ApplyVerticalMovement()
    {
        Velocity.y += Gravity * Time.fixedDeltaTime;
    }

    private void ApplyPrecisionMovement()
    {
        float targetSpeed = _horizontalInput * MoveAction.MaxHorizontalSpeed;
        Velocity.x = targetSpeed;
    }

    private void ApplyMomentumMovement()
    {
        if (Mathf.Abs(_horizontalInput) > _settings.FloatPrecisionThreshold)
        {
            float horizontalTargetSpeed = _horizontalInput * MoveAction.MaxHorizontalSpeed;
            float horizontalSpeedDifference = horizontalTargetSpeed - Velocity.x;
            float acceleration;

            bool isCounterStrafing = (_horizontalInput > _settings.FloatPrecisionThreshold && Velocity.x < 0)
                                        ||
                                     (_horizontalInput < -_settings.FloatPrecisionThreshold && Velocity.x > 0);

            acceleration = isCounterStrafing ? MoveAction.Deceleration : MoveAction.Acceleration;

            Velocity.x += horizontalSpeedDifference * acceleration * Time.fixedDeltaTime;
        }
    }

    private void ApplyFriction()
    {
        if (Mathf.Abs(_horizontalInput) < _settings.FloatPrecisionThreshold)
        {
            if (Mathf.Abs(Velocity.x) < _settings.FloatPrecisionThreshold)
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

    private void ClampVelocity()
    {
        Velocity.x = Mathf.Clamp(Velocity.x, -MoveAction.MaxHorizontalSpeed, MoveAction.MaxHorizontalSpeed);
        Velocity.y = Mathf.Clamp(Velocity.y, -MoveAction.MaxVerticalSpeed, MoveAction.MaxVerticalSpeed);
    }

    private void HandleOverhangPushOff()
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

    private void SimulateStep()
    {
        Vector3 horizontalDisplacement = new Vector3(Velocity.x, 0f, 0f) * Time.fixedDeltaTime;
        Vector3 verticalDisplacement = new Vector3(0f, Velocity.y, 0f) * Time.fixedDeltaTime;

        horizontalDisplacement = _collider.ResolveCollideAndSlide(horizontalDisplacement, 0, false);
        Velocity.x = horizontalDisplacement.x / Time.fixedDeltaTime;
        this.transform.position += horizontalDisplacement;


        verticalDisplacement = _collider.ResolveCollideAndSlide(verticalDisplacement, 0, true);
        Velocity.y = verticalDisplacement.y / Time.fixedDeltaTime;
        this.transform.position += verticalDisplacement;
    }

    private void ApplyPhysics()
    {
        GroundDetector.Refresh();
    }

    private void HandleMovementLogic()
    {
        ApplyVerticalMovement();

        if (_usePrecisionMovement)
            ApplyPrecisionMovement();
        else
        {
            ApplyMomentumMovement();
            ApplyFriction();
        }

        ClampVelocity();
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

        _rigidbody.isKinematic = true;
        _rigidbody.freezeRotation = true;
        
        _horizontalInput = InputManager.Instance.MoveInput.x;

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
        _horizontalInput = InputManager.Instance.MoveInput.x;
        IsJumpHeld = _inputHandler.IsJumpHeld;
    }

    protected override void PreFixedStateUpdate()
    {
        ApplyPhysics();
        HandleMovementLogic();
        UpdateBuffers(); // Handle all buffer logic centrally
    }

    protected override void PostFixedStateUpdate()
    {
        SimulateStep();
        RequestedJump = false;
    }
}