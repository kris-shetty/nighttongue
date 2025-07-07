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
        return null;
    }

    public override void FixedUpdateState()
    {
        BaseState nextState = GetNextState();
        if (nextState != null)
        {
            _controller.TransitionToState(nextState);
        }
    }

    public override void ExitState()
    {
        // Clear jump flags when leaving ground
        _controller.RequestedJump = false;
        _controller.HasCoyotedBuffered = false;
        _controller.HasJumpBuffered = false;
    }

    public override void UpdateState() { }
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}