using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponData weaponData;

    [Header("Weapon Models")]
    [SerializeField]
    private List<GameObject> weaponModels;

    [SerializeField]
    private Animator animator;

    private PlayerController player;

    // ---------------- STATE ----------------
    private bool shootingHeld;
    private bool isAiming;
    private float fireCooldown;

    void Start()
    {
        player = GetComponentInParent<PlayerController>();
        DisableAllWeapons();
    }

    // ---------------- INPUT ----------------
    public void OnAim(InputAction.CallbackContext ctx)
    {
        isAiming = ctx.ReadValueAsButton();
        player?.SetAiming(isAiming);
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            shootingHeld = true;

        if (ctx.canceled)
            shootingHeld = false;
    }

    void Update()
    {
        if (weaponData == null)
            return;

        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        animator.SetFloat("isAiming", isAiming ? 1f : 0f, 0.15f, Time.deltaTime);

        HandleShooting();
    }

    // ---------------- SHOOT ----------------
    private void HandleShooting()
    {
        if (!isAiming)
            return;

        if (!shootingHeld)
            return;

        if (fireCooldown > 0f)
            return;

        ShootOnce();
        fireCooldown = weaponData.fireCooldown;
    }

    private void ShootOnce()
    {
        Debug.Log("Disparo");
    }

    // ---------------- WEAPON SWITCH ----------------
    public void SetWeapon(WeaponData newWeapon)
    {
        DisableAllWeapons();

        weaponData = newWeapon;
        fireCooldown = 0f;

        if (newWeapon == null)
            return;

        GameObject model = newWeapon.playerWeaponModel;
        model.SetActive(true);

        animator.SetBool("hasWeaponEquipped", newWeapon.weaponType != WeaponType.isEmptyWeapon);
        animator.SetTrigger(newWeapon.weaponType.ToString());
    }

    private void DisableAllWeapons()
    {
        foreach (var model in weaponModels)
            model.SetActive(false);
    }
}
