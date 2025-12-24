using UnityEngine;

public class WeaponContext : MonoBehaviour
{
    public WeaponData CurrentWeapon { get; private set; }
    public WeaponModel CurrentWeaponModel { get; private set; }

    // -------- STATE --------
    public bool HasWeapon => CurrentWeapon != null && CurrentWeaponModel != null;

    // -------- TRANSFORMS --------
    public Vector3 MuzzlePosition => CurrentWeaponModel.GetMuzzlePosition();
    public Vector3 FireDirection => CurrentWeaponModel.GetFireDirection();
    public Vector3 BulletPositionCorrected => CurrentWeaponModel.GetBulletPositionCorrected();

    // -------- BULLET --------
    public GameObject BulletPrefab => CurrentWeapon.bulletPrefab;
    public float BulletSpeed => CurrentWeapon.bulletSpeed;
    public float BulletLength => CurrentWeapon.bulletLength;
    public float BulletMaxDistance => CurrentWeapon.bulletMaxDistance;
    public float BulletDamage => CurrentWeapon.damage;

    // -------- FIRE --------
    public float FireCooldown => CurrentWeapon.fireCooldown;

    // -------- LASER --------
    public float LaserDistance => CurrentWeapon.laserDistance;

    // -------- API --------
    public void SetWeapon(WeaponData weaponData, WeaponModel weaponModel)
    {
        CurrentWeapon = weaponData;
        CurrentWeaponModel = weaponModel;
    }

    public void ClearWeapon()
    {
        CurrentWeapon = null;
        CurrentWeaponModel = null;
    }
}
