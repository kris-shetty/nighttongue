using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Suction Throw")]
public class SuctionThrowAbilitySO : AbilitySO
{
    public float suctionRange = 6f;
    public float suctionForce = 10f;
    public float suctionConeAngle = 45f;

    public override void Activate(GameObject user)
    {

    }

    public override void OnPress(GameObject user)
    {
        var handler = user.GetComponent<SuctionHandler>();
        handler?.BeginSuction(this);
    }

    //public override void OnHold(GameObject user)
    //{
    //    var handler = user.GetComponent<SuctionHandler>();
    //    handler?.UpdateSuction();
    //}

    public override void OnRelease(GameObject user)
    {
        var handler = user.GetComponent<SuctionHandler>();
        Debug.Log("on release");
        handler?.ThrowHeldObject();
    }
}


