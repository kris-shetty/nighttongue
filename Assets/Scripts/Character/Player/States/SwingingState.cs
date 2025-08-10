using UnityEngine;

public class SwingingState : PlayerState
{
    private Vector3 _swingPoint;
    private SwingAbilitySO _activeAbility;
    private float _swingRestLength;

    private GrappleHandler _grappleHandler;
    private SwingHandler _swingHandler;

    public SwingingState(PlayerController controller, SwingAbilitySO swingAbility, Vector3 swingPoint)
    {
        Context = controller;
        _activeAbility = swingAbility;
        _swingPoint = swingPoint;
    }

    protected override void OnEnter()
    {
        _grappleHandler = Context.GetComponent<GrappleHandler>();
        if (_grappleHandler == null)
        {
            Debug.LogError("SwingingState :: GrappleHandler component not found on PlayerController.");
            return;
        }
        _grappleHandler.OnGrappleRequested += HandleGrappleRequest;

        Debug.Log("Entering SwingingState; Yippee!");
        
        if (!IsSwingValid())
        {
            BaseState nextState = InvalidSwingTransition();
            if (nextState != null)
            {
                Context.TransitionToState(nextState);
            }
            else
            {
                Debug.LogError("SwingingState :: No valid next state found after grapple validation. How the hell did you get here?");
            }
        }
        _swingRestLength = (Context.transform.position - _swingPoint).magnitude;
        CalculateInitialTangentialVelocity();
    }
    private bool IsSwingValid()
    {
        Vector3 initialPlayerPos = Context.transform.position;
        float distanceToTarget = Vector3.Distance(initialPlayerPos, _swingPoint);
        if (distanceToTarget > _activeAbility.MaxSwingDistance)
        {
            Debug.LogWarning("GrapplingState :: Grapple target is out of range.");
            return false;
        }
        return Physics.Raycast(initialPlayerPos, (_swingPoint - initialPlayerPos).normalized, _activeAbility.MaxSwingDistance, Context.WhatIsSwingable);
    }

    public override BaseState GetNextState()
    {
        if (Context.RequestedJump || Context.HasJumpBuffered)
        {
            return new JumpingState(Context);
        }

        return null; // Stay in SwingingState
    }

    private BaseState InvalidSwingTransition()
    {
        if (Context.RequestedJump || Context.HasJumpBuffered)
        {
            return new JumpingState(Context);
        }

        if (!Context.GroundDetector.IsGrounded)
        {
            return new FallingState(Context);
        }
        else
        {
            return new GroundedState(Context);
        }
    }

    public override void FixedUpdateState()
    {
        Context.ApplyPhysics();
        ApplyPendulumPhysics();
        ApplySwingPlayerInput();
        ApplySwingDamping();
        Context.UpdateBuffers();
        BaseState nextState = GetNextState();
        if (nextState != null)
        {
            Context.TransitionToState(nextState);
        }
        Context.SimulateStep();
    }

    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }

    private void CalculateInitialTangentialVelocity()
    {
        Vector3 playerPos = Context.transform.position;
        Vector3 ropeVec = playerPos - _swingPoint;

        if (ropeVec.magnitude < Context.Settings.FloatPrecisionThreshold)
        {
            Debug.LogWarning("Rope vector is too small, cannot calculate tangential velocity.");
            return;
        }
        Vector3 ropeDir = ropeVec.normalized;
        Vector3 tangentialDir = new Vector3(-ropeDir.y, ropeDir.x, 0);

        Vector3 initialVel = new Vector3(Context.Velocity.x, Context.Velocity.y, 0);
        float initialTangentialSpeed = Vector3.Dot(tangentialDir, initialVel);
        Vector3 initialTangentialVel = tangentialDir * initialTangentialSpeed;

        Context.Velocity.x = initialTangentialVel.x;
        Context.Velocity.y = initialTangentialVel.y;

    }
    private void ApplySpringRopeSwingConstraints()
    {
        Vector3 playerPos = Context.transform.position;
        Vector3 ropeVec = playerPos - _swingPoint;
        float currentLen = ropeVec.magnitude;
        Vector3 direction = ropeVec.normalized;

        float targetLen = _swingRestLength;
        float lengthDiff = currentLen - targetLen;

        Vector3 springForce = Vector3.zero;
        float springConstant = _activeAbility.SpringConstant;

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

        Context.Velocity.x += totalForce.x * Time.fixedDeltaTime;
        Context.Velocity.y += totalForce.y * Time.fixedDeltaTime;
    }

    private void ApplyPendulumPhysics()
    {
        Vector3 playerPos = Context.transform.position;
        Vector3 ropeVec = playerPos - _swingPoint;
        float len = ropeVec.magnitude;

        if (len < Context.Settings.FloatPrecisionThreshold)
        {
            Debug.LogWarning("Rope length too small for pendulum physics");
            return;
        }

        Vector3 direction = ropeVec / len;
        Vector3 vel = new Vector3(Context.Velocity.x, Context.Velocity.y, 0);

        float radialSpeed = Vector3.Dot(vel, direction);
        Vector3 radialVel = direction * radialSpeed;

        Vector3 tangentialDir = new Vector3(-direction.y, direction.x, 0);
        float tangentialSpeed = Vector3.Dot(vel, tangentialDir);

        float angle = Mathf.Atan2(ropeVec.x, -ropeVec.y);
        float angularVel = tangentialSpeed / len;
        float angularAccel = (Physics.gravity.y / len) * Mathf.Sin(angle);

        angularVel += angularAccel * Time.fixedDeltaTime;
        tangentialSpeed = angularVel * len;

        Vector3 newTangentialVel = tangentialDir * tangentialSpeed;

        Context.Velocity.x = newTangentialVel.x;
        Context.Velocity.y = newTangentialVel.y;
    }

    public void ApplySwingPlayerInput()
    {
        Vector3 playerPos = Context.transform.position;
        Vector3 ropeVec = playerPos - _swingPoint;

        Vector3 ropeDir = ropeVec.normalized;
        Vector3 tangentialDir = new Vector3(-ropeDir.y, ropeDir.x, 0);

        float input = Context.HorizontalInput * _activeAbility.UserControlForce;
        Vector3 inputForce = tangentialDir * input;

        Context.Velocity.x += inputForce.x * Time.fixedDeltaTime;
        Context.Velocity.y += inputForce.y * Time.fixedDeltaTime;
    }

    private void ApplySwingDamping()
    {
        Vector3 ropeVec = Context.transform.position - _swingPoint;
        float ropeLen = ropeVec.magnitude;

        if (ropeLen > Context.Settings.FloatPrecisionThreshold)
        {
            Vector3 ropeDir = ropeVec / ropeLen;

            Vector3 vel = new Vector3(Context.Velocity.x, Context.Velocity.y, 0);
            float radialVel = Vector3.Dot(vel, ropeDir);
            Vector3 tangentialVel = vel - ropeDir * radialVel;

            float dampingFactor = 1f - (_activeAbility.Damping * Time.fixedDeltaTime);
            dampingFactor = Mathf.Clamp01(dampingFactor);

            Vector3 dampedTangential = tangentialVel * dampingFactor;
            Vector3 finalVel = dampedTangential + ropeDir * radialVel;

            Context.Velocity.x = finalVel.x;
            Context.Velocity.y = finalVel.y;
        }
    }
    private void HandleGrappleRequest(GrappleAbilitySO ability, Vector3 grapplePoint)
    {
        BaseState nextState = new GrapplingState(Context, ability, grapplePoint);
        Context.TransitionToState(nextState);
    }

    protected override void OnExit() 
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
