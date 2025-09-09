using System.Collections.Generic;
using UnityEngine;

public class SuctionHandler : MonoBehaviour
{
    private Rigidbody _heldObject;
    private float _holdTimer;
    private SuctionThrowAbilitySO _activeAbility;
    private MoveActionSO _moveAction;

    [Header("Refs")]
    public Transform suctionPoint;
    public LayerMask suctionObstructionMask;

    [Header("Pickup")]
    public float maxRange = 8f;
    public float extendSpeed = 5f;
    public float retractSpeed = 5f;
    public float pickupRadius = 0.35f;
    public LayerMask pickupMask;

    [Header("Throw Arc Visualization")]
    public LineRenderer throwArcRenderer;
    public int arcPoints = 30;
    public float arcTimeStep = 0.1f;

    private GameObject _tongueGO;
    private TongueController _tongue;
    private bool _buttonHeld;
    private bool _isExtending;
    private bool _autoRetracting;
    private Vector3 _extendTarget;
    private bool _isRetracting = false;
    private bool _isReadyToThrow = false;

    // --- Home tip ---
    private Vector3 _homeTip;
    private bool _hasHomeTip;

    private Vector3 GetHomePos()
    {
        Vector3 p = _hasHomeTip ? _homeTip : suctionPoint.position;
        p.z = 0f;
        return p;
    }

    public bool IsHoldingObject => _heldObject != null;

    public void BeginSuction(SuctionThrowAbilitySO ability)
    {
        OnTonguePressed(ability);
    }

    public void EndSuctionOrDrop()
    {
        OnTongueReleased();
    }

    public void OnTonguePressed(SuctionThrowAbilitySO ability)
    {
        _activeAbility = ability;
        _buttonHeld = true;

        // Snapshot the current aim tip (line renderer end) as "home"
        _homeTip = _tongue.EndPoint; _homeTip.z = 0f;
        _hasHomeTip = true;

        // Compute extend ray target (blocked by walls)
        Vector3 start = suctionPoint.position;
        Vector3 dir = _tongue.Direction.normalized;

        float range = (_activeAbility != null && _activeAbility.SuctionRange > 0f)
            ? _activeAbility.SuctionRange
            : maxRange;

        _extendTarget = start + dir * range;
        if (Physics.Raycast(start, dir, out RaycastHit hit, range, suctionObstructionMask))
            _extendTarget = hit.point;

        float dist = Vector3.Distance(start, _extendTarget);
        float duration = dist / Mathf.Max(0.001f, extendSpeed);

        _tongue.ExtendTongue(_extendTarget, duration);
        _isExtending = true;
        _autoRetracting = false;
    }

    public void OnTongueReleased()
    {
        _buttonHeld = false;

        // If extending, retract immediately
        if (_isExtending)
        {
            RetractNow();
            return;
        }

        if (_heldObject != null)
        {
            if (_isReadyToThrow)
            {
                // In throw-aim: release should throw
                ThrowHeldObject();

                // Reset visuals/state
                if (throwArcRenderer != null) throwArcRenderer.enabled = false;
                if (!_isExtending && !_isRetracting) _tongue.AimTongue();
            }
            else
            {
                // Still retracting: release just drops
                DropHeldObject();

                if (!IsTipNear(GetHomePos()))
                    RetractNow();
            }
        }
    }

    private void Update()
    {
        // Maintain carried object at tongue tip
        if (_heldObject != null)
        {
            _holdTimer += Time.deltaTime;

            Vector3 tip = _tongue.EndPoint; tip.z = 0f;
            
            _heldObject.linearVelocity = Vector3.zero;
            _heldObject.useGravity = false;
            _heldObject.MovePosition(tip);

            VisualizeThrowArc();
            _isReadyToThrow = true;

            float d = Vector3.Distance(_heldObject.position, tip);
            if (d >= 1.5f) // safety
                DropHeldObject();
        }
        else if (throwArcRenderer != null)
        {
            throwArcRenderer.enabled = false;
        }

        // While extending, try to catch object near the tip
        if (_isExtending)
        {
            TryCatchWhileExtending();

            if (IsTipNear(_extendTarget) && !_buttonHeld)
                RetractNow();
        }

        // When auto-retracting and reached player, clear flag
        if (_autoRetracting && IsTipNear(GetHomePos()))
            _autoRetracting = false;

        // when retracting and tongue tip is back at suction point, reset tongue back to Aim mode
        if (_isRetracting && IsTipNear(GetHomePos()))
        {
            _tongue.AimTongue();
            _isRetracting = false;
        }
    }


