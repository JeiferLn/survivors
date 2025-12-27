public class TalkNpcHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.TalkToNPC;

    public void Process(QuestManager questManager, QuestDefinition quest, object data)
    {
        if (data is not string npcId)
            return;

        foreach (var obj in quest.Objectives)
        {
            if (obj.ObjectiveType != QuestType.TalkToNPC)
                continue;

            if (obj.NpcId != npcId)
                continue;

            questManager.CompleteQuestInternal(quest);
        }
    }
}
