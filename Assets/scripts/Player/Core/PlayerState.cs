using UnityEngine;

public class PlayerState : MonoBehaviour
{
    // -------- MOVEMENT --------
    public PlayerLocomotionState Movement { get; private set; } = PlayerLocomotionState.Idle;

    // -------- COMBAT --------
    public PlayerCombatState Combat { get; private set; } = PlayerCombatState.None;

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

    public void SetCombat(PlayerCombatState newState)
    {
        SetCombatState(newState);
    }

    // -------- CONVENIENCE PROPERTIES --------
    public bool IsIdle => Movement == PlayerLocomotionState.Idle;
    public bool IsMoving => Movement != PlayerLocomotionState.Idle;
    public bool IsRunning => Movement == PlayerLocomotionState.Running;
    public bool IsAiming => Combat == PlayerCombatState.Aiming;
}
