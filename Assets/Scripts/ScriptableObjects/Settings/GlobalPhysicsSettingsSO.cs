using UnityEngine;

[CreateAssetMenu(fileName = "GlobalPhysicsSettings", menuName = "Settings/Global Physics Settings")]
public class GlobalPhysicsSettingsSO : ScriptableObject
{
    public float SkinWidth = 0.02f; 
    public float FloatPrecisionThreshold = 0.01f; 
    public float GrappleIgnoreDuration = 0.05f; 
}
