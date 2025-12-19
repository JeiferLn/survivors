using UnityEngine;

public enum WeaponType
{
    isEmptyWeapon,
    isOneHandWeapon,
    isTwoHandsWeapon,
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Name")]
    public string weaponName;

    [Header("Weapon Model")]
    public GameObject weaponModel;

    [Header("Damage")]
    public float damage;

    [Header("Recoil")]
    public float recoilAmount;
    public float recoilReturnSpeed;

    [Header("Laser")]
    public float laserDistance;
    public Vector3 laserOffset;

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed;
    public float bulletLength;
    public float bulletMaxDistance;

    [Header("Fire Rate")]
    public float fireCooldown;

    [Header("Weapon Type")]
    public WeaponType weaponType;
}
