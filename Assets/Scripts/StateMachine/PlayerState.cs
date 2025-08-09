using UnityEngine;

public abstract class PlayerState : BaseState
{
    protected PlayerController Context;

    protected MoveActionSO ActiveMoveAction => MoveActionOverride ?? Context.MoveAction;
    protected JumpActionSO ActiveJumpAction => JumpActionOverride ?? Context.JumpAction;

    protected virtual MoveActionSO MoveActionOverride => null;
    protected virtual JumpActionSO JumpActionOverride => null;

    protected float Gravity;

    protected virtual void InitializeGravity()
    {
        Gravity = Context.CalculateFastFallGravity(ActiveMoveAction, ActiveJumpAction);
    }
}