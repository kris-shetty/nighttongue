using UnityEngine;
using Unity.Mathematics;

public class GroundedState : PlayerState
{
    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;
    private TongueTransformHandler _tongueTransformHandler;

    private MoveActionSO dynamicMoveAction;
    protected override MoveActionSO MoveActionOverride => dynamicMoveAction;

    private JumpActionSO dynamicJumpAction;
    protected override JumpActionSO JumpActionOverride => dynamicJumpAction;

    public GroundedState(PlayerController controller, MoveActionSO move = null, JumpActionSO jump = null)
    {
        Context = controller;
        dynamicMoveAction = move;
        dynamicJumpAction = jump;
    }

    protected override void OnEnter()
    {
        _grappleHandler = Context.GetComponent<GrappleHandler>();
        if (_grappleHandler != null)
        {
            _grappleHandler.OnGrappleRequested -= HandleGrappleRequest;
        }
        _grappleHandler.OnGrappleRequested += HandleGrappleRequest;

        _swingHandler = Context.GetComponent<SwingHandler>();
        if (_swingHandler == null)
        {
            Debug.LogError("GroundedState :: SwingHandler component not found on PlayerController.");
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
    }

    private void HandleGrappleRequest(GrappleAbilitySO grappleAbility, Vector3 grapplePoint)
    {
        Context.TransitionToState(new GrapplingState(Context, grappleAbility, grapplePoint));
    }

    private void HandleSwingRequest(SwingAbilitySO swingAbility, Vector3 swingPoint)
    {
        Context.TransitionToState(new SwingingState(Context, swingAbility, swingPoint));
    }

    public override BaseState GetNextState()
    {
        // Check for jump request or buffered jump
        if (Context.RequestedJump || Context.HasJumpBuffered)
        {
            return new JumpingState(Context, ActiveMoveAction, ActiveJumpAction);
        }
        if (!Context.GroundDetector.IsGrounded)
        {
            return new FallingState(Context, ActiveMoveAction, ActiveJumpAction);
        }
        return null;
    }

    public override void FixedUpdateState()
    {
        Context.ApplyPhysics();
        Context.HandleGroundedMovementLogic(ActiveMoveAction, Gravity);
        ApplyExternalForces();
        Context.UpdateBuffers(); 
        BaseState nextState = GetNextState();
        if (nextState != null)
        {
            Context.TransitionToState(nextState);
        }
        Context.SimulateStep();
    }

    //2025.9.7ADD
    public void ApplyExternalForces()
    {
        Vector3 windforce = Context.GetTotalExternalForce();
        float weight = Context.StateMultiplier.Grounded;
        Vector3 finalForce = windforce * weight;
        float2 windForce2D = new float2(finalForce.x, finalForce.z);
        Context.Velocity += windForce2D * Time.fixedDeltaTime;
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

        if(_tongueTransformHandler != null)
        {
            _tongueTransformHandler.OnTransformStateChanged -= HandleTransformRequest;
        }
        // Clear jump flags when leaving ground
        Context.RequestedJump = false;
        Context.HasCoyoteBuffered = false;
        Context.HasJumpBuffered = false;
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