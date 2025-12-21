using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : MonoBehaviour
{
    private PlayerState playerState;
    private WeaponController weaponController;

    private bool isAiming;
    private bool shootHeld;

    private void Awake()
    {
        playerState = GetComponent<PlayerState>();
        weaponController = GetComponent<WeaponController>();
    }

    // -------- INPUT --------
    public void OnAim(InputAction.CallbackContext ctx)
    {
        isAiming = ctx.ReadValueAsButton();
        playerState.SetCombat(isAiming ? PlayerCombatState.Aiming : PlayerCombatState.None);
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
        if (!playerState.IsAiming)
            return;

        if (!shootHeld)
            return;

        weaponController.TryShoot();
    }
}
