public interface IQuestObjectiveHandler
{
    QuestType ObjectiveType { get; }

    void Process(
        QuestManager manager,
        QuestDefinition quest,
        QuestObjective objective,
        object data
    );
}
