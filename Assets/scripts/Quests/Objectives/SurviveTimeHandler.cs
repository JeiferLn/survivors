public class SurviveTimeHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.SurviveTime;

    public void Process(QuestManager questManager, QuestData quest, object data)
    {
        if (data is not float deltaTime)
            return;

        foreach (var obj in quest.Objectives)
        {
            if (obj.Type != QuestType.SurviveTime)
                continue;

            var state = questManager.GetQuestState(quest.QuestId);
            state.AddProgress(1, obj.RequiredAmount);

            if (state.IsCompleted(obj.RequiredAmount))
                questManager.CompleteQuestInternal(quest);
        }
    }
}
