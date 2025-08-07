using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Abilities/Ball Abilities/Boost on Transform")]
public class BallBoostAbilitySO : BallAbilitySO
{
    [FormerlySerializedAs("boostForce")]
    public float BoostForce = 20f;

    public override void OnEnterBallForm(GameObject user)
    {
        var handler = user.GetComponent<BallTransformHandler>();
        if (handler != null && handler.BallRigidbody != null)
        {
            Vector3 forward = user.transform.forward;
            handler.BallRigidbody.AddForce(forward * BoostForce, ForceMode.Impulse);
        }
    }
}
