using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SenseAbility")]
public class SenseAbilitySO : AbilitySO
{
    public float detectionRadius = 3f;

    public override void Activate(GameObject user)
    {
        // Lick logic or reveal hidden things
        Debug.Log("Activating Sense");
    }
}