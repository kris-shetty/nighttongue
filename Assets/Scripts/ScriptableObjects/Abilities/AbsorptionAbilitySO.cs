using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Absorption")]
public class AbsorptionAbilitySO : AbilitySO
{
    public FlavorBuffSO absorbedBuff;

    public override void Activate(GameObject user)
    {
        // Apply buff to player
        Debug.Log("Absorbing: " + absorbedBuff.buffType);
    }

}

