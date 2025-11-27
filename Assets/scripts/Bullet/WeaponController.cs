using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    // ------------- LINE RENDERER -------------
    [Header("Laser")]
    private LineRenderer lineRenderer;

    [SerializeField]
    private float laserDistance = 10f;

    [SerializeField]
    private Vector3 laserOffset = new Vector3(0f, -0.1f, 0f);

    [Header("Recoil Settings")]
    [SerializeField]
    private float recoilAmount = 1f;

    [SerializeField]
    private float recoilReturnSpeed = 10f;

    private Vector3 recoilOffset = Vector3.zero;

    // ------------- MUZZLE FIRE -------------
    [Header("Muzzle Flash")]
    [SerializeField]
    private GameObject muzzleFlash;

    [SerializeField]
    private float flashDuration = 0.05f;

    private float flashTimer = 0f;

    // ------------- INPUT VARIABLES -------------
    private bool isAiming = false;

    // ------------- INPUT ACTIONS -------------
    public void OnAim(InputAction.CallbackContext ctx)
    {
        isAiming = ctx.ReadValueAsButton();
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (isAiming)
            {
                ShowMuzzleFlash();
                ApplyRecoilKick();
            }
        }
    }

    // ------------- START -------------
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }

    // ------------- UPDATE -------------
    void Update()
    {
        HandleLaser();
        HandleMuzzleFlash();

        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
    }

    // ------------- LASER SYSTEM ----------------
    private void HandleLaser()
    {
        if (lineRenderer == null)
            return;

        if (!isAiming)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;

        Vector3 start = transform.position + transform.TransformDirection(laserOffset);

        Vector3 dir = transform.forward;
        Vector3 end = start + dir * laserDistance;

        if (Physics.Raycast(start, dir, out RaycastHit hit, laserDistance))
        {
            end = hit.point;
        }

        // Aplicar retroceso visual
        end += recoilOffset;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    // ------------- RECOIL SYSTEM -------------
    private void ApplyRecoilKick()
    {
        recoilOffset = new Vector3(
            Random.Range(-recoilAmount, recoilAmount),
            Random.Range(0f, recoilAmount),
            0f
        );
    }

    // ------------- MUZZLE FLASH SYSTEM -------------
    private void HandleMuzzleFlash()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0f)
                muzzleFlash.SetActive(false);
        }
    }

    private void ShowMuzzleFlash()
    {
        if (muzzleFlash == null)
            return;

        muzzleFlash.SetActive(true);
        flashTimer = flashDuration;
    }
}
