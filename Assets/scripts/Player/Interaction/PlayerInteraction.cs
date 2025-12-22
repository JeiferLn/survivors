using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private IEquipable currentEquipable;
    private PlayerEquipmentController equipmentController;

    private void Awake()
    {
        equipmentController = GetComponent<PlayerEquipmentController>();

        if (equipmentController == null)
            return;
    }

    // ---------------- INPUT ----------------
    public void OnEquip(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if (equipmentController == null)
            return;

        currentEquipable?.Equip(equipmentController);
        currentEquipable = null;
    }

    // ---------------- TRIGGERS ----------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IEquipable>(out var equipable))
            currentEquipable = equipable;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IEquipable>(out var equipable) && currentEquipable == equipable)
            currentEquipable = null;
    }
}
