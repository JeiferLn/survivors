using UnityEngine;

public class NpcQuestActor : MonoBehaviour, IQuestActor
{
    [SerializeField]
    private NpcId npcId;
    public ScriptableObject QuestId => npcId;
}
