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

    private void InitializeQuests()
    {
        // Validar QuestIds únicos
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

        foreach (var quest in questDatabase.Quests)
        {
            if (string.IsNullOrEmpty(quest.QuestId))
                continue;

            if (!questStates.ContainsKey(quest.QuestId))
            {
                questStates[quest.QuestId] = new QuestState { Status = QuestStatus.Locked };
            }
        }

        foreach (var quest in questDatabase.Quests)
        {
            if (string.IsNullOrEmpty(quest.QuestId))
                continue;

            var state = questStates[quest.QuestId];

            if (state.Status == QuestStatus.Completed)
            {
                continue;
            }

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
        {
            Debug.LogWarning($"[QuestManager] No hay handler registrado para tipo: {type}");
            return;
        }

        if (!activeQuestsCache.TryGetValue(type, out var activeObjectives))
        {
            return;
        }

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
            if (string.IsNullOrEmpty(entry.QuestId))
            {
                Debug.LogWarning(
                    "[QuestManager] Se encontró un QuestId vacío en los datos guardados. Se omitirá."
                );
                continue;
            }

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
        SyncCompletedObjectives();
        ReactivateUnlockedQuests();

        LogQuestStatusSummary();

        questsInitialized = true;
    }

    private bool questsInitialized = false;

    private void SyncCompletedObjectives()
    {
        foreach (var quest in questDatabase.Quests)
        {
            var state = questStates[quest.QuestId];

            if (state.Status != QuestStatus.Active)
            {
                continue;
            }

            bool anyObjectiveCompleted = false;

            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                var objective = quest.Objectives[i];

                // Obtener el progreso actual
                float currentProgress =
                    state.ObjectivesProgress.Count > i ? state.ObjectivesProgress[i].Progress : 0f;

                // Obtener el requerimiento según el tipo
                float required = objective.Type switch
                {
                    QuestType.Countdown => objective.RequiredTime,
                    QuestType.CollectItem => objective.RequiredAmount,
                    QuestType.CraftItem => objective.CraftRequiredAmount,
                    _ => 0f,
                };

                if (state.IsObjectiveCompleted(i, objective))
                {
                    if (!state.ObjectivesProgress[i].Completed)
                    {
                        Debug.Log(
                            $"[SyncCompletedObjectives] Marcando objetivo {i} como completado para misión '{quest.QuestName}' ({quest.QuestId})\n"
                                + $"  - Progreso: {currentProgress:F2} / {required:F2}\n"
                                + $"  - Zona: {objective.Zone?.name ?? "null"}\n"
                                + $"  - Tipo: {objective.Type}"
                        );
                        state.MarkCompleted(i);
                        anyObjectiveCompleted = true;
                    }
                }
            }

            if (anyObjectiveCompleted)
            {
                Debug.Log(
                    $"[SyncCompletedObjectives] Verificando completitud de misión '{quest.QuestName}' ({quest.QuestId})"
                );
                CheckQuestCompletion(quest);
            }
        }
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

    private void LogQuestStatusSummary()
    {
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
                // Verificar si la misión tiene objetivos con progreso
                bool hasProgress = false;
                for (
                    int i = 0;
                    i < quest.Objectives.Count && i < state.ObjectivesProgress.Count;
                    i++
                )
                {
                    if (state.ObjectivesProgress[i].Progress > 0)
                    {
                        hasProgress = true;
                        break;
                    }
                }

                if (hasProgress)
                {
                    inProgress.Add(quest.QuestName);
                }
                else
                {
                    activated.Add(quest.QuestName);
                }
            }
        }

        Debug.Log("=== RESUMEN DE MISIONES AL CARGAR ===");

        if (completed.Count > 0)
        {
            Debug.Log(
                $"[QuestManager] ✓ Misiones completadas ({completed.Count}): {string.Join(", ", completed)}"
            );
        }
        else
        {
            Debug.Log("[QuestManager] ✓ Misiones completadas: Ninguna");
        }

        if (inProgress.Count > 0)
        {
            Debug.Log(
                $"[QuestManager] ⏳ Misiones en progreso ({inProgress.Count}): {string.Join(", ", inProgress)}"
            );
        }
        else
        {
            Debug.Log("[QuestManager] ⏳ Misiones en progreso: Ninguna");
        }

        if (activated.Count > 0)
        {
            Debug.Log(
                $"[QuestManager] ▶ Misiones activadas ({activated.Count}): {string.Join(", ", activated)}"
            );
        }
        else
        {
            Debug.Log("[QuestManager] ▶ Misiones activadas: Ninguna");
        }

        Debug.Log("=====================================");
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
