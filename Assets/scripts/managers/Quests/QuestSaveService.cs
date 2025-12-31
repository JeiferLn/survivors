using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Servicio que maneja la serialización y deserialización de datos de misiones.
/// Extrae la lógica de guardado/carga del QuestManager para mantener la separación de responsabilidades.
/// </summary>
public class QuestSaveService
{
    private QuestDatabase questDatabase;
    private Dictionary<string, QuestState> questStates;
    private System.Action onLoadComplete;

    public QuestSaveService(
        QuestDatabase database,
        Dictionary<string, QuestState> states,
        System.Action onLoadCallback = null
    )
    {
        questDatabase = database;
        questStates = states;
        onLoadComplete = onLoadCallback;
    }

    /// <summary>
    /// Convierte el estado actual de las misiones a un formato serializable.
    /// </summary>
    public QuestSaveData GetSaveData()
    {
        QuestSaveData data = new QuestSaveData();

        foreach (var pair in questStates)
        {
            data.States.Add(new QuestStateEntry { QuestId = pair.Key, State = pair.Value });
        }

        return data;
    }

    /// <summary>
    /// Carga datos de misiones desde un formato serializable y los aplica al estado actual.
    /// </summary>
    public void LoadFromSaveData(
        QuestSaveData data,
        System.Action rebuildCache,
        System.Action syncObjectives,
        System.Action reactivateQuests,
        System.Action logSummary
    )
    {
        questStates.Clear();

        foreach (var entry in data.States)
        {
            if (string.IsNullOrEmpty(entry.QuestId))
            {
                Debug.LogWarning(
                    "[QuestSaveService] Se encontró un QuestId vacío en los datos guardados. Se omitirá."
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

        // Asegurar que todas las misiones de la base de datos tengan un estado
        if (questDatabase != null)
        {
            foreach (var quest in questDatabase.Quests)
            {
                if (!questStates.ContainsKey(quest.QuestId))
                {
                    questStates[quest.QuestId] = new QuestState { Status = QuestStatus.Locked };
                }
            }
        }

        // Ejecutar callbacks para reconstruir el estado del sistema
        rebuildCache?.Invoke();
        syncObjectives?.Invoke();
        reactivateQuests?.Invoke();
        logSummary?.Invoke();

        onLoadComplete?.Invoke();
    }
}

