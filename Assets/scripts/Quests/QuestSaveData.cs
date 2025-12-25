using System.Collections.Generic;
using UnityEngine;

public class QuestSaveData
{
    [SerializeField]
    private List<QuestState> questStates;

    public List<QuestState> QuestStates => questStates;

    public QuestSaveData(List<QuestState> questStates)
    {
        this.questStates = questStates;
    }
}
