using UnityEngine;

public class GroundedState : BaseState
{
    private PlayerController _controller;
    
    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;

    public GroundedState(PlayerController controller)
    {
        _controller = controller;
    }

    public override void EnterState()
    {
        _grappleHandler = _controller.GetComponent<GrappleHandler>();
        if (_grappleHandler != null)
        {
            _grappleHandler.OnGrappleRequested -= HandleGrappleRequest;
        }
        _grappleHandler.OnGrappleRequested += HandleGrappleRequest;

        _swingHandler = _controller.GetComponent<SwingHandler>();
        if (_swingHandler == null)
        {
            Debug.LogError("GroundedState :: SwingHandler component not found on PlayerController.");
            return;
        }
        _swingHandler.OnSwingRequested += HandleSwingRequest;

        _controller.Gravity = _controller.FastFallGravity;
    }

    private void HandleGrappleRequest(GrappleAbilitySO grappleAbility, Vector3 grapplePoint)
    {
        _controller.TransitionToState(new GrapplingState(_controller, grappleAbility, grapplePoint));
    }

    private void HandleSwingRequest(SwingAbilitySO swingAbility, Vector3 swingPoint)
    {
        _controller.TransitionToState(new SwingingState(_controller, swingAbility, swingPoint));
    }

    public override BaseState GetNextState()
    {
        // Check for jump request or buffered jump
        if (_controller.RequestedJump || _controller.HasJumpBuffered)
        {
            return new JumpingState(_controller);
        }
        if (!_controller.GroundDetector.IsGrounded)
        {
            return new FallingState(_controller);
        }
        return null;
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
        // Clear jump flags when leaving ground
        _controller.RequestedJump = false;
        _controller.HasCoyoteBuffered = false;
        _controller.HasJumpBuffered = false;
    }

    public override void UpdateState() { }
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}