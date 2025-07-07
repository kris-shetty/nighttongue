using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbilitySO : ScriptableObject
{
    public string abilityName;
    public string inputActionName;
    public Sprite icon;

    public abstract void Activate(GameObject user);
}


