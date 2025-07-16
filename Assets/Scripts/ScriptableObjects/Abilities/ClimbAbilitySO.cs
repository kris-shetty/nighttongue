using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Climb")]
public class ClimbAbilitySO : AbilitySO
{
    public float clingRange = 5f;
    public float climbSpeed = 2f;
    public LayerMask climbableSurfaces;

    public override void Activate(GameObject user)
    {
        // Add logic to enable climbing here
        Debug.Log("Activating Climb");
        // Could call a ClimbController on the user
    }
}

