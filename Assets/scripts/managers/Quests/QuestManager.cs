using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField]
    private QuestDatabase questDatabase;

    private Dictionary<string, QuestState> questStates = new();
    private Dictionary<QuestType, IQuestObjectiveHandler> handlers = new();
    private Dictionary<
        QuestType,
        List<(QuestDefinition quest, QuestObjective objective, int index)>
    > activeQuestsCache = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        RegisterHandlers();
        InitializeQuests();
    }

    private void RegisterHandlers()
    {
        RegisterHandler(new CountdownHandler());
        RegisterHandler(new CollectItemHandler());
        RegisterHandler(new CraftItemHandler());
        RegisterHandler(new TalkToNPCHandler());
        RegisterHandler(new ReachZoneHandler());
    }

    private void RegisterHandler(IQuestObjectiveHandler handler)
    {
        handlers[handler.ObjectiveType] = handler;
    }

    private void InitializeQuests()
    {
        foreach (var quest in questDatabase.Quests)
        {
            if (!questStates.ContainsKey(quest.QuestId))
            {
                questStates[quest.QuestId] = new QuestState { Status = QuestStatus.Locked };
            }
        }

        foreach (var quest in questDatabase.Quests)
        {
            if (CanActivateQuest(quest))
            {
                ActivateQuest(quest);
            }
        }
    }

    private bool CanActivateQuest(QuestDefinition quest)
    {
        foreach (var required in quest.RequiredQuests)
        {
            if (!questStates.TryGetValue(required.QuestId, out var state))
                return false;

            if (state.Status != QuestStatus.Completed)
                return false;
        }

        foreach (var blocked in quest.BlocksQuests)
        {
            if (questStates.TryGetValue(blocked.QuestId, out var state))
            {
                if (state.Status == QuestStatus.Active)
                    return false;
            }
        }

        return true;
    }

    private void ActivateQuest(QuestDefinition quest)
    {
        var state = questStates[quest.QuestId];
        state.Status = QuestStatus.Active;
        UpdateActiveQuestCache(quest, true);
        Debug.Log($"Misión inicializada: {quest.QuestName} - {quest.QuestId}");
    }

    internal void MarkObjectiveCompleted(QuestDefinition quest, QuestObjective objective)
    {
        Debug.Log($"Objetivo completado: {objective.Type}");
        CheckQuestCompletion(quest);
    }

    private void CheckQuestCompletion(QuestDefinition quest)
    {
        var state = questStates[quest.QuestId];

        for (int i = 0; i < quest.Objectives.Count; i++)
        {
            var objective = quest.Objectives[i];
            if (!state.IsObjectiveCompleted(i, objective))
                return;
        }

        state.Status = QuestStatus.Completed;
        UpdateActiveQuestCache(quest, false);
        Debug.Log($"MISIÓN COMPLETADA: {quest.QuestId}");

        HandleQuestUnlocks(quest);
    }

    private void HandleQuestUnlocks(QuestDefinition quest)
    {
        foreach (var unlock in quest.UnlocksQuests)
        {
            if (CanActivateQuest(unlock))
            {
                ActivateQuest(unlock);
            }
        }
    }

    public void DispatchEvent(QuestType type, object data)
    {
        if (!handlers.TryGetValue(type, out var handler))
            return;

        if (!activeQuestsCache.TryGetValue(type, out var activeObjectives))
            return;

        foreach (var (quest, objective, index) in activeObjectives.ToList())
        {
            handler.Process(this, quest, objective, index, data);
        }
    }

    private void UpdateActiveQuestCache(QuestDefinition quest, bool isActive)
    {
        if (isActive)
        {
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                var objective = quest.Objectives[i];
                var type = objective.Type;

                if (!activeQuestsCache.ContainsKey(type))
                {
                    activeQuestsCache[type] = new List<(QuestDefinition, QuestObjective, int)>();
                }

                activeQuestsCache[type].Add((quest, objective, i));
            }
        }
        else
        {
            foreach (var type in activeQuestsCache.Keys.ToList())
            {
                activeQuestsCache[type].RemoveAll(x => x.quest.QuestId == quest.QuestId);

                if (activeQuestsCache[type].Count == 0)
                {
                    activeQuestsCache.Remove(type);
                }
            }
        }
    }

    public QuestState GetQuestState(string questId)
    {
        return questStates[questId];
    }

    public QuestSaveData GetSaveData()
    {
        QuestSaveData data = new QuestSaveData();

        foreach (var pair in questStates)
        {
            data.States.Add(new QuestStateEntry { QuestId = pair.Key, State = pair.Value });
        }

        return data;
    }

    public void LoadFromSaveData(QuestSaveData data)
    {
        questStates.Clear();
        activeQuestsCache.Clear();

        foreach (var entry in data.States)
        {
            var quest = questDatabase.GetQuestById(entry.QuestId);
            if (quest == null)
            {
                Debug.LogWarning(
                    $"QuestId '{entry.QuestId}' no encontrado en la base de datos. Se omitirá."
                );
                continue;
            }

            if (entry.State == null)
            {
                Debug.LogWarning(
                    $"Estado nulo para QuestId '{entry.QuestId}'. Se creará un estado nuevo."
                );
                questStates[entry.QuestId] = new QuestState { Status = QuestStatus.Locked };
                continue;
            }

            if (
                entry.State.ObjectivesProgress != null
                && entry.State.ObjectivesProgress.Count > quest.Objectives.Count
            )
            {
                Debug.LogWarning(
                    $"QuestId '{entry.QuestId}' tiene más progreso de objetivos que objetivos definidos. Se ajustará."
                );
                entry.State.ObjectivesProgress = entry
                    .State.ObjectivesProgress.Take(quest.Objectives.Count)
                    .ToList();
            }

            questStates[entry.QuestId] = entry.State;
        }

        foreach (var quest in questDatabase.Quests)
        {
            if (!questStates.ContainsKey(quest.QuestId))
            {
                questStates[quest.QuestId] = new QuestState { Status = QuestStatus.Locked };
            }
        }

        RebuildActiveQuestCache();
        ReactivateUnlockedQuests();
    }

    private void RebuildActiveQuestCache()
    {
        activeQuestsCache.Clear();

        foreach (var quest in questDatabase.Quests)
        {
            var state = questStates[quest.QuestId];
            if (state.Status == QuestStatus.Active)
            {
                UpdateActiveQuestCache(quest, true);
            }
        }
    }

    private void ReactivateUnlockedQuests()
    {
        foreach (var quest in questDatabase.Quests)
        {
            var state = questStates[quest.QuestId];

            if (state.Status == QuestStatus.Locked && CanActivateQuest(quest))
            {
                ActivateQuest(quest);
            }
        }
    }
}
