using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    [Header("Tracer Settings")]
    public float speed = 300f;
    public float tracerLength = 0.5f;
    public float maxDistance = 100f;
    public float damage = 20f;

    [Header("Collision")]
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField] private float rayRadius = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool showDebugRay = false;

    private Vector3 direction;
    private LineRenderer lr;
    private Transform t;
    private float traveled;

    private void Start()
    {
        t = transform;
        direction = t.forward.normalized;

        lr = GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.positionCount = 2;
        }
    }

    private void Update()
    {
        float moveDistance = speed * Time.deltaTime;

        Vector3 currentPos = t.position;
        Vector3 nextPos = currentPos + direction * moveDistance;

        if (CheckHit(currentPos, direction, moveDistance, out RaycastHit hit))
        {
            OnHitSomething(hit);
            return;
        }

        t.position = nextPos;
        traveled += moveDistance;

        UpdateLineRenderer();

        if (traveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private bool CheckHit(Vector3 origin, Vector3 dir, float distance, out RaycastHit hit)
    {
        if (showDebugRay)
        {
            Debug.DrawRay(origin, dir * distance, Color.red, 0.1f);
        }

        return Physics.Raycast(origin, dir, out hit, distance, hitLayers);
    }

    private void OnHitSomething(RaycastHit hit)
    {
        t.position = hit.point;

        UpdateLineRenderer();

        Debug.Log($"[BulletTracer] Hit: {hit.collider.name} at {hit.point}");

        if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }

        if (hit.collider.transform.parent != null)
        {
            if (hit.collider.transform.parent.TryGetComponent<IDamageable>(out var parentDamageable))
            {
                parentDamageable.TakeDamage(damage);
            }
        }

        Destroy(gameObject);

    }

    private void UpdateLineRenderer()
    {
        if (lr == null) return;

        Vector3 head = t.position;
        Vector3 tail = head - direction * tracerLength;

        lr.SetPosition(0, tail);
        lr.SetPosition(1, head);
    }
}