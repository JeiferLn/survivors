public interface IQuestObjectiveHandler
{
    QuestType ObjectiveType { get; }
    void Process(QuestManager questManager, QuestDefinition quest, object data);
}
