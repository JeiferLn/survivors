using System.IO;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    // ------------- REFERENCES -------------
    private PlayerState playerState;
    private LineRenderer laserRenderer;

    [Header("Weapon Data")]
    public WeaponData weaponData;

    [Header("Laser Smoothing")]
    [SerializeField]
    private float laserSmoothSpeed = 20f;

    [SerializeField]
    private float laserDelay = 0.5f;

    private Vector3 currentLaserEnd;
    private Vector3 targetLaserEnd;

    private Transform firePoint;
    private WeaponModel currentWeaponModel;
    private float fireCooldown;

    private float currentLaserDelay;
    private bool wasAiming;

    private void Awake()
    {
        laserRenderer = GetComponent<LineRenderer>();
        playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        if (playerState.IsAiming && !wasAiming)
        {
            currentLaserDelay = laserDelay;
        }

        if (!playerState.IsAiming && wasAiming)
        {
            laserRenderer.enabled = false;
            if (currentLaserDelay < laserDelay)
            {
                currentLaserDelay += Time.deltaTime;
            }
            else
            {
                currentLaserDelay = laserDelay;
            }
        }

        wasAiming = playerState.IsAiming;

        if (currentLaserDelay > 0f && playerState.IsAiming)
        {
            currentLaserDelay -= Time.deltaTime;
            laserRenderer.enabled = false;
            return;
        }

        UpdateLaser();
    }

    // -------- PUBLIC API --------
    public void TryShoot()
    {
        if (!CanShoot())
            return;

        ShootOnce();
        fireCooldown = weaponData.fireCooldown;
    }

    // -------- INTERNAL --------
    private bool CanShoot()
    {
        return weaponData != null && firePoint != null && currentWeaponModel != null;
    }

    private void ShootOnce()
    {
        Vector3 origin = currentWeaponModel.GetMuzzlePosition();
        Vector3 direction = currentWeaponModel.GetFireDirection();
        Ray ray = new Ray(origin, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.bulletMaxDistance))
        {
            Debug.Log($"Impacto en {hit.collider.name}");
        }
        else
        {
            Debug.Log("Disparo sin impacto");
        }
    }

    private void UpdateLaser()
    {
        if (
            !playerState.IsAiming
            || weaponData == null
            || firePoint == null
            || laserRenderer == null
        )
        {
            laserRenderer.enabled = false;
            return;
        }

        laserRenderer.enabled = true;

        Vector3 origin = currentWeaponModel.GetMuzzlePosition();
        Vector3 direction = currentWeaponModel.GetFireDirection();

        laserRenderer.SetPosition(0, origin);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, weaponData.laserDistance))
        {
            targetLaserEnd = hit.point;
        }
        else
        {
            targetLaserEnd = origin + direction * weaponData.laserDistance;
        }

        currentLaserEnd = Vector3.Lerp(
            currentLaserEnd,
            targetLaserEnd,
            laserSmoothSpeed * Time.deltaTime
        );

        laserRenderer.SetPosition(1, currentLaserEnd);
    }

    // -------- SETTERS --------
    public void SetWeaponModel(WeaponModel weaponModel)
    {
        currentWeaponModel = weaponModel;
        firePoint = weaponModel != null ? weaponModel.muzzle : null;

        if (weaponData != null && firePoint != null)
        {
            currentLaserEnd = firePoint.position + firePoint.forward * weaponData.laserDistance;
            targetLaserEnd = currentLaserEnd;
        }
    }

    public void SetWeapon(WeaponData newWeaponData)
    {
        weaponData = newWeaponData;
        fireCooldown = 0f;
    }
}
