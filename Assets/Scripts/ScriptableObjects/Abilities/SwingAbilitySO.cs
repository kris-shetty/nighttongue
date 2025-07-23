using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Swing")]
public class SwingAbilitySO : AbilitySO
{
    public float MaxSwingDistance = 10f;
    public float DelayTime = 1f;
    public float Cooldown = 2f;
    public float MaxSwingAngle = 45f;
    public float SpringConstant = 0.5f;
    public float Damping = 0.1f;
    public float UserControlForce = 5f;
    public override void Activate(GameObject user)
    {
        var swingHandler = user.GetComponent<SwingHandler>();
        if(swingHandler != null)
        {
            swingHandler.ToggleSwing(this);
        }
    }
}
