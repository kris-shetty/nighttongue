using Unity.Mathematics;
using UnityEngine;

public class FallingState : PlayerState
{
    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;
    private TongueTransformHandler _tongueTransformHandler;

    private MoveActionSO dynamicMoveAction;
    protected override MoveActionSO MoveActionOverride => dynamicMoveAction;

    private JumpActionSO dynamicJumpAction;
    protected override JumpActionSO JumpActionOverride => dynamicJumpAction;

    public FallingState(PlayerController controller, MoveActionSO move = null, JumpActionSO jump = null)
    {
        Context = controller;
        dynamicMoveAction = move;
        dynamicJumpAction = jump;
    }

    private void ApplyExternalForces()
    {
        Vector3 force = Context.GetTotalExternalForce();
        Context.Velocity += (float2)new Vector2(force.x, force.y) * Time.fixedDeltaTime;
    }

    protected override void OnEnter()
    { 
        _grappleHandler = Context.GetComponent<GrappleHandler>();
        if(_grappleHandler == null )
        {
            Debug.Log("FallingState :: GrappleHandler component not found on PlayerController.");
            return;
        }
        _grappleHandler.OnGrappleRequested += HandleGrappleRequest;

        _swingHandler = Context.GetComponent<SwingHandler>();
        if(_swingHandler == null)
        {
            Debug.Log("FallingState :: SwingHandler component not found on PlayerController.");
            return;
        }
        _swingHandler.OnSwingRequested += HandleSwingRequest;

        _tongueTransformHandler = Context.GetComponent<TongueTransformHandler>();
        if (_tongueTransformHandler == null)
        {
            Debug.LogError("GroundedState :: TongueTransformHandler component not found on PlayerController.");
            return;
        }
        _tongueTransformHandler.OnTransformStateChanged += HandleTransformRequest;

        Context.ActivateCoyoteTime();
    }

    public override void FixedUpdateState()
    {
        Context.ApplyPhysics();
        ApplyExternalForces();
        Context.HandleGroundedMovementLogic(ActiveMoveAction, Gravity);
        Context.UpdateBuffers();
        BaseState nextState = GetNextState();
        if (nextState != null)
        {
            Context.TransitionToState(nextState);
        }
        Context.SimulateStep();
    }

    public override BaseState GetNextState()
    {
        if (Context.GroundDetector.IsGrounded)
        {
            return new GroundedState(Context);
        }

        // Check for coyote jump
        if (Context.HasCoyoteBuffered && Context.RequestedJump)
        {
            return new JumpingState(Context);
        }

        return null; // Stay in FallingState
    }

    protected override void OnExit() 
    {
        if (_grappleHandler != null)
        {
            _grappleHandler.OnGrappleRequested -= HandleGrappleRequest;
        }
        if (_swingHandler != null)
        {
            _swingHandler.OnSwingRequested -= HandleSwingRequest;
        }

        if (_tongueTransformHandler != null)
        {
            _tongueTransformHandler.OnTransformStateChanged -= HandleTransformRequest;
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

    private void HandleTransformRequest(TongueTransformEventArgs args)
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