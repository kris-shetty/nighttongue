using UnityEngine;
using UnityEngine.InputSystem;

public class SuctionHandler : MonoBehaviour
{
    private Rigidbody heldObject;
    private float holdTimer;
    private SuctionThrowAbilitySO activeAbility;

    [Header("References")]
    public Transform suctionPoint;
    public LayerMask aimPlaneMask;
    public LineRenderer aimLine;

    private Vector3 currentHoldPoint;
    private bool isHeldTightly = false;
    private bool hasReachedHoldPointOnce = false;

    public bool IsHoldingObject => heldObject != null;

    public void ToggleSuction(SuctionThrowAbilitySO ability)
    {
        activeAbility = ability;

        if (IsHoldingObject)
        {
            ThrowHeldObject();
        }
        else
        {
            TrySuctionObject();
        }
    }

    void Update()
    {
        if (IsHoldingObject)
        {
            holdTimer += Time.deltaTime;

            Vector3 target = currentHoldPoint;
            target.z = 0f;
            float distance = Vector3.Distance(heldObject.position, target);

            // Drop if object was once close but now far again
            if (hasReachedHoldPointOnce && distance > 1.5f)
            {
                Debug.Log("Object Dropped");
                DropHeldObject();
                return;
            }

            if (distance > 0.15f)
            {
                isHeldTightly = false;
                Vector3 direction = (target - heldObject.position).normalized;
                heldObject.AddForce(direction * activeAbility.suctionForce, ForceMode.Force);
            }
            else
            {
                // Snap and stick to hold point
                if (!isHeldTightly)
                {
                    isHeldTightly = true;
                    hasReachedHoldPointOnce = true;
                    heldObject.linearVelocity = Vector3.zero;
                    heldObject.useGravity = false;
                }

                heldObject.MovePosition(target);
            }
        }

        UpdateAimLine();
    }

    void DropHeldObject()
    {
        if (heldObject != null)
        {
            heldObject.useGravity = true;
            heldObject = null;
            holdTimer = 0f;
            isHeldTightly = false;
            hasReachedHoldPointOnce = false;
        }
    }

    void TrySuctionObject()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 direction = (mouseWorldPos - transform.position);
        direction.z = 0f;
        direction = direction.normalized;

        Ray ray = new Ray(transform.position, direction);
        Debug.DrawRay(transform.position, direction * activeAbility.suctionRange, Color.cyan, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, activeAbility.suctionRange))
        {
            if (hit.rigidbody != null && hit.rigidbody.mass <= 15f)
            {
                heldObject = hit.rigidbody;
                heldObject.useGravity = false;
                heldObject.linearVelocity = Vector3.zero;
                isHeldTightly = false;
                hasReachedHoldPointOnce = false;
                Debug.Log("Object held: " + heldObject.name);
            }
        }
    }

    void ThrowHeldObject()
    {
        if (heldObject != null)
        {
            isHeldTightly = false;
            hasReachedHoldPointOnce = false;

            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 direction = (mouseWorldPos - transform.position);
            direction.z = 0f;
            direction = direction.normalized;

            float force = Mathf.Clamp01(holdTimer / activeAbility.maxHoldTime);
            heldObject.useGravity = true;
            heldObject.AddForce(direction * force * activeAbility.throwForceMultiplier, ForceMode.Impulse);

            heldObject = null;
            holdTimer = 0;
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimPlaneMask))
        {
            return hit.point;
        }
        return transform.position;
    }

    void UpdateAimLine()
    {
        if (aimLine != null)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 direction = (mouseWorldPos - transform.position);
            direction.z = 0f;
            direction = direction.normalized;

            float lineLength = 2f;
            Vector3 endPoint = transform.position + direction * lineLength;

            suctionPoint.position = transform.position;
            currentHoldPoint = endPoint;

            aimLine.SetPosition(0, transform.position);
            aimLine.SetPosition(1, endPoint);
            aimLine.startWidth = 0.5f;
            aimLine.endWidth = 0.0f;
        }
    }

    private void OnDrawGizmos()
    {
        if (suctionPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(suctionPoint.position, 0.1f);
        }
    }
}
