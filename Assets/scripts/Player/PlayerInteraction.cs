using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private IEquipable currentEquipable = null;

    public void OnEquip(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && currentEquipable != null)
        {
            currentEquipable.Equip(GetComponent<PlayerController>());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IEquipable equipable))
        {
            currentEquipable = equipable;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IEquipable equipable))
        {
            if (currentEquipable == equipable)
                currentEquipable = null;
        }
    }
}
