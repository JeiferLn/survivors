using UnityEngine;

public class CollectItemHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.CollectItem;

    public void Process(
        QuestManager manager,
        QuestDefinition quest,
        QuestObjective objective,
        object data
    )
    {
        if (data is not ItemEventData itemData)
            return;

        if (itemData.ItemId != objective.TargetId)
            return;

        var state = manager.GetQuestState(quest.QuestId);
        int index = quest.Objectives.IndexOf(objective);

        state.AddTimeProgress(index, itemData.Amount);

        if (state.IsObjectiveCompleted(index, objective))
        {
            state.MarkCompleted(index);
            manager.MarkObjectiveCompleted(quest, objective);
        }
    }
}
