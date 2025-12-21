using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponData weaponData;

    private float fireCooldown;

    private void Update()
    {
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    // -------- PUBLIC API --------
    public void TryShoot()
    {
        if (weaponData == null)
            return;

        if (fireCooldown > 0f)
            return;

        ShootOnce();
        fireCooldown = weaponData.fireCooldown;
    }

    // -------- INTERNAL --------
    private void ShootOnce()
    {
        Debug.Log($"Disparo con {weaponData.name}");
    }

    // -------- WEAPON SWITCH --------
    public void SetWeapon(WeaponData newWeapon)
    {
        weaponData = newWeapon;
        fireCooldown = 0f;
    }
}
