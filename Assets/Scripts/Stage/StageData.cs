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