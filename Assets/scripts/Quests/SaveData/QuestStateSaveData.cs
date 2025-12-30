using System;
using System.Collections.Generic;

public class QuestStateSaveData
{
    public string QuestId;
    public QuestStatus Status;
    public List<int> ObjectiveIndexes = new();
    public List<int> ObjectiveProgress = new();
}
