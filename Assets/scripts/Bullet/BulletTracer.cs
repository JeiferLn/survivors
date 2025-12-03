using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    [Header("Tracer Settings")]
    public float speed = 300f;
    public float tracerLength = 0.5f;
    public float maxDistance = 100f;
    public float damage = 20f;

    [Header("Collision")]
    [SerializeField] private LayerMask hitLayers = ~0; // Todo por defecto
    [SerializeField] private float rayRadius = 0.05f;  // Grosor del "rayo"

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

        // ══════════ DETECCIÓN DE COLISIÓN CON RAYCAST ══════════
        if (CheckHit(currentPos, direction, moveDistance, out RaycastHit hit))
        {
            OnHitSomething(hit);
            return; // Salir, la bala fue destruida o procesada
        }

        // ══════════ MOVER LA BALA ══════════
        t.position = nextPos;
        traveled += moveDistance;

        // ══════════ ACTUALIZAR LINE RENDERER ══════════
        UpdateLineRenderer();

        // ══════════ DESTRUIR SI EXCEDE DISTANCIA ══════════
        if (traveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Verifica si hay algo entre la posición actual y la siguiente.
    /// </summary>
    private bool CheckHit(Vector3 origin, Vector3 dir, float distance, out RaycastHit hit)
    {
        // Debug visual
        if (showDebugRay)
        {
            Debug.DrawRay(origin, dir * distance, Color.red, 0.1f);
        }

        // Opción 1: Raycast simple (más rápido)
         return Physics.Raycast(origin, dir, out hit, distance, hitLayers);

        // Opción 2: SphereCast (más preciso, tiene "grosor")
        //return Physics.SphereCast(origin, rayRadius, dir, out hit, distance, hitLayers);
    }

    /// <summary>
    /// Procesa el impacto contra algo.
    /// </summary>
    private void OnHitSomething(RaycastHit hit)
    {
        // Mover la bala al punto exacto de impacto
        t.position = hit.point;
        
        // Actualizar visual antes de destruir
        UpdateLineRenderer();

        Debug.Log($"[BulletTracer] Hit: {hit.collider.name} at {hit.point}");

        // Intentar hacer daño
        if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage);
        }

        // También buscar en el padre (por si el collider está en un hijo)
        if (hit.collider.transform.parent != null)
        {
            if (hit.collider.transform.parent.TryGetComponent<IDamageable>(out var parentDamageable))
            {
                parentDamageable.TakeDamage(damage);
            }
        }

        // Destruir la bala
        Destroy(gameObject);
        
        // Opcional: Instanciar efecto de impacto
        // Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
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