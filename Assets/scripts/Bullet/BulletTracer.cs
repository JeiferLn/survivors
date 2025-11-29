using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    // ---------------- SETTINGS ----------------
    [Header("Tracer Settings")]
    public float speed = 50f;
    public float tracerLength = 0.5f;
    public float maxDistance = 20f;
    public float damage = 20f;

    // ---------------- COMPONENTS ----------------
    private Vector3 direction;
    private LineRenderer lr;
    private Transform t;

    // ---------------- STATE ----------------
    private float traveled = 0f;

    // ---------------- START ----------------
    void Start()
    {
        t = transform;

        direction = t.forward.normalized;

        lr = GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.positionCount = 2;
        }
    }

    // ---------------- UPDATE ----------------
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
        {
            Destroy(gameObject);
        }
    }

    // ---------------- TRIGGER ENTER ----------------
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IDamageable>(out var damageable))
            return;

        damageable.TakeDamage(damage);
        Destroy(gameObject);
    }
}
