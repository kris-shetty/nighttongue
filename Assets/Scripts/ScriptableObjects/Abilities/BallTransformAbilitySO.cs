using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ball Transform")]
public class BallTransformAbilitySO : AbilitySO
{
    public override void Activate(GameObject user)
    {
        throw new System.NotImplementedException();
    }

    public override void OnPress(GameObject user)
    {
        var handler = user.GetComponent<BallTransformHandler>();
        handler?.ToggleForm();
    }
}
