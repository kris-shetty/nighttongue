using UnityEngine;

[CreateAssetMenu(fileName = "Throwable", menuName = "Objects/Throwable")]
public class ThrowablePropertiesSO : ScriptableObject
{
    //Throw Properties
    [Tooltip("Maximum height reached is always exactly half the maximum lateral distance covered")]
    public float MaxVerticalHeight = 10f; 

    //Hold Properties
    public float MaxHoldTime = 2f;
}
