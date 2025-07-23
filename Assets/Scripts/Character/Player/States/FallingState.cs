using UnityEngine;

public class FallingState : BaseState
{
    private PlayerController _controller;

    public FallingState(PlayerController controller)
    {
        _controller = controller;
    }

    public override void EnterState()
    { 
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

        if (_controller.RequestedGrapple)
        {
            return new GrapplingState(_controller);
        }

        if (_controller.RequestedSwing)
        {
            return new SwingingState(_controller);
        }

        return null; // Stay in FallingState
    }

    public override void ExitState() { }
    public override void UpdateState() { }
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}