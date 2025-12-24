using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private WeaponContext weaponContext;

    private void Awake()
    {
        weaponContext = GetComponent<WeaponContext>();
    }

    public void Shoot()
    {
        if (!weaponContext.HasWeapon || weaponContext.BulletPrefab == null)
            return;

        GameObject bullet = Instantiate(
            weaponContext.BulletPrefab,
            weaponContext.BulletPositionCorrected,
            Quaternion.LookRotation(weaponContext.FireDirection)
        );

        BulletTracer tracer = bullet.GetComponent<BulletTracer>();

        if (tracer == null)
            return;

        tracer.Init(
            weaponContext.FireDirection,
            weaponContext.BulletSpeed,
            weaponContext.BulletMaxDistance,
            weaponContext.BulletLength
        );
    }
}
