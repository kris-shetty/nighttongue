using UnityEngine;

public class GrapplingState : PlayerState
{
    private float _initialVerticalSpeed;
    private float _initialHorizontalSpeed;
    private float _jumpGravity;
    private float _fastFallGravity;
    private Vector3 _grapplePoint;
    private Vector3 _initialPlayerPos;
    private GrappleAbilitySO _activeAbility;
    private CollideSlideCharacterCollisionResolver _collider;
    private GrappleHandler _grappleHandler;
    private float _grappleEntryTimer = 0f;
    private float _grappleIgnoreDuration = 0.05f;

    private SwingHandler _swingHandler;
    private TongueController _tongueController;

    public GrapplingState(PlayerController controller, GrappleAbilitySO grappleAbility, Vector3 grapplePoint)
    {
        Context = controller;      
        _grapplePoint = grapplePoint;
        _initialPlayerPos = Context.transform.position;
        _grappleIgnoreDuration = Context.Settings.GrappleIgnoreDuration;   
        _activeAbility = grappleAbility;
    }

    private float CalculateVerticalHeight()
    {
        float highestPoint = 0;
   
        float verticalDistance = _grapplePoint.y - _initialPlayerPos.y;
        verticalDistance += _activeAbility.OffsetHeight;
        if (verticalDistance < 0)
        {
            highestPoint = _activeAbility.OvershootHeight;
        }
        else
        {
            highestPoint = verticalDistance + _activeAbility.OvershootHeight;
        }
        
        return highestPoint;
    }

    private void CalculateJumpVelocity()
    {
        float highestPoint = CalculateVerticalHeight();
        float verticalDistance = _grapplePoint.y - _initialPlayerPos.y;
        float horizontalDistance = _grapplePoint.x - _initialPlayerPos.x;
        verticalDistance += _activeAbility.OffsetHeight;
        horizontalDistance += _activeAbility.OffsetDistance;
        // verticalDistance += _activeAbility.OffsetDistance;
        _initialVerticalSpeed = Mathf.Sqrt(-2 * _jumpGravity * highestPoint);
        float timeUp = Mathf.Sqrt((-2 * highestPoint)/_jumpGravity);
        float timeDown = Mathf.Sqrt((-2 * (highestPoint - verticalDistance) / _fastFallGravity));
        _initialHorizontalSpeed = horizontalDistance / (timeUp + timeDown);
    }

    private void ApplyExternalForces()
    {
        Vector3 windForceVec = Context.GetTotalExternalForce();
        float weight = Context.StateMultiplier.Grappling;
        windForceVec *= weight;
        //apply force 
        Context.Velocity.x += windForceVec[0] * Time.fixedDeltaTime;
        Context.Velocity.y += windForceVec[1] * Time.fixedDeltaTime;
    }

    protected override void InitializeGravity()
    {
        _jumpGravity = Context.CalculateJumpGravity(ActiveMoveAction, ActiveJumpAction);
        Gravity = _jumpGravity;
        _fastFallGravity = Gravity * ActiveJumpAction.FastFallMultiplier;
    }

    private void ApplyGrapple()
    {
        Gravity = Context.CalculateJumpGravity(ActiveMoveAction, ActiveJumpAction);
        CalculateJumpVelocity();
        Context.Velocity.y = _initialVerticalSpeed;
        Context.Velocity.x = _initialHorizontalSpeed;
    }

    private bool IsGrappleValid()
    {
        float distanceToTarget = Vector3.Distance(_initialPlayerPos, _grapplePoint);
        if (distanceToTarget > _activeAbility.MaxGrappleDistance)
        {
            Debug.LogWarning("GrapplingState :: Grapple target is out of range.");
            return false;
        }
        return Physics.Raycast(_initialPlayerPos, (_grapplePoint - _initialPlayerPos).normalized, _activeAbility.MaxGrappleDistance, Context.WhatIsGrappable);
    }

