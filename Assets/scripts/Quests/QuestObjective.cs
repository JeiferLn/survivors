using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    public QuestType Type;

    // General
    public int RequiredAmount;

    public float RequiredTime;

    // Otros objetivos
    public string TargetId;
}
