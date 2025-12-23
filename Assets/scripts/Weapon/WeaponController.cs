using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private WeaponData weaponData;
    private WeaponContext weaponContext;

    private WeaponModel currentWeaponModel;

    private void Awake()
    {
        weaponContext = GetComponent<WeaponContext>();
    }

    public void Shoot()
    {
        currentWeaponModel = weaponContext.CurrentWeaponModel;

        if (!weaponData.bulletPrefab || !currentWeaponModel.muzzle)
        {
            Debug.LogError("Bullet tracer prefab or muzzle not set");
            return;
        }

        Debug.Log("Shooting");

        GameObject bullet = Instantiate(
            weaponData.bulletPrefab,
            currentWeaponModel.muzzle.position,
            Quaternion.LookRotation(currentWeaponModel.muzzle.forward)
        );

        BulletTracer tracer = bullet.GetComponent<BulletTracer>();
        tracer.Init(
            currentWeaponModel.muzzle.forward,
            weaponData.bulletSpeed,
            weaponData.bulletMaxDistance
        );
    }
}
