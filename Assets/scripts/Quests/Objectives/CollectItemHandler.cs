public class CollectItemHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.CollectItem;

    public void Process(QuestManager questManager, QuestData quest, object data)
    {
        if (data is not ItemEventData itemData)
            return;

        foreach (var obj in quest.Objectives)
        {
            if (obj.Type != QuestType.CollectItem)
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
