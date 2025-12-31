public interface IQuestObjectiveHandler
{
    QuestType ObjectiveType { get; }

    void Process(
        QuestManager manager,
        QuestDefinition quest,
        QuestObjective objective,
        int objectiveIndex,
        object data
    );
}
