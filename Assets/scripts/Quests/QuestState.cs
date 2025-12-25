using System;
using UnityEngine;

public class QuestState
{
    [SerializeField]
    private string questId;

    [SerializeField]
    private QuestStatus status;

    [SerializeField]
    private int currentProgress;

    public QuestState(string questId)
    {
        this.questId = questId;
        status = QuestStatus.Locked;
        currentProgress = 0;
    }

    public string QuestId => questId;
    public QuestStatus Status => status;
    public int CurrentProgress => currentProgress;

    public void SetStatus(QuestStatus newStatus)
    {
        status = newStatus;
    }

    public void AddProgress(int amount, int target)
    {
        if (status != QuestStatus.Active)
            return;

        currentProgress += amount;
        currentProgress = Math.Min(currentProgress, target);
    }

    public bool IsCompleted(int target)
    {
        return currentProgress >= target;
    }
}
