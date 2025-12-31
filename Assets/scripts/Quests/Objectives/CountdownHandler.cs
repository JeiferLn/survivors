public class CountdownHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.Countdown;

    public void Process(
        QuestManager manager,
        QuestDefinition quest,
        QuestObjective objective,
        int objectiveIndex,
        object data
    )
    {
        if (data is not float deltaTime)
            return;

        var state = manager.GetQuestState(quest.QuestId);
        state.AddTimeProgress(objectiveIndex, deltaTime);

        if (state.IsObjectiveCompleted(objectiveIndex, objective))
        {
            state.MarkCompleted(objectiveIndex);
            manager.MarkObjectiveCompleted(quest, objective);
        }
    }
}
