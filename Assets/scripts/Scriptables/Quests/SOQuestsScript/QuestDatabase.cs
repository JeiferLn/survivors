using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    [SerializeField]
    private List<QuestData> quests;

    private Dictionary<string, QuestData> questLookup;

    public IReadOnlyList<QuestData> Quests => quests;

    public void Initialize()
    {
        questLookup = new Dictionary<string, QuestData>();

        foreach (var quest in quests)
        {
            if (!questLookup.ContainsKey(quest.QuestId))
            {
                questLookup.Add(quest.QuestId, quest);
            }
        }
    }

    public QuestData GetQuestById(string questId)
    {
        if (questLookup == null)
            Initialize();

        questLookup.TryGetValue(questId, out var quest);
        return quest;
    }
}
