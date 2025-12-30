using UnityEngine;

public class CraftQuestActo : MonoBehaviour, IQuestActor
{
    [SerializeField]
    private CraftId craftId;
    public ScriptableObject QuestId => craftId;
}
