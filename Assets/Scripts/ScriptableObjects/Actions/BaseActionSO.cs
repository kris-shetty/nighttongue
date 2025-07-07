using UnityEngine;

public abstract class BaseActionSO : ScriptableObject
{
    [Header("Basic Info")]
    public string ActionName;
    [TextArea(2, 4)]
    public string ActionDescription;
}
