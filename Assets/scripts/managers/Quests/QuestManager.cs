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

    private QuestSaveService saveService;

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
        InitializeSaveService();
    }

    private void Start()
    {
        if (!questsInitialized)
        {
            InitializeQuests();
            questsInitialized = true;
        }
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

    private void InitializeSaveService()
    {
        saveService = new QuestSaveService(questDatabase, questStates);
    }

    private void InitializeQuests()
    {
        if (questDatabase == null)
        {
            return;
        }

        ValidateQuestIds();

        foreach (var quest in questDatabase.Quests)
        {
            if (string.IsNullOrEmpty(quest.QuestId))
                continue;

            if (!questStates.ContainsKey(quest.QuestId))
            {
                questStates[quest.QuestId] = new QuestState { Status = QuestStatus.Locked };
            }

            var state = questStates[quest.QuestId];

            if (state.Status == QuestStatus.Locked && HasRequiredQuestsCompleted(quest))
            {
                UnlockQuest(quest);
            }

            if (
                state.Status == QuestStatus.Available
                && quest.AutoActivateOnStart
                && CanActivateQuest(quest)
            )
            {
                ActivateQuest(quest);
            }
        }
    }

    private void ValidateQuestIds()
    {
        var questIds = new HashSet<string>();
        foreach (var quest in questDatabase.Quests)
        {
            if (string.IsNullOrEmpty(quest.QuestId))
            {
                continue;
            }

            if (questIds.Contains(quest.QuestId))
            {
                continue;
            }

            questIds.Add(quest.QuestId);
        }
    }

    private bool HasRequiredQuestsCompleted(QuestDefinition quest)
    {
        foreach (var required in quest.RequiredQuests)
        {
            if (!questStates.TryGetValue(required.QuestId, out var state))
                return false;

            if (state.Status != QuestStatus.Completed)
                return false;
        }

        return true;
    }

    private bool CanActivateQuest(QuestDefinition quest)
    {
        if (!HasRequiredQuestsCompleted(quest))
            return false;

        if (IsQuestBlocked(quest))
            return false;

        return true;
    }

    private void UnlockQuest(QuestDefinition quest)
    {
        var state = questStates[quest.QuestId];
        state.Status = QuestStatus.Available;
    }

    private void ActivateQuest(QuestDefinition quest)
    {
        var state = questStates[quest.QuestId];
        state.Status = QuestStatus.Active;
        UpdateActiveQuestCache(quest, true);
        Debug.Log($"Misión activada: {quest.QuestName}");
    }

    internal void MarkObjectiveCompleted(QuestDefinition quest, QuestObjective objective)
    {
        if (objective.Type == QuestType.TalkToNPC)
        {
            Debug.Log(
                $"Objetivo completado: Hablar con {objective.Npc.name} (Misión: {quest.QuestName})"
            );
        }
        else if (objective.Type == QuestType.CollectItem)
        {
            Debug.Log(
                $"Objetivo completado: Recolectar {objective.RequiredAmount}x {objective.Item.name} (Misión: {quest.QuestName})"
            );
        }
        else if (objective.Type == QuestType.CraftItem)
        {
            Debug.Log(
                $"Objetivo completado: Craftear {objective.CraftRequiredAmount}x {objective.ItemToCraft.name} (Misión: {quest.QuestName})"
            );
        }

        CheckQuestCompletion(quest);
    }

    private void CheckQuestCompletion(QuestDefinition quest)
    {
        var state = questStates[quest.QuestId];

        if (IsQuestBlocked(quest))
        {
            return;
        }

        for (int i = 0; i < quest.Objectives.Count; i++)
        {
            var objective = quest.Objectives[i];
            if (!state.IsObjectiveCompleted(i, objective))
                return;
        }

        state.Status = QuestStatus.Completed;
        UpdateActiveQuestCache(quest, false);

        bool hasCollectItemObjective = false;
        bool hasCraftItemObjective = false;
        foreach (var obj in quest.Objectives)
        {
            if (obj.Type == QuestType.CollectItem)
            {
                hasCollectItemObjective = true;
            }
            if (obj.Type == QuestType.CraftItem)
            {
                hasCraftItemObjective = true;
            }
        }

        if (hasCollectItemObjective)
        {
            Debug.Log($"🎯 Misión completada (CollectItem): {quest.QuestName}");
        }

        if (hasCraftItemObjective)
        {
            Debug.Log($"🎯 Misión completada (CraftItem): {quest.QuestName}");
        }

        HandleQuestUnlocks(quest);
        HandleQuestBlocks(quest);
    }

    private void HandleQuestUnlocks(QuestDefinition quest)
    {
        foreach (var unlock in quest.UnlocksQuests)
        {
            if (!questStates.ContainsKey(unlock.QuestId))
            {
                questStates[unlock.QuestId] = new QuestState { Status = QuestStatus.Locked };
            }

            var unlockState = questStates[unlock.QuestId];

            if (unlockState.Status == QuestStatus.Locked)
            {
                UnlockQuest(unlock);
            }

            if (CanActivateQuest(unlock))
            {
                ActivateQuest(unlock);
            }
        }

        foreach (var otherQuest in questDatabase.Quests)
        {
            if (string.IsNullOrEmpty(otherQuest.QuestId))
                continue;

            if (otherQuest.RequiredQuests.Contains(quest))
            {
                if (!questStates.ContainsKey(otherQuest.QuestId))
                {
                    questStates[otherQuest.QuestId] = new QuestState
                    {
                        Status = QuestStatus.Locked,
                    };
                }

                var otherState = questStates[otherQuest.QuestId];

                if (
                    otherState.Status == QuestStatus.Locked
                    && HasRequiredQuestsCompleted(otherQuest)
                )
                {
                    UnlockQuest(otherQuest);
                }
            }
        }
    }

    private void HandleQuestBlocks(QuestDefinition quest)
    {
        foreach (var blocked in quest.BlocksQuests)
        {
            if (questStates.TryGetValue(blocked.QuestId, out var state))
            {
                if (
                    state.Status == QuestStatus.Active
                    || state.Status == QuestStatus.Locked
                    || state.Status == QuestStatus.Available
                )
                {
                    state.Status = QuestStatus.Failed;
                    UpdateActiveQuestCache(blocked, false);
                }
            }
        }
    }

    private bool IsQuestBlocked(QuestDefinition quest)
    {
        foreach (var otherQuest in questDatabase.Quests)
        {
            if (string.IsNullOrEmpty(otherQuest.QuestId))
                continue;

            if (otherQuest.QuestId == quest.QuestId)
                continue;

            if (!questStates.TryGetValue(otherQuest.QuestId, out var otherState))
                continue;

            if (otherState.Status != QuestStatus.Completed)
                continue;

            foreach (var blocked in otherQuest.BlocksQuests)
            {
                if (blocked.QuestId == quest.QuestId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void DispatchEvent(QuestType type, object data)
    {
        if (!handlers.TryGetValue(type, out var handler))
        {
            return;
        }

        if (!activeQuestsCache.TryGetValue(type, out var activeObjectives))
        {
            return;
        }

        var objectivesCopy = activeObjectives.ToList();
        foreach (var (quest, objective, index) in objectivesCopy)
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
        if (questStates.TryGetValue(questId, out var state))
        {
            return state;
        }

        var newState = new QuestState { Status = QuestStatus.Locked };
        questStates[questId] = newState;
        return newState;
    }

    public bool TryActivateQuest(string questId)
    {
        if (questDatabase == null)
        {
            return false;
        }

        var quest = questDatabase.GetQuestById(questId);
        if (quest == null)
        {
            return false;
        }

        return TryActivateQuest(quest);
    }

    public bool TryActivateQuest(QuestDefinition quest)
    {
        if (quest == null || string.IsNullOrEmpty(quest.QuestId))
        {
            return false;
        }

        if (!questStates.ContainsKey(quest.QuestId))
        {
            questStates[quest.QuestId] = new QuestState { Status = QuestStatus.Locked };
        }

        var state = questStates[quest.QuestId];

        if (state.Status == QuestStatus.Available && CanActivateQuest(quest))
        {
            ActivateQuest(quest);
            return true;
        }

        if (state.Status == QuestStatus.Locked && CanActivateQuest(quest))
        {
            UnlockQuest(quest);
            ActivateQuest(quest);
            return true;
        }

        if (state.Status == QuestStatus.Active)
        {
            return false;
        }

        if (state.Status == QuestStatus.Completed)
        {
            return false;
        }

        return false;
    }

    public void ResetCountdownProgressForZone(ZoneId zone)
    {
        if (zone == null)
            return;

        if (questDatabase == null)
            return;

        foreach (var quest in questDatabase.Quests)
        {
            if (string.IsNullOrEmpty(quest.QuestId))
                continue;

            var state = GetQuestState(quest.QuestId);
            if (state.Status != QuestStatus.Active)
                continue;

            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                var objective = quest.Objectives[i];
                if (objective.Type == QuestType.Countdown && objective.Zone == zone)
                {
                    if (!state.IsObjectiveCompleted(i, objective))
                    {
                        state.ResetProgress(i);
                    }
                }
            }
        }
    }

    public QuestSaveData GetSaveData()
    {
        if (saveService == null)
            InitializeSaveService();

        return saveService.GetSaveData();
    }

    public void LoadFromSaveData(QuestSaveData data)
    {
        if (saveService == null)
            InitializeSaveService();

        activeQuestsCache.Clear();

        saveService.LoadFromSaveData(
            data,
            RebuildActiveQuestCache,
            SyncCompletedObjectives,
            ReactivateUnlockedQuests,
            LogQuestStatusSummary
        );

        questsInitialized = true;
    }

    private bool questsInitialized = false;

    private void SyncCompletedObjectives()
    {
        if (questDatabase == null)
            return;

        foreach (var quest in questDatabase.Quests)
        {
            if (string.IsNullOrEmpty(quest.QuestId))
                continue;

            if (!questStates.TryGetValue(quest.QuestId, out var state))
                continue;

            if (state.Status != QuestStatus.Active)
                continue;

            bool anyObjectiveCompleted = false;
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                if (state.IsObjectiveCompleted(i, quest.Objectives[i]))
                {
                    if (!state.ObjectivesProgress[i].Completed)
                    {
                        state.MarkCompleted(i);
                        anyObjectiveCompleted = true;
                    }
                }
            }

            if (anyObjectiveCompleted)
            {
                CheckQuestCompletion(quest);
            }
        }
    }

    private void RebuildActiveQuestCache()
    {
        activeQuestsCache.Clear();

        if (questDatabase == null)
            return;

        foreach (var quest in questDatabase.Quests)
        {
            if (string.IsNullOrEmpty(quest.QuestId))
                continue;

            if (!questStates.TryGetValue(quest.QuestId, out var state))
                continue;

            if (state.Status == QuestStatus.Active)
            {
                UpdateActiveQuestCache(quest, true);
            }
        }
    }

    private void LogQuestStatusSummary() { }

    private void ReactivateUnlockedQuests()
    {
        if (questDatabase == null)
            return;

        foreach (var quest in questDatabase.Quests)
        {
            if (string.IsNullOrEmpty(quest.QuestId))
                continue;

            if (!questStates.TryGetValue(quest.QuestId, out var state))
                continue;

            if (state.Status == QuestStatus.Locked && HasRequiredQuestsCompleted(quest))
            {
                UnlockQuest(quest);
            }

            if (
                state.Status == QuestStatus.Available
                && quest.AutoActivateOnStart
                && CanActivateQuest(quest)
            )
            {
                ActivateQuest(quest);
            }
        }
    }
}
