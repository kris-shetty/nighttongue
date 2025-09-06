using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;

public class AbilityUser : MonoBehaviour
{
    [FormerlySerializedAs("inputActions")]
    public InputActionAsset InputActions;

    [FormerlySerializedAs("abilities")]
    public AbilitySO[] Abilities;

    private AbilitySO _activeAbility;
    private InputAction _activeAction;

    // Keep delegates in a dictionary
    private Dictionary<AbilitySO, Action<InputAction.CallbackContext>> _pressDelegates = new();
    private Dictionary<AbilitySO, Action<InputAction.CallbackContext>> _holdDelegates = new();
    private Dictionary<AbilitySO, Action<InputAction.CallbackContext>> _releaseDelegates = new();

    private void OnEnable()
    {
        foreach (var ability in Abilities)
        {
            var action = InputActions.FindAction(ability.InputActionName, true);
            if (action == null) continue;

            // Prepare delegates
            _pressDelegates[ability] = ctx => OnAbilityPressed(ability, gameObject);
            _holdDelegates[ability] = ctx => ability.OnHold(gameObject);
            _releaseDelegates[ability] = ctx => ability.OnRelease(gameObject);

            // Subscribe
            action.started += _pressDelegates[ability];
            action.performed += _holdDelegates[ability];
            action.canceled += _releaseDelegates[ability];
            action.Enable();
        }
    }

    private void OnDisable()
    {
        foreach (var ability in Abilities)
        {
            var action = InputActions.FindAction(ability.InputActionName, true);
            if (action == null) continue;

            if (_pressDelegates.TryGetValue(ability, out var press)) action.started -= press;
            if (_holdDelegates.TryGetValue(ability, out var hold)) action.performed -= hold;
            if (_releaseDelegates.TryGetValue(ability, out var release)) action.canceled -= release;

            action.Disable();
        }

        _pressDelegates.Clear();
        _holdDelegates.Clear();
        _releaseDelegates.Clear();
    }

    private void OnAbilityPressed(AbilitySO ability, GameObject user)
    {
        if (_activeAbility != null && _activeAbility != ability)
        {
            _activeAbility.OnRelease(user);
        }

        _activeAbility = ability;

        _activeAbility.OnPress(user);
        _activeAbility.Activate(user);
    }

}
