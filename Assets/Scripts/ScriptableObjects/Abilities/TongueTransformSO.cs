using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Tongue Transform")]
public class TongueTransformSO : AbilitySO
{
    public MoveActionSO moveAction;
    public JumpActionSO jumpAction;
    public float Cooldown = 1f;

    public override void Activate(GameObject user)
    {
        var handler = user.GetComponent<TongueTransformHandler>();
        if (handler != null)
        {
            handler.ToggleTransform(this);
        }
    }
}

