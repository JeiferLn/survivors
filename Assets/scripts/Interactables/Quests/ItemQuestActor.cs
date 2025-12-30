using UnityEngine;

public class ItemQuestActor : MonoBehaviour, IQuestActor
{
    [SerializeField]
    private ItemId itemId;
    public ScriptableObject QuestId => itemId;
}
