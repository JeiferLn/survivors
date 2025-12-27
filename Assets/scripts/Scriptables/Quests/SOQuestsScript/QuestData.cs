using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest")]
public class QuestDefinition : ScriptableObject
{
    public string QuestId;
    public string QuestName;

    [TextArea]
    public string Description;

    public QuestCategory Category;

    [Header("Objectives")]
    public List<QuestObjective> Objectives = new();

    [Header("Quest Flow")]
    public List<QuestDefinition> RequiredQuests = new();
    public List<QuestDefinition> UnlocksQuests = new();
    public List<QuestDefinition> BlocksQuests = new();
}
