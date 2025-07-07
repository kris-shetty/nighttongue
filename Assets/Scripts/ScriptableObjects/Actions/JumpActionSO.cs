using UnityEngine;

[CreateAssetMenu(fileName = "Jump Action", menuName = "Player/Actions/Jump Action")]
public class JumpActionSO: BaseActionSO
{
    public float MaxJumpHeight = 5f;
    public float MaxJumpLateralDistance = 10f;
    public float FastFallMultiplier = 3f;

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(ActionName))
            ActionName = "Jump";

        if (string.IsNullOrEmpty(ActionDescription))
            ActionDescription = "Basic jump action with parameterized controls";
    }
}
