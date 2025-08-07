using UnityEngine;
using UnityEngine.InputSystem;

public class TongueController : MonoBehaviour
{
    [SerializeField] private LayerMask _aimPlaneMask;

    private LineRenderer _tongue;
    private GameObject _player;

    public float TongueLength = 2f;
    public Vector3 EndPoint;
    public Vector3 Direction;

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

    private void UpdateTongueAim()
    {
        UpdateAim();

        _tongue.SetPosition(0, _player.transform.position);
        _tongue.SetPosition(1, EndPoint);
        _tongue.startWidth = 0.5f;
        _tongue.endWidth = 0.0f;
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
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateTongueAim();
    }
}
