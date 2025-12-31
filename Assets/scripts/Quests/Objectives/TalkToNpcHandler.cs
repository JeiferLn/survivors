using UnityEngine;

public class TalkToNPCHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.TalkToNPC;

    public void Process(
        QuestManager manager,
        QuestDefinition quest,
        QuestObjective objective,
        int objectiveIndex,
        object data
    )
    {
        if (data is not ScriptableObject npc)
            return;

        if (npc.name != objective.Npc.name)
            return;

        var state = manager.GetQuestState(quest.QuestId);
        state.MarkCompleted(objectiveIndex);
        manager.MarkObjectiveCompleted(quest, objective);
    }
}
