using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDefinition", menuName = "Quests/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    public string QuestId;
    public string QuestName;

    [TextArea]
    public string Description;

    public QuestCategory Category;
    public List<QuestObjective> Objectives = new();

    public List<QuestDefinition> RequiredQuests = new();
    public List<QuestDefinition> UnlocksQuests = new();
    public List<QuestDefinition> BlocksQuests = new();
}
