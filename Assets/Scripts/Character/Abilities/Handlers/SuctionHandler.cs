using System.Collections.Generic;
using UnityEngine;

public class SuctionHandler : MonoBehaviour
{
    private Rigidbody _heldObject;
    private float _holdTimer;
    private SuctionThrowAbilitySO _activeAbility;
    private MoveActionSO _moveAction;
    private GameObject _tongue;
    private TongueController _tongueController;

    [Header("References")]
    public Transform suctionPoint;
    public LayerMask aimPlaneMask;
    public LineRenderer aimLine;

    [Tooltip("Layers treated as obstacles that block suction")]
    public LayerMask suctionObstructionMask;

    [Header("Throw Arc Visualization")]
    public LineRenderer throwArcRenderer;
    public int arcPoints = 30;
    public float arcTimeStep = 0.1f;

    private bool _hasReachedHoldPointOnce = false;
    private bool _isSuctioning = false;
    private HashSet<Rigidbody> _suctionedObjects = new HashSet<Rigidbody>();

    public bool IsHoldingObject => _heldObject != null;

    public void BeginSuction(SuctionThrowAbilitySO ability)
    {
        _activeAbility = ability;
        _isSuctioning = !IsHoldingObject;
    }

    private void Update()
    {
        if (IsHoldingObject)
        {
            _holdTimer += Time.deltaTime;

            Vector3 target = _tongueController.EndPoint;
            target.z = 0f;
            float distance = Vector3.Distance(_heldObject.position, target);

            if (_hasReachedHoldPointOnce && distance >= 1.5f)
            {
                DropHeldObject();
                return;
            }

            _heldObject.linearVelocity = Vector3.zero;
            _heldObject.useGravity = false;
            _heldObject.MovePosition(target);

            // Visualize the throw arc
            VisualizeThrowArc();
        }
        else
        {
            throwArcRenderer.enabled = false; // Hide arc when not holding

            if (_isSuctioning)
            {
                TrySuctionObject();
            }
        }
    }

    private void DropHeldObject()
    {
        if (_heldObject != null)
        {
            _heldObject.useGravity = true;
            _heldObject = null;
            _holdTimer = 0f;
            _hasReachedHoldPointOnce = false;
        }
    }

    private void TrySuctionObject()
    {
        Vector3 direction = _tongueController.Direction;

        HashSet<Rigidbody> currentFrameSuctioned = new HashSet<Rigidbody>();

        Collider[] colliders = Physics.OverlapSphere(transform.position, _activeAbility.SuctionRange);

        foreach (Collider collider in colliders)
        {
            if (collider.GetComponent<Rigidbody>() == null || collider.GetComponent<ThrowableObject>() == null)
            {
                continue; // Skip if no Rigidbody or ThrowableObject component
            }

            Rigidbody rb = collider.GetComponent<Rigidbody>();
            Vector3 objectDirection = (collider.transform.position - transform.position);
            objectDirection.z = 0f;
            objectDirection = objectDirection.normalized;

            float angle = Vector3.Angle(direction, objectDirection);
            float distance = Vector3.Distance(collider.transform.position, transform.position);

            if (angle < (_activeAbility.SuctionConeAngle / 2f))
            {
                Vector3 rayOrigin = suctionPoint.position;
                Vector3 rayDir = (collider.transform.position - rayOrigin).normalized;
                float rayDist = Vector3.Distance(rayOrigin, collider.transform.position);

                // Only proceed if there's no obstruction
                if (Physics.Raycast(rayOrigin, rayDir, rayDist, suctionObstructionMask))
                {
                    continue; // Something is blocking the suction path
                }

                float distanceMultiplier = Mathf.Clamp01(1f - (distance / _activeAbility.SuctionRange));
                float angleMultiplier = Mathf.Clamp01(1f - (angle / (_activeAbility.SuctionConeAngle / 2f)));

                Vector3 forceDirection = (transform.position - collider.transform.position).normalized;
                float totalForce = distanceMultiplier * angleMultiplier * _activeAbility.SuctionForce;

                rb.useGravity = false; // Disable gravity while suctioning
                rb.AddForce(forceDirection * totalForce, ForceMode.Force);

                currentFrameSuctioned.Add(rb);

                if (distance <= 2.0f && _heldObject == null)
                {
                    _isSuctioning = false;
                    _heldObject = rb;
                    _heldObject.linearVelocity = Vector3.zero;
                    _hasReachedHoldPointOnce = true;

                    return; // Exit after holding the first valid object
                }
            }
            else
            {
                // Only restore gravity if this object was previously being suctioned
                if (_suctionedObjects.Contains(rb))
                {
                    rb.useGravity = true;
                    rb.linearVelocity = Vector3.zero;
                }
            }
        }

        // Handle objects that are no longer in range (not detected by OverlapSphere)
        foreach (Rigidbody rb in _suctionedObjects)
        {
            if (!currentFrameSuctioned.Contains(rb))
            {
                rb.useGravity = true;
                rb.linearVelocity = Vector3.zero; // Reset velocity when suction ends
            }
        }

        _suctionedObjects = currentFrameSuctioned;
    }

