using UnityEngine;

public class Character : MonoBehaviour
{
    private Vector3 _velocity = Vector3.zero;

    private Vector3 _acceleration = Vector3.zero;

    private const float AccelerationMagnitude = 2;

    private const float MaxVelocityMagnitude = 5;

    private const float DamagePerSecond = 50;

    private const float DamageRange = 10;

    Collider2D[] nearbyColliders;

    private void Start()
    {
        int nbCircles = FindObjectOfType<GridShape>().config.nbCircles;
        nearbyColliders = new Collider2D[nbCircles];
    }

    private void Update()
    {
        Move();
        FindCollisions();
        DamageNearbyShapes();
        UpdateAcceleration();
        ResetCollisions();
    }

    private void FindCollisions()
    {
        Physics2D.OverlapCircleNonAlloc(transform.position, DamageRange, nearbyColliders);

    }
    private void ResetCollisions()
    {
        for (int i = 0; i < nearbyColliders.Length; i++)
        {
            if (nearbyColliders[i] == null)
            {
                break;
            }
            nearbyColliders[i] = null;
        }
    }

    private void Move()
    {
        _velocity += _acceleration * Time.deltaTime;
        if (_velocity.magnitude > MaxVelocityMagnitude)
        {
            _velocity = _velocity.normalized * MaxVelocityMagnitude;
        }
        transform.position += _velocity * Time.deltaTime;
    }

    private void UpdateAcceleration()
    {
        var direction = Vector3.zero;

        for(int i = 0; i < nearbyColliders.Length; i++)
        {
            if(nearbyColliders[i] == null)
            {
                break;
            }
            if (nearbyColliders[i].TryGetComponent<Circle>(out var circle))
            {
                direction += (circle.transform.position - transform.position) * circle.Health;
            }
        }
        _acceleration = direction.normalized * AccelerationMagnitude;
    }

    private void DamageNearbyShapes()
    {
        // Si aucun cercle proche, on retourne a (0,0,0)
        if (nearbyColliders.Length == 0 || nearbyColliders[0] == null)
        {
            transform.position = Vector3.zero;
        }

        foreach(var nearbyCollider in nearbyColliders)
        {
            if(nearbyCollider == null)
            {
                break;
            }
            if (nearbyCollider.TryGetComponent<Circle>(out var circle))
            {
                circle.ReceiveHp(-DamagePerSecond * Time.deltaTime);
            }
        }
    }
}
