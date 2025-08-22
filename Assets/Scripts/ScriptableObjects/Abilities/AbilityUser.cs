using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class AbilityUser : MonoBehaviour
{
    [FormerlySerializedAs("inputActions")]
    public InputActionAsset InputActions;

    [FormerlySerializedAs("abilities")]
    public AbilitySO[] Abilities;

    private void OnEnable()
    {
        foreach (var ability in Abilities)
        {
            var action = InputActions.FindAction(ability.InputActionName, true);
            if (action != null)
            {
                action.started += ctx => ability.OnPress(gameObject);
                action.performed += ctx => ability.OnHold(gameObject);
                action.performed += ctx => ability.Activate(gameObject);
                action.canceled += ctx => ability.OnRelease(gameObject);
                action.Enable();
            }
        }
    }

    private void OnDisable()
    {
        foreach (var ability in Abilities)
        {
            var action = InputActions.FindAction(ability.InputActionName, true);
            if (action != null)
            {
                action.started -= ctx => ability.OnPress(gameObject);
                action.performed -= ctx => ability.OnHold(gameObject);
                action.performed -= ctx => ability.Activate(gameObject);
                action.canceled -= ctx => ability.OnRelease(gameObject);
                action.Disable();
            }
        }
    }
}
