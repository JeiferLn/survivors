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
            Debug.LogError("[QuestManager] QuestDatabase no está asignado en el Inspector.");
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
            if (state.Status != QuestStatus.Completed && CanActivateQuest(quest))
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
                Debug.LogError(
                    $"[QuestManager] ERROR: La misión '{quest.QuestName}' tiene un QuestId vacío. Cada misión debe tener un QuestId único."
                );
                continue;
            }

            if (questIds.Contains(quest.QuestId))
            {
                Debug.LogError(
                    $"[QuestManager] ERROR: La misión '{quest.QuestName}' tiene un QuestId duplicado: '{quest.QuestId}'. Cada misión debe tener un QuestId único."
                );
                continue;
            }

            questIds.Add(quest.QuestId);
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
        {
            Debug.LogWarning($"[QuestManager] No hay handler registrado para tipo: {type}");
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

        Debug.LogWarning(
            $"[QuestManager] QuestId '{questId}' no encontrado. Se retornará un estado nuevo."
        );
        var newState = new QuestState { Status = QuestStatus.Locked };
        questStates[questId] = newState;
        return newState;
    }

    [ContextMenu("Reset All Quests")]
    public void ResetAllQuests()
    {
        questStates.Clear();
        activeQuestsCache.Clear();
        questsInitialized = false;

        Debug.Log("[QuestManager] Todas las misiones han sido reseteadas.");

        if (questDatabase != null)
        {
            InitializeQuests();
            questsInitialized = true;
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

    private void LogQuestStatusSummary()
    {
        if (questDatabase == null)
            return;

        var completed = new List<string>();
        var inProgress = new List<string>();
        var activated = new List<string>();

        foreach (var quest in questDatabase.Quests)
        {
            var state = questStates[quest.QuestId];
            if (state.Status == QuestStatus.Completed)
            {
                completed.Add(quest.QuestName);
            }
            else if (state.Status == QuestStatus.Active)
            {
                bool hasProgress = state.ObjectivesProgress.Any(p => p.Progress > 0);
                if (hasProgress)
                    inProgress.Add(quest.QuestName);
                else
                    activated.Add(quest.QuestName);
            }
        }

        Debug.Log("=== RESUMEN DE MISIONES AL CARGAR ===");
        Debug.Log(
            completed.Count > 0
                ? $"[QuestManager] ✓ Misiones completadas ({completed.Count}): {string.Join(", ", completed)}"
                : "[QuestManager] ✓ Misiones completadas: Ninguna"
        );
        Debug.Log(
            inProgress.Count > 0
                ? $"[QuestManager] ⏳ Misiones en progreso ({inProgress.Count}): {string.Join(", ", inProgress)}"
                : "[QuestManager] ⏳ Misiones en progreso: Ninguna"
        );
        Debug.Log(
            activated.Count > 0
                ? $"[QuestManager] ▶ Misiones activadas ({activated.Count}): {string.Join(", ", activated)}"
                : "[QuestManager] ▶ Misiones activadas: Ninguna"
        );
        Debug.Log("=====================================");
    }

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

            if (state.Status == QuestStatus.Locked && CanActivateQuest(quest))
            {
                ActivateQuest(quest);
            }
        }
    }
}
