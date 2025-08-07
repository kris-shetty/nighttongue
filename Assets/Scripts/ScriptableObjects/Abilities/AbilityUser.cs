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
                action.started += ctx => ability.OnPress(gameObject);
                action.performed += ctx => ability.OnHold(gameObject);
                action.canceled += ctx => ability.OnRelease(gameObject);
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
                action.started -= ctx => ability.OnPress(gameObject);
                action.performed -= ctx => ability.OnHold(gameObject);
                action.canceled -= ctx => ability.OnRelease(gameObject);
                action.Disable();
            }
        }
    }
}

