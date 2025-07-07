using UnityEngine;

/// <summary>
/// Resolves 3D collisions using BoxCast for both horizontal (XZ) and vertical (Y) movement.
/// Attach to character root object with appropriate collider size.
/// </summary>
public class NaiveCharacterCollisionResolver : MonoBehaviour
{
    [Header("Common Settings")]
    [SerializeField] private LayerMask _layer;
    [SerializeField] private float _minCastDistance = 0.02f;
    [SerializeField] private Vector3 _boxHalfExtents = new Vector3(0.5f, 1f, 0.5f);
    [SerializeField] private GlobalPhysicsSettingsSO _settings;
    [SerializeField] private float _skinWidth = 0.02f;

    [Header("Debug Settings")]
    [SerializeField] private Vector3 _horizontalVelocityVec;
    [SerializeField] private Vector3 _verticalVelocityVec;

    private void Awake()
    {
        if (_settings != null)
        {
            _skinWidth = _settings.SkinWidth;
        }
        else
        {
            Debug.LogWarning($"GlobalPhysicsSettings not assigned! Using default skin width of {_skinWidth}");
        }
        _boxHalfExtents -= new Vector3(_skinWidth, _skinWidth, _skinWidth); // Adjust box extents to account for skin width
    }
    public Vector3 ResolveHorizontal(Vector3 velocity, float deltaTime)
    {
        Vector3 move = new Vector3(velocity.x, 0f, velocity.z) * deltaTime;

        Vector3 origin = transform.position;
        Vector3 direction = move.normalized;
        float distance = Mathf.Max(_minCastDistance, move.magnitude + _skinWidth);

        if (Physics.BoxCast(origin, _boxHalfExtents, direction, out RaycastHit hit, Quaternion.identity, distance, _layer))
        {
            float safeDistance = Mathf.Max(0f, hit.distance - _skinWidth);
            Vector3 adjustedVel = direction * (safeDistance / deltaTime);
            return new Vector3(adjustedVel.x, velocity.y, adjustedVel.z); // keep vertical velocity untouched here
        }

        return velocity;
    }

    public Vector3 ResolveVertical(Vector3 velocity, float deltaTime)
    {
        float verticalMove = velocity.y * deltaTime;

        Vector3 origin = transform.position;
        Vector3 direction = Vector3.up * Mathf.Sign(verticalMove);
        float distance = Mathf.Max(_minCastDistance, Mathf.Abs(verticalMove));

        if (Physics.BoxCast(origin, _boxHalfExtents, direction, out RaycastHit hit, Quaternion.identity, distance, _layer))
        {
            float safeDistance = Mathf.Max(0f, hit.distance - _skinWidth);
            float safeVelocity = (safeDistance / deltaTime) * Mathf.Sign(verticalMove);
            return new Vector3(velocity.x, safeVelocity, velocity.z); // keep horizontal velocity untouched here
        }

        return velocity;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 origin = transform.position;
        Gizmos.DrawWireCube(origin, _boxHalfExtents * 2f);
    }
}
