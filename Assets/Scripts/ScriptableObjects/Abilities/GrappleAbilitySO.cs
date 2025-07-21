using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Grapple")]
public class GrappleAbilitySO : AbilitySO
{
    public float MaxGrappleDistance = 10f;
    public float DelayTime = 1f;
    public float Cooldown = 2f;
    public float OvershootHeight = 1f;

    public override void Activate(GameObject user)
    {
        var grappleHandler = user.GetComponent<GrappleHandler>();
        if (grappleHandler != null)
        {
            grappleHandler.ToggleGrapple(this);
        }
    }
}
