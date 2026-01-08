using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : MonoBehaviour
{
    private PlayerState playerState;
    private WeaponController weaponController;
    private WeaponContext weaponContext;

    private bool shootHeld;
    private float nextShootTime;

    private void Awake()
    {
        playerState = GetComponent<PlayerState>();
        weaponController = GetComponent<WeaponController>();
        weaponContext = GetComponent<WeaponContext>();
    }

    // -------- INPUT --------
    public void OnAim(InputAction.CallbackContext ctx)
    {
        playerState.SetAiming(ctx.ReadValueAsButton());
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            shootHeld = true;

        if (ctx.canceled)
            shootHeld = false;
    }

    private void Update()
    {
        HandleCombat();
    }

    // -------- LOGIC --------
    private void HandleCombat()
    {
        if (
            !playerState.IsAiming
            || !shootHeld
            || !weaponContext.HasWeapon
            || Time.time < nextShootTime
        )
            return;

        weaponController.Shoot();
        nextShootTime = Time.time + weaponContext.FireCooldown;
    }
}
