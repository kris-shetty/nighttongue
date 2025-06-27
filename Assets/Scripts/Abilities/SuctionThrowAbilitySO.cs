using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SuctionThrowAbility")]
public class SuctionThrowAbilitySO : AbilitySO
{
    public float suctionForce = 20f;
    public float maxWeight = 10f;

    public override void Activate(GameObject user)
    {
        // suction logic here (e.g., raycast + force)
        Debug.Log("Activating Suction");
    }
}

