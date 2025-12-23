using UnityEngine;

public class WeaponContext : MonoBehaviour
{
    public WeaponData CurrentWeapon { get; private set; }
    public WeaponModel CurrentWeaponModel { get; private set; }

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
