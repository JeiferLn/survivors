using Sirenix.OdinInspector;
using UnityEngine;

public class CraftItemTest : MonoBehaviour
{
    [Title("Test de Crafteo")]
    [SerializeField]
    private CraftId itemToCraft;

    [SerializeField, MinValue(1)]
    private int craftAmount = 1;

    [Button("Craftear Item", ButtonSizes.Large), ShowIf("@UnityEngine.Application.isPlaying")]
    [GUIColor(0.4f, 0.8f, 0.4f)]
    private void TestCraftItem()
    {
        if (itemToCraft == null)
        {
            Debug.LogWarning("[CraftItemTest] No hay ItemId asignado.");
            return;
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("[CraftItemTest] QuestManager.Instance es null.");
            return;
        }

        ItemEventData itemEventData = new ItemEventData(itemToCraft.name, craftAmount);
        QuestManager.Instance.DispatchEvent(QuestType.CraftItem, itemEventData);
    }
}