    protected override void OnEnter()
    {
        _collider = Context.GetComponent<CollideSlideCharacterCollisionResolver>();
        if (_collider == null)
        {
            Debug.LogError("GrapplingState :: CollideSlideCharacterCollisionResolver component not found on PlayerController.");
        }

        _grappleHandler = Context.GetComponent<GrappleHandler>();
        if (_grappleHandler == null)
        {
            Debug.LogError("GrapplingState :: GrappleHandler component not found on PlayerController.");
            return;
        }
        _grappleHandler.OnGrappleRequested += HandleGrappleRequest;

        GameObject tongue = Context.transform.Find("Tongue").gameObject;
        if (tongue == null)
        {
            Debug.LogError("GrappleHandler: Tongue GameObject not found. Please ensure it is attached as a child to this GameObject.");
        }
        else
        {
            _tongueController = tongue.GetComponent<TongueController>();
            if (_tongueController == null)
            {
                Debug.LogError("GrappleHandler :: TongueController component not found on the Tongue GameObject.");
            }
            _tongueController.AttachTongue(_grapplePoint);
        }

        _swingHandler = Context.GetComponent<SwingHandler>();
        if (_swingHandler == null)
        {
            Debug.LogError("GrapplingState :: SwingHandler component not found on PlayerController.");
        }
        _swingHandler.OnSwingRequested += HandleSwingRequest;

        Debug.Log("GrapplingState :: Is it in yet?");
        _collider.OnCollisionDetected += OnPlayerCollision;
        if (IsGrappleValid())
        {
            ApplyGrapple();
            ApplyExternalForces();
            Context.RequestedGrapple = false;
        }
        else
        {
            BaseState nextState = GetNextState();
            if (nextState != null)
            {
                Context.TransitionToState(nextState);
            }
            else
            {
                Debug.LogError("GrapplingState :: No valid next state found after grapple validation.");
            }
        }
    }

    private void OnPlayerCollision()
    {
        if(_grappleEntryTimer < _grappleIgnoreDuration)
        {
            return;
        }
        
        BaseState nextState = GetNextState();
        if (nextState != null)
        {
            Context.TransitionToState(nextState);
        }
        else
        {
            Debug.LogError("GrapplingState :: No valid next state found after grapple validation.");
        }
    }

    private void HandleGrappleRequest(GrappleAbilitySO ability, Vector3 grapplePoint)
    {
        BaseState nextState = new GrapplingState(Context, ability, grapplePoint);
        Context.TransitionToState(nextState);
    }

    private void HandleSwingRequest(SwingAbilitySO ability, Vector3 swingPoint)
    {
        BaseState nextState = new SwingingState(Context, ability, swingPoint);
        Context.TransitionToState(nextState);
    }

    protected override void OnExit()
    {
        if (Context != null)
        {
            _collider.OnCollisionDetected -= OnPlayerCollision;
        }
        
        if (_grappleHandler != null)
        {
            _grappleHandler.OnGrappleRequested -= HandleGrappleRequest;
        }

        if (_swingHandler != null)
        {
            _swingHandler.OnSwingRequested -= HandleSwingRequest;
        }

        if (_tongueController != null)
        {
            _tongueController.AimTongue();
        }
    }

    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }

    public override void FixedUpdateState()
    {
        _grappleEntryTimer += Time.fixedDeltaTime;

        ApplyExternalForces();

        Context.ApplyPhysics();
        
        if (Context.Velocity.y <= 0f)
        {
            Gravity = Context.FastFallGravity;
        }
        BaseState nextState = GetNextState();
        if (nextState is GrapplingState)
        {
            Context.TransitionToState(nextState);
        }
        Context.ApplyVerticalMovement(Gravity);
        Context.SimulateStep();
    }

    public override BaseState GetNextState()
    {
        if(Context.RequestedJump || Context.HasJumpBuffered)
        {
            return new JumpingState(Context);
        }

        if (Context.GroundDetector.IsGrounded)
        {
            return new GroundedState(Context);
        }
        else
        {
            return new FallingState(Context);
        }   
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
