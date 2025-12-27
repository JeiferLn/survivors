using System;
using UnityEngine;

[Serializable]
public class QuestObjective
{
    public QuestType ObjectiveType;

    [Header("Generic")]
    public int RequiredAmount;

    [Header("Item")]
    public string ItemId;

    [Header("NPC")]
    public string NpcId;

    [Header("Zone")]
    public string ZoneId;
}
