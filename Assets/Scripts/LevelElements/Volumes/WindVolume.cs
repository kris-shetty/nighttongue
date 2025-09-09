using System.Threading;
using UnityEngine;

public class WindVolume : MonoBehaviour, IForceSource
{
    [SerializeField] private WindVolumeSO _volume;
    private int _directionSign = 1;
    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        
        float targetTime = (_directionSign == 1)
            ? _volume.FirstHalfCycleTime
            : _volume.LastHalfCycleTime;

        if (_timer >= targetTime)
        {
            _directionSign *= -1; 
            _timer = 0f;
        }
    }

    public Vector3 GetForce()
    {
        return _volume.Direction.normalized * _directionSign * _volume.ForceStrength;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IForceReceiver>(out var receiver))
        {
            receiver.RegisterForceSource(this);
        }  
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IForceReceiver>(out var receiver))
        {
            receiver.UnregisterForceSource(this);
        }
    }
}
