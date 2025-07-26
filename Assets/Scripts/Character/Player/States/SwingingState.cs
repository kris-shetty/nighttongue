using UnityEngine;

public class SwingingState : BaseState
{
    private PlayerController _controller;
    private Vector3 _swingPoint;
    private SwingAbilitySO _activeAbility;
    private float _swingRestLength;

    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;

    public SwingingState(PlayerController controller, SwingAbilitySO swingAbility, Vector3 swingPoint)
    {
        _controller = controller;
        _activeAbility = swingAbility;
        _swingPoint = swingPoint;
    }

    public override void EnterState()
    {
        _grappleHandler = _controller.GetComponent<GrappleHandler>();
        if (_grappleHandler == null)
        {
            Debug.LogError("SwingingState :: GrappleHandler component not found on PlayerController.");
            return;
        }
        _grappleHandler.OnGrappleRequested += HandleGrappleRequest;

        Debug.Log("Entering SwingingState; Yippee!");
        _controller.Gravity = _controller.FastFallGravity;
        if (!IsSwingValid())
        {
            BaseState nextState = InvalidSwingTransition();
            if (nextState != null)
            {
                _controller.TransitionToState(nextState);
            }
            else
            {
                Debug.LogError("SwingingState :: No valid next state found after grapple validation. How the hell did you get here?");
            }
        }
        _swingRestLength = (_controller.transform.position - _swingPoint).magnitude;
        _controller.RequestedSwing = false;
        CalculateInitialTangentialVelocity();
    }
    private bool IsSwingValid()
    {
        Vector3 initialPlayerPos = _controller.transform.position;
        float distanceToTarget = Vector3.Distance(initialPlayerPos, _swingPoint);
        if (distanceToTarget > _activeAbility.MaxSwingDistance)
        {
            Debug.LogWarning("GrapplingState :: Grapple target is out of range.");
            return false;
        }
        return Physics.Raycast(initialPlayerPos, (_swingPoint - initialPlayerPos).normalized, _activeAbility.MaxSwingDistance, _controller.WhatIsSwingable);
    }

    public override BaseState GetNextState()
    {
        if (_controller.RequestedJump || _controller.HasJumpBuffered)
        {
            return new JumpingState(_controller);
        }

        return null; // Stay in SwingingState
    }

    private BaseState InvalidSwingTransition()
    {
        if (_controller.RequestedJump || _controller.HasJumpBuffered)
        {
            return new JumpingState(_controller);
        }

        if (!_controller.GroundDetector.IsGrounded)
        {
            return new FallingState(_controller);
        }
        else
        {
            return new GroundedState(_controller);
        }
    }

