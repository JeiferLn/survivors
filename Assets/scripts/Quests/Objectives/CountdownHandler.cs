public class CountdownHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.Countdown;

    public void Process(
        QuestManager manager,
        QuestDefinition quest,
        QuestObjective objective,
        object data
    )
    {
        if (data is not float deltaTime)
            return;

        var state = manager.GetQuestState(quest.QuestId);
        int index = quest.Objectives.IndexOf(objective);

        state.AddTimeProgress(index, deltaTime);

        if (state.IsObjectiveCompleted(index, objective))
        {
            state.MarkCompleted(index);
            manager.MarkObjectiveCompleted(quest, objective);
        }
    }
}
