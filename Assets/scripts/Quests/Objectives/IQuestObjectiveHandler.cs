public interface IQuestObjectiveHandler
{
    QuestType ObjectiveType { get; }
    void Process(QuestManager questManager, QuestData quest, object data);
}
