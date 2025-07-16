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
        var handler = user.GetComponent<SuctionHandler>();
        if (handler != null)
        {
            handler.ToggleSuction(this);
        }
    }
}

