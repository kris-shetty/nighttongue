using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [SerializeField] private float _checkLength = 0.5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Vector3 _verticalOffset = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private Vector3 _boxHalfExtents = new Vector3(0.4f, 0.1f, 0.4f);

    public bool IsGrounded { get; private set; }
    public bool JustLanded { get; private set; }
    public bool WasGrounded;

    public void Refresh()
    {
        WasGrounded = IsGrounded;

        // Calculate the starting position for boxcast
        Vector3 castPosition = transform.position + _verticalOffset;

        // Perform single boxcast downward
        IsGrounded = Physics.BoxCast(castPosition, _boxHalfExtents, Vector3.down,
                                   out RaycastHit hit, Quaternion.identity, _checkLength, _groundLayer);

        JustLanded = IsGrounded && !WasGrounded;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Vector3 castPosition = transform.position + _verticalOffset;

        // Draw the box at the end position (cast distance)
        Gizmos.DrawWireCube(castPosition + Vector3.down * _checkLength, _boxHalfExtents * 2f);
    }
}