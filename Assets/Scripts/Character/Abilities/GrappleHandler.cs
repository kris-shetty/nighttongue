using UnityEngine;
using System;

public class GrappleHandler : MonoBehaviour
{
    private GameObject _tongue;
    private TongueController _tongueController;
    private PlayerController _playerController;
    private GrappleAbilitySO _activeAbility;
    private float _grappleTimer = 0f;
    private bool _isGrapplingOnCooldown = false;
    public LayerMask WhatIsGrappable;
    private Vector3 _grapplePoint;
    private PlayerInputHandler _inputHandler;
    private float _currentCooldownDuration = 0f;

    public event Action<GrappleAbilitySO, Vector3> OnGrappleRequested;

    public void ToggleGrapple(GrappleAbilitySO ability)
    {
        _activeAbility = ability;

        Debug.Log("Is this stupid thing working?");

        if (_grappleTimer > 0f)
            return;

        _currentCooldownDuration = _activeAbility.Cooldown;
        _isGrapplingOnCooldown = true;

        RaycastHit hit;
        if(Physics.Raycast(transform.position, _tongueController.Direction, out hit, _activeAbility.MaxGrappleDistance, WhatIsGrappable))
        {
            _grapplePoint = hit.point;
            Debug.DrawLine(transform.position, _grapplePoint, Color.cyan, 2f);
            _inputHandler.FreezeInput(_activeAbility.DelayTime);
            Invoke(nameof(ExecuteGrapple), _activeAbility.DelayTime);
        }
        else
        {
            _grapplePoint = transform.position + _tongueController.Direction * _activeAbility.MaxGrappleDistance;
            Debug.DrawLine(transform.position, _grapplePoint, Color.red, 2f);
            _inputHandler.FreezeInput(_activeAbility.DelayTime);
            Invoke(nameof(UntoggleGrapple), _activeAbility.DelayTime);
        }
        
    }

    private void ExecuteGrapple()
    {
        OnGrappleRequested?.Invoke(_activeAbility, _grapplePoint);
    }

    public void UntoggleGrapple()
    {
        Debug.Log("GrappleHandler :: Unsuccessful grapple.");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if( _isGrapplingOnCooldown )
        {
            _grappleTimer += Time.deltaTime;
        }

        if (_grappleTimer >= _currentCooldownDuration)
        {
            _isGrapplingOnCooldown = false;
            _grappleTimer = 0f;
        }
    }

    private void Awake()
    {
        _tongue = transform.Find("Tongue").gameObject;
        _inputHandler = GetComponent<PlayerInputHandler>();
        _playerController = GetComponent<PlayerController>();

        if (_tongue == null)
        {
            Debug.LogError("GrappleHandler: Tongue GameObject not found. Please ensure it is attached as a child to this GameObject.");
        }
        else
        {
            _tongueController = _tongue.GetComponent<TongueController>();
            if (_tongueController == null)
            {
                Debug.LogError("GrappleHandler :: TongueController component not found on the Tongue GameObject.");
            }
        }

        if (_inputHandler == null)
        {
            Debug.LogError("GrappleHandler :: PlayerInputHandler component not found on the GameObject.");
        }

        if(_playerController == null)
        {
            Debug.Log("GrappleHandler :: PlayerController component not found on the GameObject.");
        }

        _playerController.WhatIsGrappable = WhatIsGrappable;
    }
}
