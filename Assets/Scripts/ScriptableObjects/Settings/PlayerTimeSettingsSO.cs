using UnityEngine;

[CreateAssetMenu(fileName = "PlayerTimeSettings", menuName = "Player/Time")]
public class PlayerTimeSettingsSO : ScriptableObject
{
    public float JumpBufferTime = 0.2f;
    public float CoyoteTime = 0.1f;
}
