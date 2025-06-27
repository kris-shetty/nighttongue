using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType { Heal, Speed, Strength, Shrink }

[CreateAssetMenu(menuName = "Abilities/FlavorBuff")]
public class FlavorBuffSO : ScriptableObject
{
    public BuffType buffType;
    public float duration;
    public float effectStrength;
}

