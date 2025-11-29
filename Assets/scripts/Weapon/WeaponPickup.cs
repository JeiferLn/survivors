using UnityEngine;

public class WeaponPickup : MonoBehaviour, IEquipable
{
    // ------------- WEAPON DATA -------------
    [SerializeField]
    private WeaponData weaponData;

    // ------------- EQUIP -------------
    public void Equip(PlayerController player)
    {
        WeaponController wc = player.GetComponentInChildren<WeaponController>();
        if (wc != null)
        {
            wc.SetWeapon(weaponData);
            Destroy(gameObject);
        }
    }
}
