using UnityEngine;

public class PushOffOverhang : MonoBehaviour
{
    [SerializeField] private float _innerOffset = 0.25f;
    [SerializeField] private float _outerOffset = 0.5f;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Vector3 _castOffset = new Vector3(0f, 0.1f, 0f);
    [SerializeField] private float _pushDistance = 0.25f; // How far to push the player
    [SerializeField] private GlobalPhysicsSettings _settings;
    [SerializeField] private float _skinWidth = 0.02f; // Default skin width

    [Header("Debug Info")]
    [SerializeField] private bool _leftOuter = false;
    [SerializeField] private bool _leftInner = false;
    [SerializeField] private bool _rightInner = false;
    [SerializeField] private bool _rightOuter = false;

    private void Awake()
    {
        if (_settings != null)
        {
            _skinWidth = _settings.skinWidth;
        }
        else
        {
            Debug.LogWarning($"GlobalPhysicsSettings not assigned! Using default skin width of {_skinWidth}");
        }
        _castOffset -= new Vector3(_skinWidth, _skinWidth, _skinWidth); // Adjust cast offset to account for skin width
    }

    public void TryPushOffLedge(float velocity, float deltaTime)
    {
        // Only check when moving upward (jumping into ceiling)
        float verticalMove = velocity * deltaTime;
        if (verticalMove <= 0f) return;

        Vector3 origin = transform.position + _castOffset;
        float distance = Mathf.Abs(verticalMove) + _skinWidth;

        // Cast upward from four positions: left outer, left inner, right inner, right outer
        _leftOuter = Physics.Raycast(origin + new Vector3(-_outerOffset, 0f, 0f), Vector3.up, distance, _layerMask);
        _leftInner = Physics.Raycast(origin + new Vector3(-_innerOffset, 0f, 0f), Vector3.up, distance, _layerMask);
        _rightInner = Physics.Raycast(origin + new Vector3(_innerOffset, 0f, 0f), Vector3.up, distance, _layerMask);
        _rightOuter = Physics.Raycast(origin + new Vector3(_outerOffset, 0f, 0f), Vector3.up, distance, _layerMask);

        // Push left if right side hits ceiling but left side is clear
        if ((_rightOuter && (!_rightInner && !_leftOuter && !_leftInner)))
        {
            transform.position += new Vector3(-_pushDistance, 0f, 0f);

        }
        // Push right if left side hits ceiling but right side is clear
        else if ((_leftOuter && (!_leftInner && !_rightOuter && !_rightInner)))
        {
            transform.position += new Vector3(_pushDistance, 0f, 0f);
        }
        else
        {
            // No push needed, reset all flags
            _leftOuter = false;
            _leftInner = false;
            _rightInner = false;
            _rightOuter = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + _castOffset;

        // Draw the four raycast positions
        Gizmos.color = _leftOuter ? Color.red : Color.green;
        Gizmos.DrawWireSphere(origin + new Vector3(-_outerOffset, 0f, 0f), 0.05f);

        Gizmos.color = _leftInner ? Color.red : Color.green;
        Gizmos.DrawWireSphere(origin + new Vector3(-_innerOffset, 0f, 0f), 0.05f);

        Gizmos.color = _rightInner ? Color.red : Color.green;
        Gizmos.DrawWireSphere(origin + new Vector3(_innerOffset, 0f, 0f), 0.05f);

        Gizmos.color = _rightOuter ? Color.red : Color.green;
        Gizmos.DrawWireSphere(origin + new Vector3(_outerOffset, 0f, 0f), 0.05f);
    }
}