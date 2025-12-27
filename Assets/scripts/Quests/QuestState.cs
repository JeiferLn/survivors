public class QuestState
{
    public QuestStatus Status { get; private set; } = QuestStatus.Locked;
    public float CurrentProgress { get; private set; }

    public void SetActive()
    {
        Status = QuestStatus.Active;
    }

    public void SetCompleted()
    {
        Status = QuestStatus.Completed;
    }

    public void SetBlocked()
    {
        Status = QuestStatus.Locked;
    }

    public void AddProgress(float amount, float required)
    {
        CurrentProgress += amount;
        CurrentProgress = UnityEngine.Mathf.Clamp(CurrentProgress, 0, required);
    }

    public void AddTime(float deltaTime, float required)
    {
        AddProgress(deltaTime, required);
    }

    public bool IsCompleted(float required)
    {
        return CurrentProgress >= required;
    }

    public void Load(float progress, QuestStatus status)
    {
        CurrentProgress = progress;
        Status = status;
    }
}
