using System;
using System.Collections.Generic;

[Serializable]
public class QuestState
{
    public QuestStatus Status;
    public List<ObjectiveProgress> ObjectivesProgress = new();

    public void AddProgress(int index, float delta)
    {
        EnsureIndex(index);
        ObjectivesProgress[index].Progress += delta;
    }

    public void AddTimeProgress(int index, float delta)
    {
        AddProgress(index, delta);
    }

    public bool IsObjectiveCompleted(int index, QuestObjective objective)
    {
        EnsureIndex(index);

        return objective.Type switch
        {
            QuestType.Countdown => ObjectivesProgress[index].Progress >= objective.RequiredTime,
            QuestType.CollectItem => ObjectivesProgress[index].Progress >= objective.RequiredAmount,
            QuestType.CraftItem => ObjectivesProgress[index].Progress
                >= objective.CraftRequiredAmount,
            _ => ObjectivesProgress[index].Completed,
        };
    }

    public void MarkCompleted(int index)
    {
        EnsureIndex(index);
        ObjectivesProgress[index].Completed = true;
    }

    public void ResetProgress(int index)
    {
        EnsureIndex(index);
        // Solo resetear si el objetivo no está completado
        if (!ObjectivesProgress[index].Completed)
        {
            ObjectivesProgress[index].Progress = 0f;
        }
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
