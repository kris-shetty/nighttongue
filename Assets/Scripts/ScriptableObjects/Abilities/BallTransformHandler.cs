using UnityEngine;
using UnityEngine.Serialization;

public class BallTransformHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Rigidbody _humanoidRigidbody;

    [FormerlySerializedAs("ballRigidbody")]
    public Rigidbody BallRigidbody;

    [Header("Models")]
    [FormerlySerializedAs("humanoidModel")]
    public GameObject HumanoidModel;

    [FormerlySerializedAs("ballModel")]
    public GameObject BallModel;

    [Header("Movement")]
    [FormerlySerializedAs("rollForce")]
    public float RollForce = 10f;

    [FormerlySerializedAs("maxSpeed")]
    public float MaxSpeed = 15f;

    [Header("Ball Abilities")]
    [FormerlySerializedAs("ballAbilities")]
    public BallAbilitySO[] BallAbilities;

    private bool _isInBallForm = false;

    public void ToggleForm()
    {
        Debug.Log("Toggle Ball Form");
        SetBallForm(!_isInBallForm);
    }

    private void SetBallForm(bool active)
    {
        _isInBallForm = active;

        HumanoidModel.SetActive(!active);
        BallModel.SetActive(active);

        _playerController.enabled = !active;
        _humanoidRigidbody.isKinematic = active;
        BallRigidbody.isKinematic = !active;

        if (active)
        {
            foreach (var ability in BallAbilities)
            {
                ability.OnEnterBallForm(gameObject);
            }
        }
        else
        {
            foreach (var ability in BallAbilities)
            {
                ability.OnExitBallForm(gameObject);
            }
        }
    }

    private void Update()
    {
        if (_isInBallForm)
        {
            foreach (var ability in BallAbilities)
            {
                ability.OnUpdateBallForm(gameObject, BallRigidbody);
            }
        }
    }
}
