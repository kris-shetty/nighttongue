using UnityEngine;

public abstract class BallAbilitySO : ScriptableObject
{
    public virtual void OnEnterBallForm(GameObject user) { }

    public virtual void OnUpdateBallForm(GameObject user, Rigidbody rigidbody) { }

    public virtual void OnExitBallForm(GameObject user) { }
}
