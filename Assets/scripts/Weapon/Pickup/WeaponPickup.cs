using UnityEngine;

public class WeaponPickup : MonoBehaviour, IEquipable
{
    [SerializeField]
    private WeaponData weaponData;

    public void Equip(PlayerEquipmentController equipmentController)
    {
        if (equipmentController == null)
        {
            return;
        }

        if (weaponData == null)
        {
            return;
        }

        equipmentController.EquipWeapon(weaponData);
        Destroy(gameObject);
    }
}
