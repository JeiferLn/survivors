using UnityEngine;

public class PlayerEquipmentController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform weaponOffset;

    private WeaponController weaponController;
    private PlayerState playerState;

    private GameObject currentWeaponInstance;

    public WeaponData CurrentWeapon { get; private set; }

    private void Awake()
    {
        weaponController = GetComponent<WeaponController>();
        playerState = GetComponent<PlayerState>();
    }

    // -------- PUBLIC API --------
    public void EquipWeapon(WeaponData weaponData)
    {
        UnequipCurrentWeapon();

        if (weaponData == null || weaponData.playerWeaponPrefab == null)
            return;

        currentWeaponInstance = Instantiate(weaponData.playerWeaponPrefab, weaponOffset);
        currentWeaponInstance.transform.localPosition = Vector3.zero;
        currentWeaponInstance.transform.localRotation = Quaternion.identity;

        CurrentWeapon = weaponData;

        weaponController.SetWeapon(weaponData);
        playerState.SetCombatState(PlayerCombatState.Armed);
    }

    public void UnequipCurrentWeapon()
    {
        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance);
            currentWeaponInstance = null;
        }

        CurrentWeapon = null;

        weaponController.SetWeapon(null);
        playerState.SetCombatState(PlayerCombatState.None);
    }
}
