using Unity.Mathematics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _maxHorizontalSpeed = 5f;
    [SerializeField] private float _maxVerticalSpeed = 1000f;

    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _deceleration = 70f;
    [SerializeField] private float _groundFrictionForce = 30f;

    [SerializeField] private bool _usePrecisionMovement = false;

    [SerializeField] private float2 _velocity;
    [SerializeField] private float _horizontalInput;

    [Header("Jump Settings")]
    [SerializeField] private float _fastFallMultiplier = 2f;
    public static readonly float PeakVelocityThreshold = 0.5f;
    [SerializeField] private float _maxJumpHeight = 10f;
    [SerializeField] private float _maxJumpLateralDistance = 30f;
    [SerializeField] private float _constantGravityLateralDistance;
    [SerializeField] private bool _reachedJumpPeak = false;
    private float _gravity;
    private float _fastFallGravity;
    private float _jumpGravity;
    private float _initialJumpSpeed;
    private bool _requestedJump;
    private bool _heldJump;

    // Jump Buffering
    [SerializeField] private float _jumpBufferTime = 0.1f;
    private bool _hasJumpBuffered = false;
    private float _elapsedJumpBufferTime = 0f;

    // Coyote Time
    [SerializeField] private float _coyoteTime = 0.1f;
    private bool _hasCoyoteBuffered = false;
    private float _elapsedCoyoteTime = 0f;

    [Header("Global Settings")]
    public static readonly float FloatPrecisionThreshold = 0.01f;
    private GroundDetector _groundDetector;
    private NaiveCharacterCollisionResolver _terrainCollider;
    private CollideSlideCharacterCollisionResolver _collider;
    private PushOffOverhang _pushOff;
    private Rigidbody _rigidbody;
    public Vector3 velocity;

    private void ApplyJump()
    {
        _gravity = _jumpGravity;
        _velocity.y = _initialJumpSpeed;
    }

    private void UpdateJumpBuffer()
    {
        if (_hasJumpBuffered)
        {
            _elapsedJumpBufferTime += Time.fixedDeltaTime;
            if (_elapsedJumpBufferTime >= _jumpBufferTime)
            {
                _hasJumpBuffered = false;
                _elapsedJumpBufferTime = 0f;
            }
        }
    }

    private void UpdateCoyoteBuffer()
    {
        if (_hasCoyoteBuffered)
        {
            _elapsedCoyoteTime += Time.fixedDeltaTime;
            if (_elapsedCoyoteTime >= _coyoteTime)
            {
                _hasCoyoteBuffered = false;
                _elapsedCoyoteTime = 0f;
            }
        }
    }

    private void ApplyVerticalMovement()
    {
        _velocity.y += _gravity * Time.fixedDeltaTime;
    }

    private void ApplyPrecisionMovement()
    {
        float targetSpeed = _horizontalInput * _maxHorizontalSpeed;
        _velocity.x = targetSpeed;
    }

    private void ApplyHorizontalMovement()
    {
        if (Mathf.Abs(_horizontalInput) > FloatPrecisionThreshold)
        {
            float horizontalTargetSpeed = _horizontalInput * _maxHorizontalSpeed;
            float horizontalSpeedDifference = horizontalTargetSpeed - _velocity.x;
            float acceleration;

            bool isCounterStrafing = (_horizontalInput > FloatPrecisionThreshold && _velocity.x < 0)
                                        ||
                                     (_horizontalInput < -FloatPrecisionThreshold && _velocity.x > 0);

            acceleration = isCounterStrafing ? _deceleration : _acceleration;

            _velocity.x += horizontalSpeedDifference * acceleration * Time.fixedDeltaTime;
        }
    }

    private void ApplyFriction()
    {
        if (Mathf.Abs(_horizontalInput) < FloatPrecisionThreshold)
        {
            if (Mathf.Abs(_velocity.x) < FloatPrecisionThreshold)
            {
                _velocity.x = 0f;
            }
            else
            {
                float delta = _groundFrictionForce * Time.fixedDeltaTime;

                if (_velocity.x > 0f)
                {
                    _velocity.x = Mathf.Max(_velocity.x - delta, 0f);
                }
                else if (_velocity.x < 0f)
                {
                    _velocity.x = Mathf.Min(_velocity.x + delta, 0f);
                }
            }
        }
    }

    private void ClampVelocity()
    {
        _velocity.x = Mathf.Clamp(_velocity.x, -_maxHorizontalSpeed, _maxHorizontalSpeed);
        _velocity.y = Mathf.Clamp(_velocity.y, -_maxVerticalSpeed, _maxVerticalSpeed);
    }

    private void HandleOverhangPushOff()
    {
        if (_pushOff != null)
        {
            _pushOff.TryPushOffLedge(_velocity.y, Time.fixedDeltaTime);
        }
        else
        {
            Debug.LogWarning("PushOffOverhang component is missing on PlayerMovement object. Please add it to enable overhang push-off functionality.");
        }
    }

    //private void HandleNaiveCollisionResponse()
    //{
    //    velocity = new Vector3(_velocity.x, _velocity.y, 0f);
    //    Vector3 horizontalComponent = _terrainCollider.ResolveHorizontal(velocity, Time.fixedDeltaTime);
    //    Vector3 verticalComponent = _terrainCollider.ResolveVertical(velocity, Time.fixedDeltaTime);
    //    _velocity.x = horizontalComponent.x;
    //    _velocity.y = verticalComponent.y;
    //}

    private void SimulateStep()
    {
        //Vector3 currentVel = _rigidbody.linearVelocity;
        //Vector3 targetVel = new Vector3(_velocity.x, _velocity.y, 0f);
        //Vector3 velocityDiff = targetVel - currentVel;

        //_rigidbody.AddForce(velocityDiff, ForceMode.VelocityChange);
        Vector3 horizontalDisplacement = new Vector3(_velocity.x, 0f, 0f) * Time.fixedDeltaTime;
        Vector3 verticalDisplacement = new Vector3(0f, _velocity.y, 0f) * Time.fixedDeltaTime;
        
        horizontalDisplacement = _collider.ResolveCollideAndSlide(horizontalDisplacement, 0, false);
        _velocity.x = horizontalDisplacement.x / Time.fixedDeltaTime;
        this.transform.position += horizontalDisplacement;

        verticalDisplacement = _collider.ResolveCollideAndSlide(verticalDisplacement, 0, true);
        _velocity.y = verticalDisplacement.y / Time.fixedDeltaTime;
        this.transform.position += verticalDisplacement;
    }

    private void HandleJumpLogic()
    {
        bool canJump = _groundDetector.IsGrounded || _hasCoyoteBuffered;
        bool wantsToJump = _requestedJump || _hasJumpBuffered;

        if (canJump && wantsToJump)
        {
            ApplyJump();
            _hasJumpBuffered = false;
            _hasCoyoteBuffered = false;
            _reachedJumpPeak = false;
        }

        // Vertical motion
        if (_groundDetector.IsGrounded)
        {
            _reachedJumpPeak = false;
        }
        else
        {
            // Check if we've reached jump peak
            if (Mathf.Abs(_velocity.y) < PeakVelocityThreshold)
            {
                _reachedJumpPeak = true;
            }

            // Apply fast fall gravity
            if (!_heldJump || _reachedJumpPeak)
            {
                _gravity = _fastFallGravity;
            }

            // Buffer jump if requested while in air
            if (_requestedJump)
            {
                _hasJumpBuffered = true;
                _elapsedJumpBufferTime = 0f;
            }

            // Start coyote time if we just left the ground
            if (_groundDetector.WasGrounded && !_hasCoyoteBuffered)
            {
                _hasCoyoteBuffered = true;
                _elapsedCoyoteTime = 0f;
            }
        }

        // Update buffers
        UpdateJumpBuffer();
        UpdateCoyoteBuffer();
    }

    private void HandleMovementLogic()
    {
        ApplyVerticalMovement();

        if (_usePrecisionMovement)
            ApplyPrecisionMovement();
        else
        {
            ApplyHorizontalMovement();
            ApplyFriction();
        }

        ClampVelocity();
        HandleOverhangPushOff();
        //HandleNaiveCollisionResponse();
        SimulateStep();
    }

    private void ApplyPhysics()
    {
        _groundDetector.Refresh();
    }

    void OnDestroy()
    {
        InputManager.Instance.OnJumpStarted -= HandleJumpStarted;
        InputManager.Instance.OnJumpPerformed -= HandleJumpPerformed; 
        InputManager.Instance.OnJumpCanceled -= HandleJumpCanceled;
    }

    private void HandleJumpStarted()
    {
        _requestedJump = true;
        _heldJump = true;
    }

    private void HandleJumpPerformed()
    {
        _heldJump = true;
    }

    private void HandleJumpCanceled()
    {
        _heldJump = false;
    }

    private void Awake()
    {
        _constantGravityLateralDistance = (2 * _maxJumpLateralDistance * Mathf.Sqrt(_fastFallMultiplier)) / (1 + Mathf.Sqrt(_fastFallMultiplier));
        _jumpGravity = (-2f * _maxJumpHeight * Mathf.Pow(_maxHorizontalSpeed, 2.0f)) / (Mathf.Pow((_constantGravityLateralDistance / 2.0f), 2.0f));
        _fastFallGravity = _jumpGravity * _fastFallMultiplier;
        _gravity = _fastFallGravity;
        _initialJumpSpeed = (2.0f * _maxJumpHeight * _maxHorizontalSpeed) / (_constantGravityLateralDistance / 2.0f);
    }

    void Start()
    {
        _groundDetector = GetComponent<GroundDetector>();
        _rigidbody = GetComponent<Rigidbody>();
        _pushOff = GetComponent<PushOffOverhang>();
        _collider = GetComponent<CollideSlideCharacterCollisionResolver>();

        _rigidbody.isKinematic = true;
        _rigidbody.freezeRotation = true;

        InputManager.Instance.OnJumpStarted += HandleJumpStarted;
        InputManager.Instance.OnJumpPerformed += HandleJumpPerformed;
        InputManager.Instance.OnJumpCanceled += HandleJumpCanceled;
    }

    void Update()
    {
        _horizontalInput = InputManager.Instance.MoveInput.x;
    }

    void FixedUpdate()
    {
        ApplyPhysics();
        HandleJumpLogic();
        HandleMovementLogic();
        _requestedJump = false;
    }
}