using UnityEngine;
using System;

public class TongueTransformHandler : MonoBehaviour
{
    private PlayerController _playerController;
    private TongueTransformSO _activeAbility;
    private float _currentCooldownDuration = 0f;
    private float _transformTimer = 0f;
    private bool _isTransformingOnCooldown = false;
    private bool _isTransformed = false;
    [SerializeField] private GameObject _base;
    [SerializeField] private GameObject _transformed;
    private PushOffOverhang _pushOffOverhang;

    public event Action<TongueTransformEventArgs> OnTransformStateChanged;

    public void ToggleTransform(TongueTransformSO tongueTransform)
    {
        _activeAbility = tongueTransform;
        _currentCooldownDuration = _activeAbility.Cooldown;
        
        if (_transformTimer > 0f)
            return;

        if (_isTransformed)
        {
            UntoggleTransform();
            return;
        }
        else
        {
            ExecuteTransform();
        }
    }

    private void ExecuteTransform()
    {
        _base.SetActive(false);
        _transformed.SetActive(true);
        _pushOffOverhang.enabled = false; 
        _isTransformed = true;
        OnTransformStateChanged?.Invoke(new TongueTransformEventArgs(true, _activeAbility.moveAction, _activeAbility.jumpAction));
    }

    public void UntoggleTransform()
    {
        _transformed.SetActive(false);
        _base.SetActive(true);
        _isTransformingOnCooldown = true;
        _pushOffOverhang.enabled = true;
        _isTransformed = false;
        OnTransformStateChanged?.Invoke(new TongueTransformEventArgs(false, _activeAbility.moveAction, _activeAbility.jumpAction));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _pushOffOverhang = GetComponent<PushOffOverhang>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isTransformingOnCooldown)
        {
            _transformTimer += Time.deltaTime;
        }

        if (_transformTimer >= _currentCooldownDuration)
        {
            _isTransformingOnCooldown = false;
            _transformTimer = 0f;
        }
    }
}
