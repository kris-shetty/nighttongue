using System;
using UnityEngine;

public class SwingHandler : MonoBehaviour
{
    private GameObject _tongue;
    private TongueController _tongueController;
    private PlayerController _playerController;
    private PlayerInputHandler _inputHandler;
    public SwingAbilitySO ActiveAbility;
    private float _swingTimer = 0f;
    private bool _isSwingingOnCooldown = false;
    public LayerMask WhatIsSwingable;
    private Vector3 _swingPoint;
    private float _currentCooldownDuration = 0f;

    public event Action<SwingAbilitySO, Vector3> OnSwingRequested;
    public void ToggleSwing(SwingAbilitySO ability)
    {
        ActiveAbility = ability;

        if (_swingTimer > 0f)
            return;

        _currentCooldownDuration = ActiveAbility.Cooldown;
        _isSwingingOnCooldown = true;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, _tongueController.Direction, out hit, ActiveAbility.MaxSwingDistance, WhatIsSwingable))
        {
            _swingPoint = hit.point;
            Debug.DrawLine(transform.position, _swingPoint, Color.green, 2f);
            _inputHandler.FreezeInput(ActiveAbility.DelayTime);
            Invoke(nameof(ExecuteSwing), ActiveAbility.DelayTime);
        }
        else
        {
            _swingPoint = transform.position + _tongueController.Direction * ActiveAbility.MaxSwingDistance;
            Debug.DrawLine(transform.position, _swingPoint, Color.red, 2f);
            _inputHandler.FreezeInput(ActiveAbility.DelayTime);
            Invoke(nameof(UntoggleSwing), ActiveAbility.DelayTime);
        }
    }

    private void ExecuteSwing()
    {
        OnSwingRequested?.Invoke(ActiveAbility, _swingPoint);
    }

    public void UntoggleSwing()
    {
        _playerController.RequestedSwing = false;
    }

    private void Update()
    {
        if(_isSwingingOnCooldown)
        {
            _swingTimer += Time.deltaTime;
        }

        if(_swingTimer >= _currentCooldownDuration)
        {
            _isSwingingOnCooldown = false;
            _swingTimer = 0f;
        }
    }

    private void Awake()
    {
        _tongue = transform.Find("Tongue").gameObject;
        _inputHandler = GetComponent<PlayerInputHandler>();
        _playerController = GetComponent<PlayerController>();

        if (_tongue == null)
        {
            Debug.LogError("SwingHandler :: Tongue GameObject not found. Please ensure it is attached as a child to this GameObject.");
        }
        else
        {
            _tongueController = _tongue.GetComponent<TongueController>();
            if (_tongueController == null)
            {
                Debug.LogError("SwingHandler :: TongueController component is missing from the Tongue GameObject.");
            }
        }

        if (_inputHandler == null)
        {
            Debug.LogError("SwingHandler :: PlayerInputHandler component is missing from the GameObject.");
        }

        if (_playerController == null)
        {
            Debug.LogError("SwingHandler :: PlayerController component is missing from the GameObject.");
        }

        _playerController.WhatIsSwingable = WhatIsSwingable;
    }
}
