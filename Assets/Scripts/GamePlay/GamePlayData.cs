using System.Collections.Generic;

[System.Serializable]
public class SessionRequestWrapper
{
    public SessionRequest request;
}

[System.Serializable]
public class SessionRequest
{
    public int quest_id;
}

[System.Serializable]
public class SpeechRequest
{
    public int quest_id;
    public int turn;
    public string audio;
}

[System.Serializable]
public class SessionStartBodyWrapper
{
    public SessionStartBody request;
}

[System.Serializable]
public class SessionStartBody
{
    public int quest_id;
}

[System.Serializable]
public class SessionResponse
{
    public bool isSuccess;
    public SessionData data;
}

[System.Serializable]
public class SessionData
{
    public string sessionId;
    public string npcDialogue;
}

[System.Serializable]
public class TurnResponse
{
    public bool isSuccess;
    public TurnData data;
}

[System.Serializable]
public class TurnData
{
    public string userText;
    public string npcDialogue;
    public string npcDialogueAudio;
    public bool isTurnPassed;
    public TurnEvaluation turnEvaluation;
    public QuestResult questResult;
}

[System.Serializable]
public class TurnEvaluation
{
    public float contextRelevance;
    public float grammarAccuracy;
    public float expressionQuality;
    public List<string> objectiveProgress;
    public bool isQuestComplete;
    public string reason;
    public List<string> betterSuggestions;
    public string recommendationReason;
}

[System.Serializable]
public class QuestResult
{
    public float averageContextRelevance;
    public float averageGrammarAccuracy;
    public float averageExpressionQuality;
    public List<string> achievedObjectives;
    public bool isQuestSuccess;
}