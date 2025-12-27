using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    public string QuestId;
    public string QuestName;

    [TextArea]
    public string Description;

    public QuestCategory Category;

    [Header("Objectives")]
    public List<QuestObjective> Objectives = new();

    [Header("Quest Flow")]
    public List<QuestData> RequiredQuests = new();
    public List<QuestData> UnlocksQuests = new();
    public List<QuestData> BlocksQuests = new();
}
