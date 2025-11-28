using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    // ------------- LASER VARIABLES -------------
    [Header("Laser")]
    private LineRenderer lineRenderer;

    // ------------- LASER SETTINGS -------------
    [SerializeField] private float laserDistance = 20f;
    [SerializeField] private Vector3 laserOffset = new Vector3(0f, -0.1f, 0f);

    // ------------- RECOIL VARIABLES -------------
    [Header("Recoil Settings")]
    [SerializeField] private float recoilAmount = 1f;
    [SerializeField] private float recoilReturnSpeed = 10f;

    private Vector3 recoilOffset = Vector3.zero;

    // ------------- Muzzle Flash VARIABLES -------------
    [Header("Muzzle Flash")]
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private float flashDuration = 0.05f;

    private float flashTimer = 0f;

    // ------------- Bullet Tracer VARIABLES -------------
    [Header("Bullet Tracer")]
    [SerializeField] private GameObject bulletLinePrefab;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float bulletLength = 0.5f;
    [SerializeField] private float bulletMaxDistance = 20f;

    // ------------- Aiming VARIABLES -------------
    private bool isAiming = false;

    private Vector3 lastLaserEnd;

    // ------------- Player VARIABLES -------------
    [Header("Player")]
    [SerializeField] private PlayerController player;

    private Transform t;

    // ------------- INITIALIZATION -------------
    void Start()
    {
        t = transform;

        // SI NO SE ASIGNÓ MANUALMENTE, BUSCAR EN EL PADRE
        if (player == null)
            player = GetComponentInParent<PlayerController>();

        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }

    // ------------- AIM INPUT -------------
    public void OnAim(InputAction.CallbackContext ctx)
    {
        isAiming = ctx.ReadValueAsButton();

        if (player != null)
            player.SetAiming(isAiming);
    }

    // ------------- SHOOT INPUT -------------
    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && isAiming)
        {
            ShowMuzzleFlash();
            ApplyRecoilKick();
            ShootBullet();
        }
    }

    // ------------- UPDATE -------------
    void Update()
    {
        HandleLaser();
        HandleMuzzleFlash();
        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
    }

    // ------------- HANDLE LASER -------------
    private void HandleLaser()
    {
        if (lineRenderer == null) return;

        if (!isAiming)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;

        Vector3 muzzlePos = t.position + t.TransformDirection(laserOffset);
        Vector3 dir = t.forward;

        Vector3 end = muzzlePos + dir * laserDistance;

        if (Physics.Raycast(muzzlePos, dir, out RaycastHit hit, laserDistance))
            end = hit.point;

        lastLaserEnd = end;

        lineRenderer.SetPosition(0, muzzlePos);
        lineRenderer.SetPosition(1, end);
    }

    // ------------- APPLY RECOIL KICK -------------
    private void ApplyRecoilKick()
    {
        recoilOffset = new Vector3(
            Random.Range(-recoilAmount, recoilAmount),
            Random.Range(0f, recoilAmount),
            0f
        );
    }

    // ------------- HANDLE Muzzle Flash -------------
    private void HandleMuzzleFlash()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0f && muzzleFlash != null)
                muzzleFlash.SetActive(false);
        }
    }

    // ------------- SHOW Muzzle Flash -------------
    private void ShowMuzzleFlash()
    {
        if (muzzleFlash == null) return;

        muzzleFlash.SetActive(true);
        flashTimer = flashDuration;
    }

    // ------------- SHOOT BULLET -------------
    private void ShootBullet()
    {
        if (bulletLinePrefab == null)
            return;

        Vector3 muzzlePos = t.position + t.TransformDirection(laserOffset);
        Vector3 shootDir = (lastLaserEnd - muzzlePos).normalized;

        GameObject bullet = Instantiate(bulletLinePrefab, muzzlePos, Quaternion.LookRotation(shootDir));

        BulletTracer bt = bullet.GetComponent<BulletTracer>();
        if (bt != null)
        {
            bt.speed = bulletSpeed;
            bt.tracerLength = bulletLength;
            bt.maxDistance = bulletMaxDistance;
        }
    }
}
