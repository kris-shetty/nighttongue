using Unity.VisualScripting;
using UnityEngine;

public abstract class PlayerState : BaseState
{
    protected PlayerController Context;

    protected MoveActionSO ActiveMoveAction => MoveActionOverride ?? Context.MoveAction;
    protected JumpActionSO ActiveJumpAction => JumpActionOverride ?? Context.JumpAction;

    protected virtual MoveActionSO MoveActionOverride => null;
    protected virtual JumpActionSO JumpActionOverride => null;

    protected float Gravity;

    public sealed override void EnterState()
    {
        InitializeGravity();
        OnEnter();
    }

    public sealed override void ExitState()
    {
        OnExit();
    }

    protected abstract void OnEnter();
    protected abstract void OnExit();
    protected virtual void InitializeGravity()
    {
        Gravity = Context.CalculateFastFallGravity(ActiveMoveAction, ActiveJumpAction);
    }
}