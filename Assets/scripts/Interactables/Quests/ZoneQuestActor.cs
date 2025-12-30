using UnityEngine;

public class ZoneQuestActor : MonoBehaviour, IQuestActor
{
    [SerializeField]
    private ZoneId zoneId;
    public ScriptableObject QuestId => zoneId;
}