    public override void FixedUpdateState()
    {
        _controller.ApplyPhysics();
        ApplyPendulumPhysics();
        ApplySwingPlayerInput();
        ApplySwingDamping();
        _controller.UpdateBuffers();
        BaseState nextState = GetNextState();
        if (nextState != null)
        {
            _controller.TransitionToState(nextState);
        }
        _controller.SimulateStep();
    }

    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }

    private void CalculateInitialTangentialVelocity()
    {
        Vector3 playerPos = _controller.transform.position;
        Vector3 ropeVec = playerPos - _swingPoint;

        if (ropeVec.magnitude < _controller.Settings.FloatPrecisionThreshold)
        {
            Debug.LogWarning("Rope vector is too small, cannot calculate tangential velocity.");
            return;
        }
        Vector3 ropeDir = ropeVec.normalized;
        Vector3 tangentialDir = new Vector3(-ropeDir.y, ropeDir.x, 0);

        Vector3 initialVel = new Vector3(_controller.Velocity.x, _controller.Velocity.y, 0);
        float initialTangentialSpeed = Vector3.Dot(tangentialDir, initialVel);
        Vector3 initialTangentialVel = tangentialDir * initialTangentialSpeed;

        _controller.Velocity.x = initialTangentialVel.x;
        _controller.Velocity.y = initialTangentialVel.y;

    }
    private void ApplySpringRopeSwingConstraints()
    {
        Vector3 playerPos = _controller.transform.position;
        Vector3 ropeVec = playerPos - _swingPoint;
        float currentLen = ropeVec.magnitude;
        Vector3 direction = ropeVec.normalized;

        float targetLen = _swingRestLength;
        float lengthDiff = currentLen - targetLen;

        Vector3 springForce = Vector3.zero;
        float springConstant = _controller.SwingAbility.SpringConstant;

        if (lengthDiff > 0)
        {
            springForce = -springConstant * lengthDiff * direction;
        }
        else
        {
            springForce = Vector3.zero; 
        }

        Vector3 gravity = Physics.gravity;
        Vector3 totalForce = springForce + gravity;

        _controller.Velocity.x += totalForce.x * Time.fixedDeltaTime;
        _controller.Velocity.y += totalForce.y * Time.fixedDeltaTime;
    }

    private void ApplyPendulumPhysics()
    {
        Vector3 playerPos = _controller.transform.position;
        Vector3 ropeVec = playerPos - _swingPoint;
        float len = ropeVec.magnitude;

        if (len < _controller.Settings.FloatPrecisionThreshold)
        {
            Debug.LogWarning("Rope length too small for pendulum physics");
            return;
        }

        Vector3 direction = ropeVec / len;
        Vector3 vel = new Vector3(_controller.Velocity.x, _controller.Velocity.y, 0);

        float radialSpeed = Vector3.Dot(vel, direction);
        Vector3 radialVel = direction * radialSpeed;

        Vector3 tangentialDir = new Vector3(-direction.y, direction.x, 0);
        float tangentialSpeed = Vector3.Dot(vel, tangentialDir);

        float angle = Mathf.Atan2(ropeVec.x, -ropeVec.y);
        float angularVel = tangentialSpeed / len;
        float angularAccel = (_controller.Gravity / len) * Mathf.Sin(angle);

        angularVel += angularAccel * Time.fixedDeltaTime;
        tangentialSpeed = angularVel * len;

        Vector3 newTangentialVel = tangentialDir * tangentialSpeed;

        _controller.Velocity.x = newTangentialVel.x;
        _controller.Velocity.y = newTangentialVel.y;
    }

    public void ApplySwingPlayerInput()
    {
        Vector3 playerPos = _controller.transform.position;
        Vector3 ropeVec = playerPos - _swingPoint;

        Vector3 ropeDir = ropeVec.normalized;
        Vector3 tangentialDir = new Vector3(-ropeDir.y, ropeDir.x, 0);

        float input = _controller.HorizontalInput * _activeAbility.UserControlForce;
        Vector3 inputForce = tangentialDir * input;

        _controller.Velocity.x += inputForce.x * Time.fixedDeltaTime;
        _controller.Velocity.y += inputForce.y * Time.fixedDeltaTime;
    }

    private void ApplySwingDamping()
    {
        Vector3 ropeVec = _controller.transform.position - _swingPoint;
        float ropeLen = ropeVec.magnitude;

        if (ropeLen > _controller.Settings.FloatPrecisionThreshold)
        {
            Vector3 ropeDir = ropeVec / ropeLen;

            Vector3 vel = new Vector3(_controller.Velocity.x, _controller.Velocity.y, 0);
            float radialVel = Vector3.Dot(vel, ropeDir);
            Vector3 tangentialVel = vel - ropeDir * radialVel;

            float dampingFactor = 1f - (_activeAbility.Damping * Time.fixedDeltaTime);
            dampingFactor = Mathf.Clamp01(dampingFactor);

            Vector3 dampedTangential = tangentialVel * dampingFactor;
            Vector3 finalVel = dampedTangential + ropeDir * radialVel;

            _controller.Velocity.x = finalVel.x;
            _controller.Velocity.y = finalVel.y;
        }
    }
    private void HandleGrappleRequest(GrappleAbilitySO ability, Vector3 grapplePoint)
    {
        BaseState nextState = new GrapplingState(_controller, ability, grapplePoint);
        _controller.TransitionToState(nextState);
    }

    public override void ExitState() 
    {
        if (_grappleHandler != null)
        {
            _grappleHandler.OnGrappleRequested -= HandleGrappleRequest;
        }
    }


    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
