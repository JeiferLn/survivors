using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private WeaponContext weaponContext;
    private float fireCooldown;

    private void Awake()
    {
        weaponContext = GetComponent<WeaponContext>();
    }

    private void Update()
    {
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    public void TryShoot()
    {
        if (!CanShoot())
            return;

        ShootOnce();
        fireCooldown = weaponContext.CurrentWeapon.fireCooldown;
    }

    private bool CanShoot()
    {
        return weaponContext.CurrentWeapon != null && weaponContext.CurrentWeaponModel != null;
    }

    private void ShootOnce()
    {
        var weapon = weaponContext.CurrentWeapon;
        var model = weaponContext.CurrentWeaponModel;

        Vector3 origin = model.GetMuzzlePosition();
        Vector3 direction = model.GetFireDirection();

        Physics.Raycast(origin, direction, out _, weapon.bulletMaxDistance);
    }
}
