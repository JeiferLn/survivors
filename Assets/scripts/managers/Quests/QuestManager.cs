using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("Database")]
    [SerializeField]
    private QuestDatabase questDatabase;

    private Dictionary<string, QuestState> questStates;

    private void Awake()
    {
        questDatabase.Initialize();
        InitializeStates();
    }

    private void InitializeStates()
    {
        questStates = new Dictionary<string, QuestState>();

        foreach (var quest in questDatabase.Quests)
        {
            questStates.Add(quest.QuestId, new QuestState(quest.QuestId));
        }

        InitializeAvailableQuests();
    }

    private void InitializeAvailableQuests()
    {
        foreach (var quest in questDatabase.Quests)
        {
            if (AreRequirementsCompleted(quest))
            {
                var state = questStates[quest.QuestId];
                if (state.Status == QuestStatus.Locked)
                {
                    state.SetStatus(QuestStatus.Available);
                }
            }
        }
    }

    private bool AreRequirementsCompleted(QuestDefinition quest)
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

    public void ActivateQuest(string questId)
    {
        if (!questStates.ContainsKey(questId))
            return;

        var state = questStates[questId];

        if (state.Status == QuestStatus.Available)
        {
            state.SetStatus(QuestStatus.Active);
        }
    }

    public void AddProgress(string questId, int amount)
    {
        var quest = questDatabase.GetQuestById(questId);
        if (quest == null)
            return;

        var state = questStates[questId];
        state.AddProgress(amount, quest.TargetAmount);

        if (state.IsCompleted(quest.TargetAmount))
        {
            CompleteQuest(quest);
        }
    }

    private void CompleteQuest(QuestDefinition quest)
    {
        var state = questStates[quest.QuestId];
        state.SetStatus(QuestStatus.Completed);

        HandleUnlocks(quest);
        HandleBlocks(quest);
    }

    private void HandleUnlocks(QuestDefinition quest)
    {
        foreach (var unlock in quest.UnlocksQuests)
        {
            if (questStates.TryGetValue(unlock.QuestId, out var state))
            {
                if (state.Status == QuestStatus.Locked)
                    state.SetStatus(QuestStatus.Available);
            }
        }
    }

    private void HandleBlocks(QuestDefinition quest)
    {
        foreach (var block in quest.BlocksQuests)
        {
            if (questStates.TryGetValue(block.QuestId, out var state))
            {
                if (state.Status == QuestStatus.Active || state.Status == QuestStatus.Available)
                {
                    state.SetStatus(QuestStatus.Failed);
                }
            }
        }
    }

    // =====================
    // SAVE / LOAD
    // =====================

    public QuestSaveData GetSaveData()
    {
        return new QuestSaveData(new List<QuestState>(questStates.Values));
    }

    public void LoadFromSaveData(QuestSaveData saveData)
    {
        questStates.Clear();

        foreach (var state in saveData.QuestStates)
        {
            questStates.Add(state.QuestId, state);
        }

        InitializeAvailableQuests();
    }
}
