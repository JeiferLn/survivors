using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectableItem : MonoBehaviour, IInteractable
{
    [Title("Item")]
    [SerializeField]
    private ItemId itemId;

    [SerializeField, MinValue(1)]
    private int amount = 1;

    [Title("Configuración")]
    [SerializeField]
    private bool destroyOnCollect = true;

    private void Awake()
    {
        // Verificar que tenga un Collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError(
                $"[CollectableItem] {gameObject.name} no tiene un Collider. Se requiere un Collider para la interacción."
            );
        }
        else
        {
            if (!col.isTrigger)
            {
                Debug.LogWarning(
                    $"[CollectableItem] {gameObject.name} tiene un Collider que no es trigger. Considera hacerlo trigger para mejor detección."
                );
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    // IInteractable
    // ══════════════════════════════════════════════════════════════

    public void Interact()
    {
        if (itemId == null)
        {
            Debug.LogWarning("[CollectableItem] No hay ItemId asignado.");
            return;
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[CollectableItem] QuestManager.Instance es null.");
            return;
        }

        Debug.Log($"Recolectando {itemId.name} x {amount}");

        ItemEventData itemEventData = new ItemEventData(itemId.name, amount);

        QuestManager.Instance.DispatchEvent(QuestType.CollectItem, itemEventData);

        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // ══════════════════════════════════════════════════════════════
    // MÉTODOS PÚBLICOS
    // ══════════════════════════════════════════════════════════════

    public string GetInteractionText()
    {
        if (itemId == null)
            return "Recolectar";

        return $"Recolectar {itemId.name}";
    }
}
