using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilitySO : ScriptableObject
{
    public string abilityName;
    public Sprite icon;
    public KeyCode activationKey;

    // call this when the ability is activated
    public abstract void Activate(GameObject user);
}

