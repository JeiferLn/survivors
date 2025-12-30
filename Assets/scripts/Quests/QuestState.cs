using System;
using System.Collections.Generic;

[Serializable]
public class QuestState
{
    public QuestStatus Status;

    // Progreso por objetivo (tiempo, cantidad, etc)
    public List<ObjectiveProgress> ObjectivesProgress = new();

    public void AddTimeProgress(int index, float delta)
    {
        EnsureIndex(index);
        ObjectivesProgress[index].Progress += delta;
    }

    public bool IsObjectiveCompleted(int index, QuestObjective objective)
    {
        EnsureIndex(index);

        return objective.Type switch
        {
            QuestType.Countdown => ObjectivesProgress[index].Progress >= objective.RequiredTime,
            QuestType.CollectItem => ObjectivesProgress[index].Progress >= objective.CollectAmount,
            QuestType.CraftItem => ObjectivesProgress[index].Progress >= objective.CraftAmount,
            _ => ObjectivesProgress[index].Completed,
        };
    }

    public void MarkCompleted(int index)
    {
        EnsureIndex(index);
        ObjectivesProgress[index].Completed = true;
    }

    private void EnsureIndex(int index)
    {
        while (ObjectivesProgress.Count <= index)
        {
            ObjectivesProgress.Add(new ObjectiveProgress());
        }
    }
}

[Serializable]
public class ObjectiveProgress
{
    public float Progress;
    public bool Completed;
}
