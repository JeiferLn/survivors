using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    [HorizontalGroup("Header")]
    [HideLabel]
    public QuestType Type;

    [ShowIf(nameof(UsesAmount))]
    [LabelText("Required Amount")]
    public int RequiredAmount;

    [ShowIf(nameof(UsesItem))]
    [LabelText("Item Id")]
    public string ItemId;

    [ShowIf(nameof(UsesNpc))]
    [LabelText("Npc Id")]
    public string NpcId;

    [ShowIf(nameof(UsesZone))]
    [LabelText("Zone Id")]
    public string ZoneId;

    // ---------- Helpers ----------

    private bool UsesAmount =>
        Type == QuestType.CollectItem
        || Type == QuestType.CraftItem
        || Type == QuestType.SurviveTime;

    private bool UsesItem => Type == QuestType.CollectItem || Type == QuestType.CraftItem;

    private bool UsesNpc => Type == QuestType.TalkToNPC;

    private bool UsesZone => Type == QuestType.ReachZone;
}
