using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityUser : MonoBehaviour
{
    public InputActionAsset inputActions;
    public AbilitySO[] abilities;

    private void OnEnable()
    {
        foreach (var ability in abilities)
        {
            var action = inputActions.FindAction(ability.inputActionName, true);
            if (action != null)
            {
                action.performed += ctx => ability.Activate(gameObject);
                action.Enable();
            }
        }
    }

    private void OnDisable()
    {
        foreach (var ability in abilities)
        {
            var action = inputActions.FindAction(ability.inputActionName, true);
            if (action != null)
            {
                action.performed -= ctx => ability.Activate(gameObject);
                action.Disable();
            }
        }
    }
}
