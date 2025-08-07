using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ball Abilities/Boost on Transform")]
public class BallBoostAbilitySO : BallAbilitySO
{
    public float boostForce = 20f;

    public override void OnEnterBallForm(GameObject player)
    {
        var handler = player.GetComponent<BallTransformHandler>();
        if (handler != null && handler.ballRigidbody != null)
        {
            Vector3 forward = player.transform.forward;
            handler.ballRigidbody.AddForce(forward * boostForce, ForceMode.Impulse);
        }
    }
}
