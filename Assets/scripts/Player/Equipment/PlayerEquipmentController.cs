using UnityEngine;

public class PlayerEquipmentController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform weaponOffset;

    private WeaponController weaponController;
    private PlayerState playerState;

    private GameObject currentWeaponInstance;

    private PlayerAnimationController animationController;

    public WeaponData CurrentWeapon { get; private set; }

    private void Awake()
    {
        weaponController = GetComponent<WeaponController>();
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

        weaponController.SetWeapon(weaponData);
        playerState.SetCombatState(PlayerCombatState.Armed);

        if (weaponData.playerWeaponPrefab.TryGetComponent(out WeaponIK weaponIK))
        {
            animationController.SetLeftHandIKTarget(weaponIK.leftHandGrip);
        }
    }

    public void UnequipCurrentWeapon()
    {
        if (CurrentWeapon == null)
            return;

        CurrentWeapon.playerWeaponPrefab.SetActive(false);
        CurrentWeapon = null;

        weaponController.SetWeapon(null);
        playerState.SetCombatState(PlayerCombatState.None);
    }
}
