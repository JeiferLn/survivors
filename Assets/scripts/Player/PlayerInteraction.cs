using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private IEquipable currentEquipable;
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    // ---------------- EQUIP INPUT ----------------
    public void OnEquip(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if (currentEquipable == null)
            return;

        if (currentEquipable is IEquipable equipable)
        {
            equipable.Equip(playerController);
            currentEquipable = null;
        }
    }

    // ---------------- TRIGGER ENTER ----------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IEquipable>(out var equipable))
        {
            currentEquipable = equipable;
        }
    }

    // ---------------- TRIGGER EXIT ----------------
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IEquipable>(out var equipable) && currentEquipable == equipable)
        {
            currentEquipable = null;
        }
    }
}
