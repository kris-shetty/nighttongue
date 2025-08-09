using System;

public class TongueTransformEventArgs: EventArgs
{
    public bool IsTransformed { get; }
    public JumpActionSO JumpAction { get; }
    public MoveActionSO MoveAction { get; }

    public TongueTransformEventArgs(bool isTransformed, MoveActionSO move = null, JumpActionSO jump = null)
    {
        IsTransformed = isTransformed;
        JumpAction = jump;
        MoveAction = move;
    }
}
