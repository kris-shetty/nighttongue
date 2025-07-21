using UnityEngine;

public class JumpingState : BaseState
{
    private PlayerController _controller;
    private JumpActionSO _jump;
    private MoveActionSO _move;
    private float _initialJumpSpeed;

    public JumpingState(PlayerController controller)
    {
        _controller = controller;
        _jump = _controller.JumpAction;
        _move = _controller.MoveAction;
        _initialJumpSpeed = (2.0f * _jump.MaxJumpHeight * _move.MaxHorizontalSpeed) / (_controller.ConstantGravityLateralDistance / 2.0f);
    }

    private void ApplyJump()
    {
        _controller.Gravity = _controller.JumpGravity;
        _controller.Velocity.y = _initialJumpSpeed;
    }

    private void HandleJumpLogic()
    {
        ApplyJump();
        _controller.HasJumpBuffered = false;
        _controller.HasCoyoteBuffered = false;
    }

    public override BaseState GetNextState()
    {
        if (_controller.Velocity.y <= 0f)
        {
            return new FallingState(_controller);
        }

        if(_controller.RequestedGrapple)
        {
            return new GrapplingState(_controller);
        }
        return null;
    }

    public override void EnterState()
    {
        HandleJumpLogic();
    }

    public override void FixedUpdateState()
    {
        _controller.ApplyPhysics();
        _controller.HandleAirMovementLogic();
        _controller.UpdateBuffers();
        // Apply fast fall gravity if jump is not held
        if (!_controller.IsJumpHeld)
        {
            _controller.Gravity = _controller.FastFallGravity;
        }
        
        BaseState state = GetNextState();

        if (state != null)
        {
            _controller.TransitionToState(state);
        }
        _controller.SimulateStep();
    }

    public override void ExitState() { }
    public override void UpdateState() { }
    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}