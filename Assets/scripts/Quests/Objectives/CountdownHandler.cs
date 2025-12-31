using UnityEngine;

public class CountdownHandler : IQuestObjectiveHandler
{
    public QuestType ObjectiveType => QuestType.Countdown;

    private float debugLogInterval = 1f;
    private float debugTimer = 0f;

    public void Process(
        QuestManager manager,
        QuestDefinition quest,
        QuestObjective objective,
        int objectiveIndex,
        object data
    )
    {
        if (data is not CountdownEventData eventData)
        {
            Debug.LogWarning($"[CountdownHandler] Datos inválidos para misión: {quest.QuestName}");
            return;
        }

        if (eventData.Zone != objective.Zone)
        {
            return;
        }

        var state = manager.GetQuestState(quest.QuestId);

        if (state.IsObjectiveCompleted(objectiveIndex, objective))
        {
            return;
        }

        float progressBefore =
            state.ObjectivesProgress.Count > objectiveIndex
                ? state.ObjectivesProgress[objectiveIndex].Progress
                : 0f;

        state.AddTimeProgress(objectiveIndex, eventData.DeltaTime);

        float progressAfter = state.ObjectivesProgress[objectiveIndex].Progress;
        float requiredTime = objective.RequiredTime;

        debugTimer += eventData.DeltaTime;
        if (debugTimer >= debugLogInterval)
        {
            Debug.Log(
                $"[CountdownHandler] Progresando misión: '{quest.QuestName}' ({quest.QuestId})\n"
                    + $"  - Progreso: {progressAfter:F2}s / {requiredTime:F2}s ({progressAfter / requiredTime * 100:F1}%)\n"
                    + $"  - Zona: {objective.Zone?.name ?? "null"}\n"
                    + $"  - DeltaTime: {eventData.DeltaTime:F4}s"
            );
            debugTimer = 0f;
        }

        if (state.IsObjectiveCompleted(objectiveIndex, objective))
        {
            Debug.Log(
                $"[CountdownHandler] ✓ Objetivo completado - Misión: {quest.QuestName}, Progreso final: {progressAfter:F2}s / {requiredTime:F2}s"
            );
            state.MarkCompleted(objectiveIndex);
            manager.MarkObjectiveCompleted(quest, objective);
        }
    }
}
