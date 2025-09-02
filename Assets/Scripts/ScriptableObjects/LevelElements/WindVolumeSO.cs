using UnityEngine;

[CreateAssetMenu(fileName = "Wind Volume", menuName = "Level Elements/Wind Volume")]
public class WindVolumeSO : ScriptableObject
{
    public Vector3 Direction;
    public float FirstHalfCycleTime;
    public float LastHalfCycleTime;
    public float ForceStrength;
}
