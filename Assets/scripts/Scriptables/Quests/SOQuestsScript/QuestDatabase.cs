using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField]
    private List<QuestDefinition> quests;

    private Dictionary<string, QuestDefinition> questLookup;

    public IReadOnlyList<QuestDefinition> Quests => quests;

    public void Initialize()
    {
        questLookup = new Dictionary<string, QuestDefinition>();

        foreach (var quest in quests)
        {
            if (!questLookup.ContainsKey(quest.QuestId))
            {
                questLookup.Add(quest.QuestId, quest);
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
}
