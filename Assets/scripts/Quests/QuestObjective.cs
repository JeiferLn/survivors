using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class QuestObjective
{
    public QuestType Type;

    [ShowIf(nameof(IsCountdown))]
    [BoxGroup("Countdown")]
    public ZoneId Zone;

    [ShowIf(nameof(IsCountdown))]
    [BoxGroup("Countdown")]
    [MinValue(1)]
    public float RequiredTime;

    [ShowIf(nameof(IsReachZone))]
    [BoxGroup("Reach Zone")]
    public ZoneId ReachZone;

    [ShowIf(nameof(IsTalkToNpc))]
    [BoxGroup("Talk To NPC")]
    public NpcId Npc;

    [ShowIf(nameof(IsCollectItem))]
    [BoxGroup("Collect Item")]
    public ItemId Item;

    [ShowIf(nameof(IsCollectItem))]
    [BoxGroup("Collect Item")]
    [MinValue(1)]
    public int RequiredAmount;

    [ShowIf(nameof(IsCraftItem))]
    [BoxGroup("Craft Item")]
    public CraftId ItemToCraft;

    [ShowIf(nameof(IsCraftItem))]
    [BoxGroup("Craft Item")]
    [MinValue(1)]
    public int CraftRequiredAmount;

    private bool IsCountdown() => Type == QuestType.Countdown;

    private bool IsReachZone() => Type == QuestType.ReachZone;

    private bool IsTalkToNpc() => Type == QuestType.TalkToNPC;

    private bool IsCollectItem() => Type == QuestType.CollectItem;

    private bool IsCraftItem() => Type == QuestType.CraftItem;
}
