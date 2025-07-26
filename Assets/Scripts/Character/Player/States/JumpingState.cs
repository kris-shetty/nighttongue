using UnityEngine;

public class JumpingState : BaseState
{
    private PlayerController _controller;
    private JumpActionSO _jump;
    private MoveActionSO _move;
    private float _initialJumpSpeed;
    
    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;

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

        return null;
    }

    public override void EnterState()
    {
        _grappleHandler = _controller.GetComponent<GrappleHandler>();
        if (_grappleHandler == null)
        {
            Debug.LogError("JumpingState :: GrappleHandler component not found on PlayerController.");
            return;
        }
        _grappleHandler.OnGrappleRequested += HandleGrappleRequest;

        _swingHandler = _controller.GetComponent<SwingHandler>();
        if(_swingHandler == null)
        {
            Debug.LogError("JumpingState :: SwingHandler component not found on PlayerController.");
            return;
        }
        _swingHandler.OnSwingRequested += HandleSwingRequest;

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