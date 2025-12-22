using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponData weaponData;

    private LineRenderer laserRenderer;
    private Transform firePoint;
    private WeaponModel currentWeaponModel;
    private float fireCooldown;

    private void Awake()
    {
        laserRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

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
        Vector3 direction = currentWeaponModel.GetFireDirection();
        Ray ray = new Ray(firePoint.position, direction);

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
        if (!CanShoot())
        {
            laserRenderer.enabled = false;
            return;
        }

        laserRenderer.enabled = true;

        Vector3 origin = firePoint.position + firePoint.TransformDirection(weaponData.laserOffset);
        Vector3 direction = currentWeaponModel.GetFireDirection();

        laserRenderer.SetPosition(0, origin);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, weaponData.laserDistance))
        {
            laserRenderer.SetPosition(1, hit.point);
        }
        else
        {
            laserRenderer.SetPosition(1, origin + direction * weaponData.laserDistance);
        }
    }

    // -------- SETTERS --------
    public void SetWeaponModel(WeaponModel weaponModel)
    {
        currentWeaponModel = weaponModel;
        firePoint = weaponModel != null ? weaponModel.muzzle : null;
    }

    public void SetWeapon(WeaponData newWeaponData)
    {
        weaponData = newWeaponData;
        fireCooldown = 0f;
    }
}
