using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Abilities/Suction Throw")]
public class SuctionThrowAbilitySO : AbilitySO
{
    [FormerlySerializedAs("suctionRange")]
    public float SuctionRange = 6f;

    [FormerlySerializedAs("suctionForce")]
    public float SuctionForce = 10f;

    [FormerlySerializedAs("suctionConeAngle")]
    public float SuctionConeAngle = 45f;

    public override void Activate(GameObject user)
    {
    }

    public override void OnPress(GameObject user)
    {
        var handler = user.GetComponent<SuctionHandler>();
        handler?.BeginSuction(this);
    }

    public override void OnRelease(GameObject user)
    {
        var handler = user.GetComponent<SuctionHandler>();
        handler?.ThrowHeldObject();
    }
}
