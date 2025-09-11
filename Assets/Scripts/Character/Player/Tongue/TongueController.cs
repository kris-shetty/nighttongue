using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TongueController : MonoBehaviour
{
    [SerializeField] private LayerMask _aimPlaneMask;
    public GlobalPhysicsSettingsSO Settings;

    private LineRenderer _tongue;
    private GameObject _player;

    private Coroutine _activeRoutine;
    private TongueMode _mode = TongueMode.Aim;

    public float TongueLength = 2f;
    public Vector3 EndPoint;
    public Vector3 Direction;

    private enum TongueMode
    {
        Aim,
        Extending,
        Attached
    }

    private Vector3 GetMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _aimPlaneMask))
        {
            return hit.point;
        }

        return _player.transform.position;
    }

    private void UpdateAim()
    {
        Vector3 mouseWorldPos = GetMouseWorldPos();

        Direction = (mouseWorldPos - _player.transform.position);
        Direction.z = 0f;
        Direction = Direction.normalized;

        EndPoint = _player.transform.position + Direction * TongueLength;
    }

    private void UpdateTongue()
    {
        _tongue.SetPosition(0, _player.transform.position);
        _tongue.SetPosition(1, EndPoint);
    }

    private void Awake()
    {
        _tongue = GetComponent<LineRenderer>();
        _player = GameObject.FindWithTag("Player");

        if (_tongue == null)
        {
            Debug.LogError("TongueController: LineRenderer component is missing from the TongueController.");
        }

        if (_player == null)
        {
            Debug.LogError("TongueController: Player GameObject not found. Ensure it has the 'Player' tag.");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _tongue.startWidth = 0.5f;
        _tongue.endWidth = 0.0f;
    }

    // Update is called once per frame
    private void Update()
    {
        switch(_mode)
        {
            case TongueMode.Aim:
                UpdateAim();
                break;
        }
        UpdateTongue();
    }
    public void ExtendTongue(Vector3 target, float duration)
    {
        if (_mode == TongueMode.Aim)
        {
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
            }
            _activeRoutine = StartCoroutine(ExtendTongueRoutine(target, duration));
        }
        
    }

    public void AimTongue()
    {
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
        }
        _mode = TongueMode.Aim;
        EndPoint = _player.transform.position + Direction * TongueLength;
        _activeRoutine = null;
    }

    private IEnumerator ExtendTongueRoutine(Vector3 target, float duration)
    {
        Debug.Log("TongueController :: Extending tongue.");
        _mode = TongueMode.Extending;
        Vector3 start = EndPoint;
        Vector3 end = target;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = easeOutExpo(t);

            EndPoint = Vector3.Lerp(start, end, t);
            yield return null;
        }
        EndPoint = end;
        _activeRoutine = null;
    }

    public void AttachTongue(Vector3 target)
    {
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
        }
        _mode = TongueMode.Attached;
        EndPoint = target;
        _activeRoutine = null;
    }

    private float easeOutExpo(float x)
    {
        if (Mathf.Abs(x - 1f) < Settings.FloatPrecisionThreshold)
        {
            return 1f;
        }
        else
        {
            return 1f - Mathf.Pow(2, -10 * x);
        }
    }
}
