using UnityEngine;

[CreateAssetMenu(fileName = "GlobalPhysicsSettings", menuName = "Settings/Global Physics Settings")]
public class GlobalPhysicsSettingsSO : ScriptableObject
{
    public float SkinWidth = 0.02f; // Default skin width
    public float FloatPrecisionThreshold = 0.01f; // Threshold for float comparisons
}
