using UnityEngine;

public class ReachZoneHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.ReachZone;

    public void Process(
        QuestManager manager,
        QuestDefinition quest,
        QuestObjective objective,
        object data
    )
    {
        if (data is not ScriptableObject zone)
            return;

        if (zone.name != objective.ZoneId)
            return;

        var state = manager.GetQuestState(quest.QuestId);
        int index = quest.Objectives.IndexOf(objective);

        state.MarkCompleted(index);
        manager.MarkObjectiveCompleted(quest, objective);
    }
}
