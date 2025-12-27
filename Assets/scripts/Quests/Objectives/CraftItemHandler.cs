public class CraftItemHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.CraftItem;

    public void Process(QuestManager questManager, QuestDefinition quest, object data)
    {
        if (data is not ItemEventData itemData)
            return;

        foreach (var obj in quest.Objectives)
        {
            if (obj.Type != QuestType.CraftItem)
                continue;

            if (obj.ItemId != itemData.ItemId)
                continue;

            var state = questManager.GetQuestState(quest.QuestId);
            state.AddProgress(itemData.Amount, obj.RequiredAmount);

            if (state.IsCompleted(obj.RequiredAmount))
                questManager.CompleteQuestInternal(quest);
        }
    }
}
