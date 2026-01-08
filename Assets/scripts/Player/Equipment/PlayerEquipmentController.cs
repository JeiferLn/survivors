using UnityEngine;

public class PlayerEquipmentController : MonoBehaviour
{
    private WeaponContext weaponContext;
    private WeaponController weaponController;

    [Header("References")]
    [SerializeField]
    private Transform weaponOffset;
    private PlayerState playerState;

    private PlayerAnimationController animationController;

    public WeaponData CurrentWeapon { get; private set; }
    private GameObject currentWeaponPrefab;

    private void Awake()
    {
        weaponContext = GetComponent<WeaponContext>();
        weaponController = GetComponent<WeaponController>();
        playerState = GetComponent<PlayerState>();
        animationController = GetComponentInChildren<PlayerAnimationController>();
    }

    // -------- PUBLIC API --------
    public void EquipWeapon(WeaponData weaponData)
    {
        UnequipCurrentWeapon();

        if (weaponData == null || weaponData.weaponId == null)
            return;

        GameObject weaponPrefab = FindWeaponPrefabByID(weaponData.weaponId);

        if (weaponPrefab == null)
            return;

        weaponPrefab.SetActive(true);
        currentWeaponPrefab = weaponPrefab;
        CurrentWeapon = weaponData;

        playerState.SetCombatState(PlayerCombatState.Armed);

        if (weaponPrefab.TryGetComponent(out WeaponModel weaponModel))
        {
            animationController.SetLeftHandIKTarget(weaponModel.leftHandGrip);

            weaponContext.SetWeapon(weaponData, weaponModel);
        }
        else
        {
            Debug.LogWarning($"El prefab {weaponPrefab.name} no tiene el componente WeaponModel");
        }
    }

    public void UnequipCurrentWeapon()
    {
        if (CurrentWeapon == null)
            return;

        if (currentWeaponPrefab != null)
        {
            currentWeaponPrefab.SetActive(false);
            currentWeaponPrefab = null;
        }

        CurrentWeapon = null;

        weaponContext.ClearWeapon();
        playerState.SetCombatState(PlayerCombatState.None);
        animationController.SetLeftHandIKTarget(null);
    }

    // -------- HELPER METHODS --------
    private GameObject FindWeaponPrefabByID(WeaponId weaponId)
    {
        if (weaponController == null || weaponController.weaponPrefabs == null)
        {
            Debug.LogWarning("WeaponController o weaponPrefabs no están asignados");
            return null;
        }

        foreach (GameObject prefab in weaponController.weaponPrefabs)
        {
            if (prefab == null)
                continue;

            if (prefab.TryGetComponent(out WeaponModel weaponModel))
            {
                if (weaponModel.weaponId == weaponId)
                {
                    return prefab;
                }
            }
        }

        return null;
    }
}
