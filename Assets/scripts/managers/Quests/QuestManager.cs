using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField]
    private QuestDatabase questDatabase;

    private Dictionary<string, QuestState> questStates = new();
    private Dictionary<QuestType, IQuestObjectiveHandler> handlers = new();

    private void Awake()
    {
        RegisterHandlers();
        InitializeQuests();
    }

    private void Update()
    {
        DispatchEvent(QuestType.SurviveTime, Time.deltaTime);
    }

    private void RegisterHandlers()
    {
        Register(new CollectItemHandler());
        Register(new CraftItemHandler());
        Register(new ReachZoneHandler());
        Register(new TalkNpcHandler());
        Register(new SurviveTimeHandler());
    }

    private void Register(IQuestObjectiveHandler handler)
    {
        handlers.Add(handler.ObjectiveType, handler);
    }

    private void InitializeQuests()
    {
        foreach (var quest in questDatabase.Quests)
            questStates[quest.QuestId] = new QuestState();

        TryActivateAvailableQuests();
    }

    private void TryActivateAvailableQuests()
    {
        foreach (var quest in questDatabase.Quests)
        {
            var state = questStates[quest.QuestId];
            if (state.Status != QuestStatus.Locked)
                continue;

            if (AreRequirementsCompleted(quest))
                ActivateQuest(quest);
        }
    }

    private bool AreRequirementsCompleted(QuestDefinition quest)
    {
        foreach (var req in quest.RequiredQuests)
            if (questStates[req.QuestId].Status != QuestStatus.Completed)
                return false;

        return true;
    }

    private void ActivateQuest(QuestDefinition quest)
    {
        questStates[quest.QuestId].SetActive();
    }

    internal void CompleteQuestInternal(QuestDefinition quest)
    {
        var state = questStates[quest.QuestId];
        if (state.Status == QuestStatus.Completed)
            return;

        state.SetCompleted();
        HandleUnlocksAndBlocks(quest);
        TryActivateAvailableQuests();
    }

    private void HandleUnlocksAndBlocks(QuestDefinition quest)
    {
        foreach (var unlock in quest.UnlocksQuests)
            if (questStates[unlock.QuestId].Status == QuestStatus.Locked)
                ActivateQuest(unlock);

        foreach (var block in quest.BlocksQuests)
            questStates[block.QuestId].SetBlocked();
    }

    private void DispatchEvent(QuestType type, object data)
    {
        if (!handlers.TryGetValue(type, out var handler))
            return;

        foreach (var quest in questDatabase.Quests)
        {
            var state = questStates[quest.QuestId];
            if (state.Status != QuestStatus.Active)
                continue;

            handler.Process(this, quest, data);
        }
    }

    public QuestStateSaveData GetSaveData()
    {
        var saveData = new QuestStateSaveData();

        foreach (var pair in questStates)
        {
            saveData.QuestId = pair.Key;
            saveData.Status = pair.Value.Status;
            saveData.Progress = pair.Value.CurrentProgress;
        }

        return saveData;
    }

    public void LoadFromSaveData(QuestStateSaveData saveData)
    {
        if (!questStates.TryGetValue(saveData.QuestId, out var state))
            return;

        state.Load(saveData.Progress, saveData.Status);
    }

    public void OnItemCollected(string itemId, int amount)
    {
        DispatchEvent(QuestType.CollectItem, new ItemEventData(itemId, amount));
    }

    public void OnItemCrafted(string itemId, int amount)
    {
        DispatchEvent(QuestType.CraftItem, new ItemEventData(itemId, amount));
    }

    public void OnZoneReached(string zoneId)
    {
        DispatchEvent(QuestType.ReachZone, zoneId);
    }

    public void OnNpcTalked(string npcId)
    {
        DispatchEvent(QuestType.TalkToNPC, npcId);
    }

    public QuestState GetQuestState(string questId)
    {
        return questStates.TryGetValue(questId, out var state) ? state : null;
    }
}
