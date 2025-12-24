using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BulletTracer : MonoBehaviour
{
    private LineRenderer line;
    private Vector3 direction;
    private float speed;
    private float maxDistance;
    private float traveledDistance;
    private float bulletLength;

    public void Init(Vector3 dir, float bulletSpeed, float maxDist, float length)
    {
        direction = dir.normalized;
        speed = bulletSpeed;
        maxDistance = maxDist;
        bulletLength = length;

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

            Debug.Log("Impactó con: " + hit.collider.name);
            Destroy(gameObject, 0.02f);
            return;
        }

        line.SetPosition(1, newHead);
        line.SetPosition(0, newHead - direction * bulletLength);

        traveledDistance += step;

        if (traveledDistance >= maxDistance)
            Destroy(gameObject);
    }
}
