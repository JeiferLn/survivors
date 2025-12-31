using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    [Header("══════════ MAIN QUESTS ══════════")]
    [SerializeField]
    private List<QuestDefinition> mainCountdown = new();

    [SerializeField]
    private List<QuestDefinition> mainReachZone = new();

    [SerializeField]
    private List<QuestDefinition> mainTalkToNPC = new();

    [SerializeField]
    private List<QuestDefinition> mainCollectItem = new();

    [SerializeField]
    private List<QuestDefinition> mainCraftItem = new();

    [Space(15)]
    [Header("══════════ SIDE QUESTS ══════════")]
    [SerializeField]
    private List<QuestDefinition> sideCountdown = new();

    [SerializeField]
    private List<QuestDefinition> sideReachZone = new();

    [SerializeField]
    private List<QuestDefinition> sideTalkToNPC = new();

    [SerializeField]
    private List<QuestDefinition> sideCollectItem = new();

    [SerializeField]
    private List<QuestDefinition> sideCraftItem = new();

    private Dictionary<string, QuestDefinition> questLookup;
    private List<QuestDefinition> combinedQuests;

    public IReadOnlyList<QuestDefinition> Quests
    {
        get
        {
            if (combinedQuests == null)
            {
                BuildCombinedList();
            }
            return combinedQuests;
        }
    }

    private void BuildCombinedList()
    {
        combinedQuests = new List<QuestDefinition>();

        combinedQuests.AddRange(mainCountdown);
        combinedQuests.AddRange(mainReachZone);
        combinedQuests.AddRange(mainTalkToNPC);
        combinedQuests.AddRange(mainCollectItem);
        combinedQuests.AddRange(mainCraftItem);

        combinedQuests.AddRange(sideCountdown);
        combinedQuests.AddRange(sideReachZone);
        combinedQuests.AddRange(sideTalkToNPC);
        combinedQuests.AddRange(sideCollectItem);
        combinedQuests.AddRange(sideCraftItem);
    }

    public void Initialize()
    {
        BuildCombinedList();
        questLookup = new Dictionary<string, QuestDefinition>();

        foreach (var quest in combinedQuests)
        {
            if (quest != null && !string.IsNullOrEmpty(quest.QuestId))
            {
                if (!questLookup.ContainsKey(quest.QuestId))
                {
                    questLookup.Add(quest.QuestId, quest);
                }
            }
        }
    }

    public QuestDefinition GetQuestById(string questId)
    {
        if (questLookup == null)
            Initialize();

        questLookup.TryGetValue(questId, out var quest);
        return quest;
    }

    private void OnValidate()
    {
        BuildCombinedList();
    }
}
