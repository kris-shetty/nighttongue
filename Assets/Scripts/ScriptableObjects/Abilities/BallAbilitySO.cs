using UnityEngine;

public abstract class BallAbilitySO : ScriptableObject
{
    public virtual void OnEnterBallForm(GameObject player) { }

    public virtual void OnUpdateBallForm(GameObject player, Rigidbody rb) { }

    public virtual void OnExitBallForm(GameObject player) { }
}
