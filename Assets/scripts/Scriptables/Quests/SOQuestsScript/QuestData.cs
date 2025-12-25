using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private string questId;

    [SerializeField]
    private string questName;

    [TextArea]
    [SerializeField]
    private string description;

    [Header("Classification")]
    [SerializeField]
    private QuestCategory category;

    [SerializeField]
    private QuestType questType;

    [Header("Objective")]
    [SerializeField]
    private int targetAmount;

    [Header("Dependencies")]
    [SerializeField]
    private List<QuestDefinition> requiredQuests;

    [SerializeField]
    private List<QuestDefinition> unlocksQuests;

    [SerializeField]
    private List<QuestDefinition> blocksQuests;

    // Getters (solo lectura)
    public string QuestId => questId;
    public string QuestName => questName;
    public string Description => description;
    public QuestCategory Category => category;
    public QuestType QuestType => questType;
    public int TargetAmount => targetAmount;

    public IReadOnlyList<QuestDefinition> RequiredQuests => requiredQuests;
    public IReadOnlyList<QuestDefinition> UnlocksQuests => unlocksQuests;
    public IReadOnlyList<QuestDefinition> BlocksQuests => blocksQuests;
}
