using UnityEngine;

public class BallTransformHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Rigidbody humanoidRigidbody;
    [SerializeField] public Rigidbody ballRigidbody;

    [Header("Models")]
    public GameObject humanoidModel;
    public GameObject ballModel;

    [Header("Movement")]
    public float rollForce = 10f;
    public float maxSpeed = 15f;

    [Header("Ball Abilities")]
    public BallAbilitySO[] ballAbilities;

    private bool isInBallForm = false;

    public void ToggleForm()
    {
        Debug.Log("Toggle Ball Form");
        SetBallForm(!isInBallForm);
    }

    private void SetBallForm(bool active)
    {
        isInBallForm = active;

        humanoidModel.SetActive(!active);
        ballModel.SetActive(active);

        playerController.enabled = !active;
        humanoidRigidbody.isKinematic = active;
        ballRigidbody.isKinematic = !active;

        if (active)
        {
            foreach (var ability in ballAbilities)
                ability.OnEnterBallForm(gameObject);
        }
        else
        {
            foreach (var ability in ballAbilities)
                ability.OnExitBallForm(gameObject);
        }
    }

    private void Update()
    {
        if (isInBallForm)
        {
            foreach (var ability in ballAbilities)
                ability.OnUpdateBallForm(gameObject, ballRigidbody);
        }
    }

}