    private void TryCatchWhileExtending()
    {
        Vector3 tip = _tongue.EndPoint; tip.z = 0f;
        Collider[] hits = Physics.OverlapSphere(tip, pickupRadius, pickupMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            var rb = hits[i].attachedRigidbody;
            if (rb == null) continue;
            if (rb.GetComponent<ThrowableObject>() == null) continue;

            // Attach & auto-retract with the object
            _heldObject = rb;
            _heldObject.linearVelocity = Vector3.zero;
            _heldObject.useGravity = false;

            _tongue.AttachTongue(_heldObject.position);

            Vector3 target = GetHomePos();
            float distBack = Vector3.Distance(_tongue.EndPoint, target);
            float durBack = distBack / Mathf.Max(0.001f, retractSpeed);
            _tongue.ExtendTongue(target, durBack);

            _autoRetracting = true;
            _isExtending = false;
            _isRetracting = true;
            return;
        }
    }

    private void RetractNow()
    {
        Vector3 target = GetHomePos();
        float distBack = Vector3.Distance(_tongue.EndPoint, target);
        float durBack = distBack / Mathf.Max(0.001f, retractSpeed);

        _tongue.ExtendTongue(target, durBack);
        _isExtending = false;
        _isRetracting = true;
    }

    private bool IsTipNear(Vector3 worldPos, float eps = 0.03f)
    {
        Vector3 a = _tongue.EndPoint; a.z = 0f;
        Vector3 b = worldPos; b.z = 0f;
        return (a - b).sqrMagnitude <= eps * eps;
    }

    private void DropHeldObject()
    {
        if (_heldObject == null) return;
        _heldObject.useGravity = true;
        _heldObject.linearVelocity = Vector3.zero;
        _heldObject = null;
        _holdTimer = 0f;
    }

    // ===== Throw logic =====
    public void ThrowHeldObject()
    {
        if (_heldObject == null) return;

        Vector3 direction = _tongue.Direction;

        _heldObject.useGravity = true;

        ThrowableObject throwableObject = _heldObject.GetComponent<ThrowableObject>();
        if (throwableObject == null)
        {
            _heldObject = null;
            _holdTimer = 0f;
            return;
        }

        float maxVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * throwableObject.MaxVerticalHeight);
        float angle = Vector3.Angle(Vector3.up, direction);

        Vector3 horizontalDirection = new Vector3(Mathf.Sign(direction.x), 0f, 0f);

        Vector3 verticalVelocity = new Vector3(0f, maxVelocity * Mathf.Cos(angle * Mathf.Deg2Rad), 0f);
        Vector3 horizontalVelocity = horizontalDirection * (maxVelocity * Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector3 finalVelocity = verticalVelocity + horizontalVelocity;

        float multiplier = Mathf.Clamp01(_holdTimer / throwableObject.MaxHoldTime);
        _heldObject.AddForce(finalVelocity * multiplier, ForceMode.VelocityChange);

        _heldObject = null;
        _holdTimer = 0f;
    }

    private void VisualizeThrowArc()
    {
        if (_heldObject == null || throwArcRenderer == null) return;

        ThrowableObject throwableObject = _heldObject.GetComponent<ThrowableObject>();
        if (throwableObject == null) return;

        float maxVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * throwableObject.MaxVerticalHeight);
        float multiplier = Mathf.Clamp01(_holdTimer / throwableObject.MaxHoldTime);
        Vector3 direction = _tongue.Direction.normalized;

        float angle = Vector3.Angle(Vector3.up, direction);
        Vector3 horizontalDir = new Vector3(Mathf.Sign(direction.x), 0f, 0f);

        Vector3 v0 = new Vector3(
            horizontalDir.x * maxVelocity * Mathf.Sin(angle * Mathf.Deg2Rad),
            maxVelocity * Mathf.Cos(angle * Mathf.Deg2Rad),
            0f
        ) * multiplier;

        Vector3 startPos = _heldObject.position;
        throwArcRenderer.positionCount = arcPoints;

        for (int i = 0; i < arcPoints; i++)
        {
            float t = i * arcTimeStep;
            Vector3 pos = startPos + v0 * t + 0.5f * Physics.gravity * t * t;

            if (i > 0)
            {
                Vector3 prevPos = throwArcRenderer.GetPosition(i - 1);
                Vector3 dir = pos - prevPos;
                float dist = dir.magnitude;

                if (Physics.Raycast(prevPos, dir.normalized, dist, suctionObstructionMask))
                {
                    throwArcRenderer.positionCount = i;
                    break;
                }
            }
            throwArcRenderer.SetPosition(i, pos);
        }
        throwArcRenderer.enabled = true;
    }

    private void Awake()
    {
        _tongueGO = transform.Find("Tongue")?.gameObject;
        if (_tongueGO == null)
        {
            Debug.LogError("SuctionHandler: Tongue GameObject not found. Attach as a child.");
        }
        else
        {
            _tongue = _tongueGO.GetComponent<TongueController>();
            if (_tongue == null)
                Debug.LogError("SuctionHandler :: TongueController missing on Tongue.");
        }

        _moveAction = GetComponent<PlayerController>().MoveAction;
    }

    private void OnDrawGizmosSelected()
    {
        if (_tongue != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_tongue.EndPoint, pickupRadius);
        }
    }
}
