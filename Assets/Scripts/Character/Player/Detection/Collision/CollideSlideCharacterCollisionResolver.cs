using System;
using UnityEngine;

public class CollideSlideCharacterCollisionResolver : MonoBehaviour
{
    [SerializeField] private int _maxCollideAndSlideDepth = 5;
    [SerializeField] private float _maxSlopeAngle = 55f; // Maximum slope angle to consider for sliding
    [SerializeField] private LayerMask _layer;
    [SerializeField] private GlobalPhysicsSettingsSO _settings;
    [SerializeField] private float _skinWidth = 0.02f; // Default skin width
    [SerializeField] private Vector3 _point1 = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 _point2 = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private float _radius = 0.5f;

    public event Action OnCollisionDetected;

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
        _radius -= _skinWidth; // Adjust radius to account for skin width
    }

    //TODO: Check if magnitude needs to be calculated before projection
    private Vector3 ProjectAndScale(Vector3 displacement, Vector3 normal)
    {
        float magnitude = displacement.magnitude;
        displacement = Vector3.ProjectOnPlane(displacement, normal).normalized;
        return displacement * magnitude;
    }

    // Source: Collide And Slide - *Actually Decent* Character Collision From Scratch, Improved Collision detection and Response
    // URL: https://www.youtube.com/watch?v=YR6Q7dUz2uk&t=522s, https://www.peroxide.dk/papers/collision/collision.pdf
    // Author: Poke Dev, Kasper Fauerby 
    // Notes: 
    public Vector3 ResolveCollideAndSlide(Vector3 displacement, int depth, bool gravityPass)
    {
        if (depth >= _maxCollideAndSlideDepth)
        {
            return Vector3.zero;
        }

        Vector3 origin = transform.position;
        float distance = displacement.magnitude;

        // Early exit for very small displacements
        if (distance <= 0.001f)
        {
            return displacement;
        }

        distance += _skinWidth;
        Vector3 direction = displacement.normalized;
        RaycastHit hit;

        if (Physics.CapsuleCast(origin + _point1, origin + _point2, _radius, direction, out hit, distance, _layer))
        {
            OnCollisionDetected?.Invoke();
            Vector3 snapToSurface = direction * Mathf.Max(0, hit.distance - _skinWidth);
            Vector3 leftover = displacement - snapToSurface;
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            if (snapToSurface.magnitude < _skinWidth)
            {
                // If the snap distance is less than the skin width, we can consider it a collision
                // and resolve it by returning zero displacement for the snap part
                snapToSurface = Vector3.zero;
            }

            if (angle < _maxSlopeAngle)
            {
                if (gravityPass)
                {
                    // If this is a gravity pass, we can just snap to the surface
                    return snapToSurface;
                }
                leftover = ProjectAndScale(leftover, hit.normal);
            }
            else // wall or steep slope
            {
                leftover = ProjectAndScale(leftover, hit.normal);
            }

            return snapToSurface + ResolveCollideAndSlide(leftover, depth + 1, gravityPass);
        }

        // If no collision was detected, return the original displacement
        return displacement;
    }
}
