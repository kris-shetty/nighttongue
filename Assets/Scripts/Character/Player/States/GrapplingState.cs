using System.Runtime.CompilerServices;
using UnityEngine;

public class GrapplingState : BaseState
{
    private PlayerController _controller;
    private float _initialVerticalSpeed;
    private float _initialHorizontalSpeed;
    private Vector3 _grapplePoint;
    private Vector3 _initialPlayerPos;
    private GrappleAbilitySO _activeAbility;
    private CollideSlideCharacterCollisionResolver _collider;
    private float _grappleEntryTimer = 0f;
    private float _grappleIgnoreDuration = 0.05f;

    public GrapplingState(PlayerController controller)
    {
        _controller = controller;
        _collider = _controller.GetComponent<CollideSlideCharacterCollisionResolver>();
        _grapplePoint = _controller.GrappleTargetPoint;
        _initialPlayerPos = _controller.transform.position;
        _grappleIgnoreDuration = _controller.Settings.GrappleIgnoreDuration;
        _activeAbility = _controller.GrappleAbility;
    }

    private float CalculateVerticalHeight()
    {
        float highestPoint = 0;
        
        if(_controller.GrappleTargetPoint == null)
        {
            Debug.LogError("Grapple target point is not set.");
            return highestPoint;
        }

         float verticalDistance = _grapplePoint.y - _initialPlayerPos.y;
        if(verticalDistance < 0)
        {
            highestPoint = _activeAbility.OvershootHeight;
        }
        else
        {
            highestPoint = verticalDistance + _activeAbility.OvershootHeight;
        }

        return highestPoint;
    }

    private void CalculateJumpVelocity()
    {
        float highestPoint = CalculateVerticalHeight();
        float verticalDistance = _grapplePoint.y - _initialPlayerPos.y;
        float horizontalDistance = _grapplePoint.x - _initialPlayerPos.x;
        _initialVerticalSpeed = Mathf.Sqrt(-2 * _controller.JumpGravity * highestPoint);
        float timeUp = Mathf.Sqrt((-2 * highestPoint)/_controller.JumpGravity);
        float timeDown = Mathf.Sqrt((-2 * (highestPoint - verticalDistance) / _controller.FastFallGravity));
        _initialHorizontalSpeed = horizontalDistance / (timeUp + timeDown);
    }

    private void ApplyGrapple()
    {
        CalculateJumpVelocity();
        _controller.Gravity = _controller.JumpGravity;
        _controller.Velocity.y = _initialVerticalSpeed;
        _controller.Velocity.x = _initialHorizontalSpeed;
    }

    private bool IsGrappleValid()
    {
        float distanceToTarget = Vector3.Distance(_initialPlayerPos, _grapplePoint);
        if (distanceToTarget > _activeAbility.MaxGrappleDistance)
        {
            Debug.LogWarning("GrapplingState :: Grapple target is out of range.");
            return false;
        }
        return Physics.Raycast(_initialPlayerPos, (_grapplePoint - _initialPlayerPos).normalized, _activeAbility.MaxGrappleDistance, _controller.WhatIsGrappable);
    }

    public override void EnterState()
    {
        Debug.Log("GrapplingState :: Is it in yet?");
        _collider.OnCollisionDetected += OnPlayerCollision;
        if (IsGrappleValid())
        {
            ApplyGrapple();
            _controller.RequestedGrapple = false;
        }
        else
        {
            BaseState nextState = GetNextState();
            if (nextState != null)
            {
                _controller.TransitionToState(nextState);
            }
            else
            {
                Debug.LogError("GrapplingState :: No valid next state found after grapple validation.");
            }
        }
    }

    private void OnPlayerCollision()
    {
        if(_grappleEntryTimer < _grappleIgnoreDuration)
        {
            return;
        }
        
        BaseState nextState = GetNextState();
        if (nextState != null)
        {
            _controller.TransitionToState(nextState);
        }
        else
        {
            Debug.LogError("GrapplingState :: No valid next state found after grapple validation.");
        }
    }

    public override void ExitState()
    {
        _collider.OnCollisionDetected -= OnPlayerCollision;
    }

    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }

    public override void FixedUpdateState()
    {
        _grappleEntryTimer += Time.fixedDeltaTime;

        _controller.ApplyPhysics();
        if(_controller.Velocity.y <= 0f)
        {
            _controller.Gravity = _controller.FastFallGravity;
        }
        BaseState nextState = GetNextState();
        if (nextState is GrapplingState)
        {
            _controller.TransitionToState(nextState);
        }
        _controller.ApplyVerticalMovement();
        _controller.SimulateStep();
    }

    public override BaseState GetNextState()
    {
        if (_controller.RequestedGrapple)
        {
            return new GrapplingState(_controller);
        }

        if(_controller.RequestedSwing)
        {
              return new SwingingState(_controller);
        }

        if(_controller.RequestedJump || _controller.HasJumpBuffered)
        {
            return new JumpingState(_controller);
        }

        if (_controller.GroundDetector.IsGrounded)
        {
            return new GroundedState(_controller);
        }
        else
        {
            return new FallingState(_controller);
        }   
    }

    public override void OnTriggerEnter(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
    public override void OnTriggerExit(Collider other) { }
}
