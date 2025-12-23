using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BulletTracer : MonoBehaviour
{
    private LineRenderer line;
    private Vector3 direction;
    private float speed;
    private float maxDistance;
    private float traveledDistance;

    public void Init(Vector3 dir, float bulletSpeed, float maxDist)
    {
        direction = dir.normalized;
        speed = bulletSpeed;
        maxDistance = maxDist;

        line = GetComponent<LineRenderer>();
        line.positionCount = 2;

        Vector3 start = transform.position;
        line.SetPosition(0, start);
        line.SetPosition(1, start);
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        Vector3 start = line.GetPosition(1);
        Vector3 end = start + direction * step;

        if (Physics.Raycast(start, direction, out RaycastHit hit, step))
        {
            line.SetPosition(1, hit.point);
            Debug.Log("Impactó con: " + hit.collider.name);
            Destroy(gameObject, 0.02f);
            return;
        }

        line.SetPosition(1, end);
        traveledDistance += step;

        if (traveledDistance >= maxDistance)
            Destroy(gameObject);
    }
}
