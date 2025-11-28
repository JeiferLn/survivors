using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    [Header("Tracer Settings")]
    public float speed = 50f;
    public float tracerLength = 0.5f;
    public float maxDistance = 20f;

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
            Vector3 tail = t.position - direction * tracerLength;
            lr.SetPosition(0, tail);
            lr.SetPosition(1, t.position);
        }
    }

    void Update()
    {
        float move = speed * Time.deltaTime;
        t.position += direction * move;
        traveled += move;

        if (lr != null)
        {
            Vector3 tail = t.position - direction * tracerLength;
            lr.SetPosition(0, tail);
            lr.SetPosition(1, t.position);
        }

        if (traveled >= maxDistance)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
