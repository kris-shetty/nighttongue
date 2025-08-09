using UnityEngine;

public class JumpingState : PlayerState
{
    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;
    private TongueTransformHandler _tongueTransformHandler;

    private MoveActionSO dynamicMoveAction;
    protected override MoveActionSO MoveActionOverride => dynamicMoveAction;

    private JumpActionSO dynamicJumpAction;
    protected override JumpActionSO JumpActionOverride => dynamicJumpAction;

    private float _jumpGravity;
    private float _fastFallGravity;
    private float _initialJumpSpeed;

    public JumpingState(PlayerController controller, MoveActionSO move = null, JumpActionSO jump = null)
    {
        Context = controller;
        dynamicMoveAction = move;
        dynamicJumpAction = jump;
    }

    protected override void InitializeGravity()
    {
        _jumpGravity = Context.CalculateJumpGravity(ActiveMoveAction, ActiveJumpAction);
        _fastFallGravity = Context.CalculateFastFallGravity(ActiveMoveAction, ActiveJumpAction);
        Gravity = _jumpGravity; 
    }

    private void ApplyJump()
    {
        Context.Velocity.y = _initialJumpSpeed;
    }

    private void HandleJumpLogic()
    {
        ApplyJump();
        Context.HasJumpBuffered = false;
        Context.HasCoyoteBuffered = false;
    }

    public override BaseState GetNextState()
    {
        if (Context.Velocity.y <= 0f)
        {
            return new FallingState(Context, ActiveMoveAction, ActiveJumpAction);
        }

        return null;
    }

    public override void EnterState()
    {
        _initialJumpSpeed = Context.CalculateInitialJumpSpeed(ActiveMoveAction, ActiveJumpAction);
        InitializeGravity();

        _grappleHandler = Context.GetComponent<GrappleHandler>();
        if (_grappleHandler == null)
        {
            Debug.LogError("JumpingState :: GrappleHandler component not found on PlayerController.");
            return;
        }
        _grappleHandler.OnGrappleRequested += HandleGrappleRequest;

        _swingHandler = Context.GetComponent<SwingHandler>();
        if(_swingHandler == null)
        {
            Debug.LogError("JumpingState :: SwingHandler component not found on PlayerController.");
            return;
        }
        _swingHandler.OnSwingRequested += HandleSwingRequest;

        _tongueTransformHandler = Context.GetComponent<TongueTransformHandler>();
        if (_tongueTransformHandler == null)
        {
            Debug.LogError("JumpingState :: TongueTransformHandler component not found on PlayerController.");
            return;
        }
        _tongueTransformHandler.OnTransformStateChanged += HandleTransformStateChange;

        HandleJumpLogic();
    }

    public override void FixedUpdateState()
    {
        Context.ApplyPhysics();
        Context.HandleAirMovementLogic(ActiveMoveAction, Gravity);
        Context.UpdateBuffers();
        // Apply fast fall gravity if jump is not held
        if (!Context.IsJumpHeld)
        {
            Gravity = _fastFallGravity;
        }
        
        BaseState state = GetNextState();

        if (state != null)
        {
            Context.TransitionToState(state);
        }
        Context.SimulateStep();
    }

    public override void ExitState() 
    {
        if (_grappleHandler != null)
        {
            _grappleHandler.OnGrappleRequested -= HandleGrappleRequest;
        }

        if(_swingHandler != null)
        {
            _swingHandler.OnSwingRequested -= HandleSwingRequest;
        }
    }

    private void HandleGrappleRequest(GrappleAbilitySO grappleAbility, Vector3 grapplePoint)
    {
        Context.TransitionToState(new GrapplingState(Context, grappleAbility, grapplePoint));
    }

    private void HandleSwingRequest(SwingAbilitySO swingAbility, Vector3 swingPoint)
    {
        Context.TransitionToState(new SwingingState(Context, swingAbility, swingPoint));
    }

    private void HandleTransformStateChange(TongueTransformEventArgs args)
    {
        dynamicMoveAction = args.MoveAction;
        dynamicJumpAction = args.JumpAction;
        InitializeGravity();
    }

    public override void UpdateState() { }
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}