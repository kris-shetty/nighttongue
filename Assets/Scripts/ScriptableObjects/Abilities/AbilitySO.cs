using UnityEngine;
using UnityEngine.Serialization;

public abstract class AbilitySO : ScriptableObject
{
    [FormerlySerializedAs("abilityName")] public string AbilityName;
    [FormerlySerializedAs("inputActionName")] public string InputActionName;
    [FormerlySerializedAs("icon")] public Sprite Icon;

    public abstract void Activate(GameObject user);
    public virtual void OnPress(GameObject user) { }
    public virtual void OnHold(GameObject user) { }
    public virtual void OnRelease(GameObject user) { }
}
