using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SuctionThrowAbility")]
public class SuctionThrowAbilitySO : AbilitySO
{
    public float suctionRange = 6f;
    public float suctionForce = 15f;
    public float maxHoldTime = 2f;
    public float throwForceMultiplier = 25f;

    public override void Activate(GameObject user)
    {
        var handler = user.GetComponent<SuctionHandler>();
        if (handler != null)
        {
            handler.ToggleSuction(this);
        }
    }
}

