using UnityEngine;

public class CraftItemHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.CraftItem;

    public void Process(
        QuestManager manager,
        QuestDefinition quest,
        QuestObjective objective,
        int objectiveIndex,
        object data
    )
    {
        if (data is not ItemEventData itemData)
            return;

        if (itemData.ItemId != objective.ItemToCraft.name)
            return;

        var state = manager.GetQuestState(quest.QuestId);
        state.AddProgress(objectiveIndex, itemData.Amount);

        if (state.IsObjectiveCompleted(objectiveIndex, objective))
        {
            state.MarkCompleted(objectiveIndex);
            manager.MarkObjectiveCompleted(quest, objective);
        }
    }
}
