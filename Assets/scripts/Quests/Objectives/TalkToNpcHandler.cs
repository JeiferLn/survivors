using UnityEngine;

public class TalkToNPCHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.TalkToNPC;

    public void Process(
        QuestManager manager,
        QuestDefinition quest,
        QuestObjective objective,
        object data
    )
    {
        if (data is not ScriptableObject npc)
            return;

        if (npc.name != objective.NpcId)
            return;

        var state = manager.GetQuestState(quest.QuestId);
        int index = quest.Objectives.IndexOf(objective);

        state.MarkCompleted(index);
        manager.MarkObjectiveCompleted(quest, objective);
    }
}
