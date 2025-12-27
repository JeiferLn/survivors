public class ReachZoneHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.ReachZone;

    public void Process(QuestManager questManager, QuestData quest, object data)
    {
        if (data is not string zoneId)
            return;

        foreach (var obj in quest.Objectives)
        {
            if (obj.Type != QuestType.ReachZone)
                continue;

            if (obj.ZoneId != zoneId)
                continue;

            questManager.CompleteQuestInternal(quest);
        }
    }
}
