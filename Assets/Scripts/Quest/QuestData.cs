using System.Collections.Generic;

[System.Serializable]
public class QuestDetailResponse
{
    public bool isSuccess;
    public QuestDetailData data;
}

[System.Serializable]
public class QuestDetailData
{
    public int questId;
    public int stageId;
    public string title;
    public string description;
    public string thumbnailUrl;
    public List<string> objectives;
    public string status;
    public int bestScore;
    public int attemptCount;
}