    public void ThrowHeldObject()
    {
        if (_heldObject != null)
        {
            _hasReachedHoldPointOnce = false;

            Vector3 direction = _tongueController.Direction;

            _heldObject.useGravity = true;

            ThrowableObject throwableObject = _heldObject.GetComponent<ThrowableObject>();
            if (throwableObject == null)
            {
                return;
            }

            float maxVelocity = CalculateThrowVelocity(throwableObject.MaxVerticalHeight);
            float angle = Vector3.Angle(Vector3.up, direction);

            Vector3 horizontalDirection = new Vector3(Mathf.Sign(direction.x), 0f, 0f);

            Vector3 verticalVelocity = new Vector3(0f, maxVelocity * Mathf.Cos(angle * Mathf.Deg2Rad), 0f);
            Vector3 horizontalVelocity = horizontalDirection * (maxVelocity * Mathf.Sin(angle * Mathf.Deg2Rad));

            Vector3 finalVelocity = verticalVelocity + horizontalVelocity;

            float multiplier = Mathf.Clamp01(_holdTimer / throwableObject.MaxHoldTime);
            _heldObject.AddForce(finalVelocity * multiplier, ForceMode.VelocityChange);

            _heldObject = null;
            _holdTimer = 0f;
            _isSuctioning = false;
        }
        else if (_isSuctioning)
        {
            Debug.Log("Cancelling suction before object was held.");
            _isSuctioning = false;

            foreach (var rb in _suctionedObjects)
            {
                rb.useGravity = true;
                rb.linearVelocity = Vector3.zero;
            }

            _suctionedObjects.Clear();
        }
    }

    private float CalculateThrowVelocity(float maxHeight)
    {
        if (_heldObject == null)
        {
            return 0f;
        }

        float velocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * maxHeight);
        return velocity;
    }

    private void VisualizeThrowArc()
    {
        if (_heldObject == null || throwArcRenderer == null)
        {
            return;
        }

        ThrowableObject throwableObject = _heldObject.GetComponent<ThrowableObject>();
        if (throwableObject == null)
        {
            return;
        }

        float maxVelocity = CalculateThrowVelocity(throwableObject.MaxVerticalHeight);
        float multiplier = Mathf.Clamp01(_holdTimer / throwableObject.MaxHoldTime);
        Vector3 direction = _tongueController.Direction.normalized;

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

            // Raycast from previous point to current point
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

    private void OnDrawGizmos()
    {
        if (suctionPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(suctionPoint.position, 0.1f);
        }
    }

    private void Awake()
    {
        _tongue = transform.Find("Tongue").gameObject;

        if (_tongue == null)
        {
            Debug.LogError("SuctionHandler: Tongue GameObject not found. Please ensure it is attached as a child to this GameObject.");
        }
        else
        {
            _tongueController = _tongue.GetComponent<TongueController>();
            if (_tongueController == null)
            {
                Debug.LogError("SuctionHandler :: TongueController component not found on the Tongue GameObject.");
            }
        }

        _moveAction = GetComponent<PlayerController>().MoveAction;
    }
}
