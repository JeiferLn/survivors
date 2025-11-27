using UnityEngine;
using UnityEngine.InputSystem;

public class LaserController : MonoBehaviour
{
    // ------------- LINE RENDERER -------------
    [Header("Laser")]
    private LineRenderer lineRenderer;

    [SerializeField]
    private float laserDistance = 10f;

    // ------------- INPUT VARIABLES -------------
    private bool isAiming = false;

    // ------------- INPUT ACTIONS -------------
    public void OnAim(InputAction.CallbackContext ctx)
    {
        isAiming = ctx.ReadValueAsButton();
    }

    // ------------- START -------------
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    // ------------- UPDATE -------------
    void Update()
    {
        HandleLaser();
    }

    // ------------- LASER SYSTEM -------------
    private void HandleLaser()
    {
        if (lineRenderer == null) return;

        if (!isAiming)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;

        Vector3 start = transform.position;

        Vector3 dir = transform.forward;
        Vector3 end = start + dir * laserDistance;

        if (Physics.Raycast(start, dir, out RaycastHit hit, laserDistance))
        {
            end = hit.point;
        }

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
}
