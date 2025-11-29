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
        if (!ctx.performed || currentEquipable == null)
            return;

        currentEquipable.Equip(playerController);
    }

    // ---------------- TRIGGER ENTER ----------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IEquipable equipable))
        {
            currentEquipable = equipable;
        }
    }

    // ---------------- TRIGGER EXIT ----------------
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IEquipable equipable) && currentEquipable == equipable)
        {
            currentEquipable = null;
        }
    }
}
