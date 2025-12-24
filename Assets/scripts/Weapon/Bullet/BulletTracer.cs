using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BulletTracer : MonoBehaviour
{
    private LineRenderer line;
    private Vector3 direction;
    private IDamageable damageable;
    private float speed;
    private float maxDistance;
    private float traveledDistance;
    private float bulletLength;
    private float bulletDamage;

    public void Init(Vector3 dir, float bulletSpeed, float maxDist, float length, float damage)
    {
        direction = dir.normalized;
        speed = bulletSpeed;
        maxDistance = maxDist;
        bulletLength = length;
        bulletDamage = damage;

        line = GetComponent<LineRenderer>();
        line.positionCount = 2;

        Vector3 start = transform.position;
        line.SetPosition(0, start);
        line.SetPosition(1, start);
    }

    void Update()
    {
        float step = speed * Time.deltaTime;

        Vector3 head = line.GetPosition(1);
        Vector3 newHead = head + direction * step;

        if (Physics.Raycast(head, direction, out RaycastHit hit, step))
        {
            line.SetPosition(1, hit.point);
            line.SetPosition(0, hit.point - direction * bulletLength);

            damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable == null)
                return;

            damageable.TakeDamage(bulletDamage);
            Destroy(gameObject);
        }

        line.SetPosition(1, newHead);
        line.SetPosition(0, newHead - direction * bulletLength);

        traveledDistance += step;

        if (traveledDistance >= maxDistance)
            Destroy(gameObject);
    }
}
