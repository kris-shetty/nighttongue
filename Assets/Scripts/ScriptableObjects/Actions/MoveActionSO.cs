using UnityEngine;

[CreateAssetMenu(fileName = "Move Action", menuName = "Player/Actions/Move Action")]
public class MoveActionSO : BaseActionSO
{
    public float MaxHorizontalSpeed = 7f;
    public float MaxVerticalSpeed = 50f;
    public float Acceleration = 5f;
    public float Deceleration = 10f;
    public float GroundFrictionForce = 5f;

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(ActionName))
            ActionName = "Move";

        if (string.IsNullOrEmpty(ActionDescription))
            ActionDescription = "Movement with configurable speed, acceleration, and friction parameters";
    }
}
