using UnityEngine;

public class GroundedState : BaseState
{
    private PlayerController _controller;

    public GroundedState(PlayerController controller)
    {
        _controller = controller;
    }

    public override void EnterState()
    {
        _controller.Gravity = _controller.FastFallGravity;
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
        if (_controller.RequestedGrapple)
        {
            return new GrapplingState(_controller);
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