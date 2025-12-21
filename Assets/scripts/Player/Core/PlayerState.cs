using UnityEngine;

public class PlayerState : MonoBehaviour
{
    // -------- MOVEMENT --------
    public PlayerLocomotionState Movement { get; private set; } = PlayerLocomotionState.Idle;

    // -------- COMBAT --------
    public PlayerCombatState Combat { get; private set; } = PlayerCombatState.None;

    // -------- AIMING --------
    public bool IsAiming { get; private set; }

    // -------- MOVEMENT API --------
    public void SetMovementState(PlayerLocomotionState newState)
    {
        if (Movement == newState)
            return;

        Movement = newState;
    }

    public void SetLocomotion(PlayerLocomotionState newState)
    {
        SetMovementState(newState);
    }

    // -------- COMBAT API --------
    public void SetCombatState(PlayerCombatState newState)
    {
        if (Combat == newState)
            return;

        Combat = newState;
    }

    // -------- AIM API --------
    public void SetAiming(bool value)
    {
        IsAiming = value;
    }

    // -------- CONVENIENCE --------
    public bool IsIdle => Movement == PlayerLocomotionState.Idle;
    public bool IsMoving => Movement != PlayerLocomotionState.Idle;
    public bool IsRunning => Movement == PlayerLocomotionState.Running;

    public bool IsUnarmed => Combat == PlayerCombatState.None;
    public bool IsArmed => Combat == PlayerCombatState.Armed;
}
