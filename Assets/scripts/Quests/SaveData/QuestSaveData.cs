using System;
using System.Collections.Generic;

[Serializable]
public class QuestSaveData
{
    public List<QuestStateEntry> States = new();
}

[Serializable]
public class QuestStateEntry
{
    public string QuestId;
    public QuestState State;
}
