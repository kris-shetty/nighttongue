using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LockZPosition : MonoBehaviour
{
    public float fixedZ = 0f;

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.z = fixedZ;
        transform.position = pos;
    }
}
