using UnityEngine;

public class ImpulseForceSource : IForceSource
{
    private readonly Vector3 impulse;
    private bool consumed = false;

    public ImpulseForceSource(Vector3 impulse)
    {
        this.impulse = impulse;
    }

    public Vector3 GetForce()
    {
        if (consumed) return Vector3.zero;
        consumed = true;
        return impulse;
    }

    public bool IsActive => !consumed;
}
