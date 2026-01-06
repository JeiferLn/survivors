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

    [Tooltip(
        "Si es true, la misión se activará automáticamente al iniciar el juego si está disponible"
    )]
    public bool AutoActivateOnStart = false;
}
