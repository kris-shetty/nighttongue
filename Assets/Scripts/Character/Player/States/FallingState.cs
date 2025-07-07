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
    }

    public override void FixedUpdateState()
    {
        BaseState nextState = GetNextState();
        if (nextState != null)
        {
            _controller.TransitionToState(nextState);
        }
    }

    public override BaseState GetNextState()
    {
        if (_controller.GroundDetector.IsGrounded)
        {
            return new GroundedState(_controller);
        }

        // Check for coyote jump
        if (_controller.HasCoyotedBuffered && _controller.RequestedJump)
        {
            return new JumpingState(_controller);
        }

        return null; // Stay in FallingState
    }

    public override void ExitState() { }
    public override void UpdateState() { }
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}