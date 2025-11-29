using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    // ------------- WEAPON DATA -------------
    [Header("Weapon Data")]
    public WeaponData weaponData;

    // ------------- LINE RENDERER -------------
    private LineRenderer lineRenderer;
    private Transform t;

    // ------------- SHOOTING -------------
    private bool shootingHeld = false;
    private float fireCooldown = 0f;

    // ------------- AIMING -------------
    private bool isAiming = false;

    // ------------- RECOIL -------------
    private Vector3 recoilOffset = Vector3.zero;
    private float flashTimer = 0f;

    // ------------- EFFECTS -------------
    [Header("Effects")]
    [SerializeField]
    private GameObject muzzleFlash;

    // ------------- PLAYER -------------
    [Header("Player")]
    [SerializeField]
    private PlayerController player;

    // ------------- LASER -------------
    private Vector3 lastLaserEnd;
    private Vector3 laserDirection;

    // ------------- START -------------
    void Start()
    {
        t = transform;

        if (player == null)
            player = GetComponentInParent<PlayerController>();

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }

    // ------------- INPUT ACTIONS -------------
    public void OnAim(InputAction.CallbackContext ctx)
    {
        isAiming = ctx.ReadValueAsButton();

        if (player != null)
            player.SetAiming(isAiming);
    }

    // ------------- SHOOT -------------
    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            shootingHeld = true;

        if (ctx.canceled)
            shootingHeld = false;
    }

    // ------------- UPDATE -------------
    void Update()
    {
        HandleLaser();
        HandleMuzzleFlash();

        recoilOffset = Vector3.Lerp(
            recoilOffset,
            Vector3.zero,
            Time.deltaTime * weaponData.recoilReturnSpeed
        );

        if (shootingHeld && isAiming)
        {
            if (fireCooldown <= 0f)
            {
                ShootBullet();
                ShowMuzzleFlash();
                ApplyRecoilKick();
                fireCooldown = weaponData.fireCooldown;
            }
        }

        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    // ------------- HANDLE LASER -------------
    private void HandleLaser()
    {
        if (lineRenderer == null || !isAiming)
        {
            if (lineRenderer != null)
                lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;

        Vector3 muzzlePos = t.position + t.TransformDirection(weaponData.laserOffset);

        Vector3 dir = (t.forward + recoilOffset).normalized;

        laserDirection = dir;

        Vector3 end = muzzlePos + dir * weaponData.laserDistance;

        if (Physics.Raycast(muzzlePos, dir, out RaycastHit hit, weaponData.laserDistance))
            end = hit.point;

        lastLaserEnd = end;

        lineRenderer.SetPosition(0, muzzlePos);
        lineRenderer.SetPosition(1, end);
    }

    // ------------- APPLY RECOIL KICK -------------
    private void ApplyRecoilKick()
    {
        recoilOffset = new Vector3(
            Random.Range(-weaponData.recoilAmount, weaponData.recoilAmount),
            Random.Range(0f, weaponData.recoilAmount),
            0f
        );
    }

    // ------------- HANDLE MUZZLE FLASH -------------
    private void HandleMuzzleFlash()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;

            if (flashTimer <= 0f && muzzleFlash != null)
                muzzleFlash.SetActive(false);
        }
    }

    // ------------- SHOW MUZZLE FLASH -------------
    private void ShowMuzzleFlash()
    {
        if (muzzleFlash == null)
            return;

        muzzleFlash.SetActive(true);
        flashTimer = 0.05f;
    }

    // ------------- SHOOT BULLET -------------
    private void ShootBullet()
    {
        if (weaponData.bulletPrefab == null)
            return;

        Vector3 muzzlePos = t.position + t.TransformDirection(weaponData.laserOffset);

        Vector3 dir = laserDirection;

        GameObject bulletObj = Instantiate(
            weaponData.bulletPrefab,
            muzzlePos,
            Quaternion.LookRotation(dir)
        );

        BulletTracer tracer = bulletObj.GetComponent<BulletTracer>();
        if (tracer != null)
        {
            tracer.speed = weaponData.bulletSpeed;
            tracer.tracerLength = weaponData.bulletLength;
            tracer.maxDistance = weaponData.bulletMaxDistance;
            tracer.damage = weaponData.damage;
        }
    }

    // ------------- SET WEAPON -------------
    public void SetWeapon(WeaponData newWeapon)
    {
        weaponData = newWeapon;
    }
}
