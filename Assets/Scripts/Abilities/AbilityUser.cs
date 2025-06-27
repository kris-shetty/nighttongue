using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityUser : MonoBehaviour
{
    public AbilitySO[] abilities;

    void Update()
    {
        foreach (var ability in abilities)
        {
            if (Input.GetKeyDown(ability.activationKey))
            {
                ability.Activate(gameObject);
            }
        }
    }
}
