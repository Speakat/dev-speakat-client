using System.Collections.Generic;

[System.Serializable]
public class StageData
{
    public bool isSuccess;
    public StageList data;
}

[System.Serializable]
public class StageList
{
    public List<StageItem> items;
}

[System.Serializable]
public class StageItem
{
    public int stageId;
    public string title;
    public string description;
    public string thumbnailUrl;
    public string status;
    public int questCount;
    public int completedQuestCount;
}

[System.Serializable]
public class StageDetailResponse
{
    public bool isSuccess;
    public StageDetailData data;
    public string code;
    public string message;
}

[System.Serializable]
public class StageDetailData
{
    public int stageId;
    public string title;
    public string description;
    public string status;
    public List<QuestItem> quests;
}

[System.Serializable]
public class QuestItem
{
    public int questId;
    public string title;
    public string description;
    public int sortOrder;
    public bool isCompleted;
    public int attemptCount;
}