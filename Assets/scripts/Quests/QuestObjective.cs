using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class QuestObjective
{
    [HideLabel]
    public QuestType Type;

    // =========================
    // COUNTDOWN (SURVIVE ZONE)
    // =========================

    [ShowIf(nameof(IsCountdown))]
    [BoxGroup("Countdown")]
    [LabelText("Zone ID")]
    public string ZoneId;

    [ShowIf(nameof(IsCountdown))]
    [BoxGroup("Countdown")]
    [LabelText("Required Time (seconds)")]
    [MinValue(1)]
    public float RequiredTime;

    // =========================
    // REACH ZONE
    // =========================

    [ShowIf(nameof(IsReachZone))]
    [BoxGroup("Reach Zone")]
    [LabelText("Zone ID")]
    public string ReachZoneId;

    // =========================
    // TALK TO NPC
    // =========================

    [ShowIf(nameof(IsTalkToNpc))]
    [BoxGroup("Talk To NPC")]
    [LabelText("NPC ID")]
    public string NpcId;

    // =========================
    // COLLECT ITEM
    // =========================

    [ShowIf(nameof(IsCollectItem))]
    [BoxGroup("Collect Item")]
    [LabelText("Item ID")]
    public string CollectItemId;

    [ShowIf(nameof(IsCollectItem))]
    [BoxGroup("Collect Item")]
    [LabelText("Required Amount")]
    [MinValue(1)]
    public int CollectAmount;

    // =========================
    // CRAFT ITEM
    // =========================

    [ShowIf(nameof(IsCraftItem))]
    [BoxGroup("Craft Item")]
    [LabelText("Recipe ID")]
    public string CraftRecipeId;

    [ShowIf(nameof(IsCraftItem))]
    [BoxGroup("Craft Item")]
    [LabelText("Required Amount")]
    [MinValue(1)]
    public int CraftAmount;

    // =========================
    // ODIN HELPERS
    // =========================

    private bool IsCountdown() => Type == QuestType.Countdown;

    private bool IsReachZone() => Type == QuestType.ReachZone;

    private bool IsTalkToNpc() => Type == QuestType.TalkToNPC;

    private bool IsCollectItem() => Type == QuestType.CollectItem;

    private bool IsCraftItem() => Type == QuestType.CraftItem;
}
