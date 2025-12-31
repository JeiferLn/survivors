using UnityEngine;

public class ReachZoneHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.ReachZone;

    public void Process(
        QuestManager manager,
        QuestDefinition quest,
        QuestObjective objective,
        int objectiveIndex,
        object data
    )
    {
        if (data is not ScriptableObject zone)
            return;

        if (zone.name != objective.ReachZone.name)
            return;

        var state = manager.GetQuestState(quest.QuestId);
        state.MarkCompleted(objectiveIndex);
        manager.MarkObjectiveCompleted(quest, objective);
    }
}
