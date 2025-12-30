using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField]
    private QuestDatabase questDatabase;

    private Dictionary<string, QuestState> questStates = new();
    private Dictionary<QuestType, IQuestObjectiveHandler> handlers = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RegisterHandlers();
        InitializeQuests();
    }

    // ============================
    // INITIALIZATION
    // ============================

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

    // ============================
    // QUEST FLOW
    // ============================

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

        foreach (var objective in quest.Objectives)
        {
            int index = quest.Objectives.IndexOf(objective);

            if (!state.IsObjectiveCompleted(index, objective))
                return;
        }

        state.Status = QuestStatus.Completed;

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

    // ============================
    // EVENTS
    // ============================

    public void DispatchEvent(QuestType type, object data)
    {
        if (!handlers.TryGetValue(type, out var handler))
            return;

        foreach (var quest in questDatabase.Quests)
        {
            var state = questStates[quest.QuestId];
            if (state.Status != QuestStatus.Active)
                continue;

            foreach (var objective in quest.Objectives)
            {
                if (objective.Type != type)
                    continue;

                handler.Process(this, quest, objective, data);
            }
        }
    }

    // ============================
    // SAVE / LOAD
    // ============================

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

        foreach (var entry in data.States)
        {
            questStates[entry.QuestId] = entry.State;
        }
    }
}
