using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    [Header("Tracer Settings")]
    public float speed = 50f;
    public float tracerLength = 0.5f;
    public float maxDistance = 20f;
    public float damage = 20f;

    private Vector3 direction;
    private LineRenderer lr;
    private float traveled = 0f;
    private Transform t;

    void Start()
    {
        t = transform;
        lr = GetComponent<LineRenderer>();
        direction = t.forward;

        if (lr != null)
        {
            lr.positionCount = 2;
        }
    }

    void Update()
    {
        float move = speed * Time.deltaTime;

        t.position += direction * move;
        traveled += move;

        if (lr != null)
        {
            Vector3 head = t.position;
            Vector3 tail = head - direction * tracerLength;

            lr.SetPosition(0, tail);
            lr.SetPosition(1, head);
        }

        if (traveled >= maxDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
