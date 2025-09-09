using UnityEngine;

[CreateAssetMenu(fileName = "State Multiplier", menuName = "Level Elements/Wind/State Multiplier")]
public class StateMultiplierSO : ScriptableObject
{
    public float Grounded;
    public float Airborne;
    public float Grappling;
    public float Swinging;
}
