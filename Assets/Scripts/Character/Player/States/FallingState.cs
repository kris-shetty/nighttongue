using UnityEngine;

public class FallingState : BaseState
{
    private PlayerController _controller;
    
    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;

    public FallingState(PlayerController controller)
    {
        _controller = controller;
    }

    public override void EnterState()
    { 
        _grappleHandler = _controller.GetComponent<GrappleHandler>();
        if(_grappleHandler == null )
        {
            Debug.Log("FallingState :: GrappleHandler component not found on PlayerController.");
            return;
        }
        _grappleHandler.OnGrappleRequested += HandleGrappleRequest;

        _swingHandler = _controller.GetComponent<SwingHandler>();
        if(_swingHandler == null)
        {
            Debug.Log("FallingState :: SwingHandler component not found on PlayerController.");
            return;
        }
        _swingHandler.OnSwingRequested += HandleSwingRequest;

        _controller.Gravity = _controller.FastFallGravity;
        _controller.ActivateCoyoteTime();
    }

    public override void FixedUpdateState()
    {
        _controller.ApplyPhysics();
        _controller.HandleGroundedMovementLogic();
        _controller.UpdateBuffers();
        BaseState nextState = GetNextState();
        if (nextState != null)
        {
            _controller.TransitionToState(nextState);
        }
        _controller.SimulateStep();
    }

    public override BaseState GetNextState()
    {
        if (_controller.GroundDetector.IsGrounded)
        {
            return new GroundedState(_controller);
        }

        // Check for coyote jump
        if (_controller.HasCoyoteBuffered && _controller.RequestedJump)
        {
            return new JumpingState(_controller);
        }

        return null; // Stay in FallingState
    }

    public override void ExitState() 
    {
        if (_grappleHandler != null)
        {
            _grappleHandler.OnGrappleRequested -= HandleGrappleRequest;
        }
        if (_swingHandler != null)
        {
            _swingHandler.OnSwingRequested -= HandleSwingRequest;
        }
    }

    private void HandleGrappleRequest(GrappleAbilitySO grappleAbility, Vector3 grapplePoint)
    {
        _controller.TransitionToState(new GrapplingState(_controller, grappleAbility, grapplePoint));
    }

    private void HandleSwingRequest(SwingAbilitySO swingAbility, Vector3 swingPoint)
    {
        _controller.TransitionToState(new SwingingState(_controller, swingAbility, swingPoint));
    }

    public override void UpdateState() { }
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}