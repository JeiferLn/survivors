using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    // ---------------- WEAPON DATA ----------------
    [Header("Weapon Data")]
    public WeaponData weaponData;

    // ---------------- COMPONENTS ----------------
    private LineRenderer lineRenderer;
    private Transform t;
    private PlayerController player;

    // ---------------- STATE ----------------
    private bool shootingHeld = false;
    private bool isAiming = false;

    private float fireCooldown = 0f;
    private float flashTimer = 0f;

    private Vector3 recoilOffset = Vector3.zero;
    private Vector3 laserDirection;

    // ---------------- EFFECTS ----------------
    [Header("Effects")]
    [SerializeField]
    private GameObject muzzleFlash;

    // ---------------- START ----------------
    void Start()
    {
        t = transform;

        if (!TryGetComponentInParent(out player))
            Debug.LogWarning("No PlayerController found in parent.");

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }

    // ---------------- INPUT: AIM ----------------
    public void OnAim(InputAction.CallbackContext ctx)
    {
        isAiming = ctx.ReadValueAsButton();

        if (player != null)
            player.SetAiming(isAiming);
    }

    // ---------------- INPUT: SHOOT ----------------
    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            shootingHeld = true;
        }

        if (ctx.canceled)
        {
            shootingHeld = false;
        }
    }

    // ---------------- UPDATE ----------------
    void Update()
    {
        // UPDATE COOLDOWN
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        HandleLaser();
        UpdateRecoil();
        UpdateMuzzleFlash();
        HandleShooting();
    }

    // ---------------- SHOOT LOGIC ----------------
    private void HandleShooting()
    {
        if (!isAiming)
            return;

        if (!shootingHeld)
            return;

        if (fireCooldown > 0f)
            return;


        ShootOnce();
        fireCooldown = weaponData.fireCooldown;
    }

    private void ShootOnce()
    {
        ShowMuzzle();
        ApplyRecoilKick();
        ShootBullet();
    }

    // ---------------- RECOIL ----------------
    private void UpdateRecoil()
    {
        recoilOffset = Vector3.Lerp(
            recoilOffset,
            Vector3.zero,
            Time.deltaTime * weaponData.recoilReturnSpeed
        );
    }

    private void ApplyRecoilKick()
    {
        recoilOffset = new Vector3(
            Random.Range(-weaponData.recoilAmount, weaponData.recoilAmount),
            Random.Range(0f, weaponData.recoilAmount),
            0f
        );
    }

    // ---------------- LASER ----------------
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

        Vector3 muzzlePos = t.position + t.TransformDirection(weaponData.laserOffset);
        laserDirection = (t.forward + recoilOffset).normalized;

        Vector3 end = muzzlePos + laserDirection * weaponData.laserDistance;

        if (Physics.Raycast(muzzlePos, laserDirection, out RaycastHit hit, weaponData.laserDistance))
            end = hit.point;

        lineRenderer.SetPosition(0, muzzlePos);
        lineRenderer.SetPosition(1, end);
    }

    // ---------------- MUZZLE FLASH ----------------
    private void ShowMuzzle()
    {
        if (muzzleFlash == null)
            return;

        muzzleFlash.SetActive(true);
        flashTimer = 0.05f;
    }

    private void UpdateMuzzleFlash()
    {
        if (flashTimer <= 0f)
            return;

        flashTimer -= Time.deltaTime;

        if (flashTimer <= 0f && muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }

    // ---------------- BULLET ----------------
    private void ShootBullet()
    {
        if (weaponData.bulletPrefab == null)
            return;

        SoundManager.Instance.PlaySFX("pistol-shoot");
        Vector3 muzzlePos = t.position + t.TransformDirection(weaponData.laserOffset);

        GameObject obj = Instantiate(
            weaponData.bulletPrefab,
            muzzlePos,
            Quaternion.LookRotation(laserDirection)
        );

        if (obj.TryGetComponent(out BulletTracer tracer))
        {
            tracer.speed = weaponData.bulletSpeed;
            tracer.tracerLength = weaponData.bulletLength;
            tracer.maxDistance = weaponData.bulletMaxDistance;
            tracer.damage = weaponData.damage;
        }
    }

    // ---------------- SET WEAPON ----------------
    public void SetWeapon(WeaponData newWeapon)
    {
        weaponData = newWeapon;
        fireCooldown = 0f;
    }

    private bool TryGetComponentInParent<T>(out T comp)
    {
        comp = GetComponentInParent<T>();
        return comp != null;
    }
}
