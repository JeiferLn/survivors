using UnityEngine;

public class PlayerEquipmentController : MonoBehaviour
{
    private WeaponContext weaponContext;

    [Header("References")]
    [SerializeField]
    private Transform weaponOffset;
    private PlayerState playerState;

    private PlayerAnimationController animationController;

    public WeaponData CurrentWeapon { get; private set; }

    private void Awake()
    {
        weaponContext = GetComponent<WeaponContext>();
        playerState = GetComponent<PlayerState>();
        animationController = GetComponentInChildren<PlayerAnimationController>();
    }

    // -------- PUBLIC API --------
    public void EquipWeapon(WeaponData weaponData)
    {
        UnequipCurrentWeapon();

        if (weaponData == null || weaponData.playerWeaponPrefab == null)
            return;

        weaponData.playerWeaponPrefab.SetActive(true);

        CurrentWeapon = weaponData;

        playerState.SetCombatState(PlayerCombatState.Armed);

        if (weaponData.playerWeaponPrefab.TryGetComponent(out WeaponModel weaponModel))
        {
            animationController.SetLeftHandIKTarget(weaponModel.leftHandGrip);

            weaponContext.SetWeapon(weaponData, weaponModel);
        }
    }

    public void UnequipCurrentWeapon()
    {
        if (CurrentWeapon == null)
            return;

        CurrentWeapon.playerWeaponPrefab.SetActive(false);
        CurrentWeapon = null;

        weaponContext.ClearWeapon();
        playerState.SetCombatState(PlayerCombatState.None);
        animationController.SetLeftHandIKTarget(null);
    }
}
