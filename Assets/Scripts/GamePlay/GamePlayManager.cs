using UnityEngine;
using Newtonsoft.Json;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance { get; private set; }
    public DialoguePanelUIController dialoguePanel;
    public RecordButtonController recordButton;
    public FeedbackPopupUIController feedbackPopup;
    public ResultUIController resultUI;

    private string dialogue;
    private int stageId;
    private int questId;
    private string sessionId;
    public int turnCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        recordButton.OnRecordingCompleted -= HandleRecordingCompleted;
        recordButton.OnRecordingCompleted += HandleRecordingCompleted;
    }

    void Start()
    {
        GameSessionStart();
    }

    public void Restart()
    {
        Reset();
        GameSessionStart();
    }

    private void Reset()
    {
        stageId = 0;
        questId = 0;
        sessionId = "";
        turnCount = 0;

        if (resultUI != null)
            resultUI.gameObject.SetActive(false);
    }

    private void HandleRecordingCompleted(AudioClip recordedClip)
    {
        // TODO: 실제 API 호출

        // 더미 데이터로 테스트(turnCount에 따라 퀘스트 완료/미완료 분기)
        TurnResponse response = turnCount < 1 ? GetDummyTurnResponse_Uncomplete() : GetDummyTurnResponse_Complete();
        
        
        HandleTurnResponse(response);
    }

    private void HandleTurnResponse(TurnResponse response)
    {
        if (!response.isSuccess) return;

        TurnData data = response.data;

        // turnCount 체크
        if (data.isTurnPassed)
            turnCount++;
        else
        {
            feedbackPopup.SetFeedbackPopup(data.turnEvaluation.recommendationReason, data.turnEvaluation.betterSuggestions);
        }

        // npcDialogue 저장 후 SetQuestion 호출
        dialogue = data.npcDialogue;
        SetQuestion();

        // 퀘스트 완료 여부 분기
        if (data.turnEvaluation.isQuestComplete)
        {
            CompleteQuest(data.questResult);
        }
        else
        {
            recordButton.SetRecordActive();
        }
    }

    private void SetQuestion()
    {
        dialoguePanel.SetQuestionPanel(dialogue);
    }

    private void GameSessionStart()
    {
        // 테스트용 임시 코드
        stageId = SceneContext.SelectedStageId != 0 ? SceneContext.SelectedStageId : 1;
        questId = SceneContext.SelectedQuestId != 0 ? SceneContext.SelectedQuestId : 1;

        // TODO: 게임 시작 api 호출
        SessionResponse response = GetDummySessionResponse();

        if (response.isSuccess)
        {
            this.sessionId = response.data.sessionId;
            this.dialogue = response.data.npcDialogue;
            SetQuestion();
        }
    }

    private void CompleteQuest(QuestResult questResult)
    {
        Debug.Log("Complete Quest");

        if (resultUI != null)
        {
            resultUI.SetResultUI(questResult.averageContextRelevance, questResult.averageGrammarAccuracy, questResult.averageExpressionQuality);
        }
    }

    // 테스트용 더미 데이터 코드
    private SessionResponse GetDummySessionResponse()
    {
        string dummyJson = @"{
        ""isSuccess"": true,
        ""data"": {
            ""sessionId"": ""a1b2c3d4-5678-90ab-cdef-1234567890ab"",
            ""npcDialogue"": ""Hi there! Welcome to Bean & Brew.\nWhat can I get started for you today?""
        }
    }";

        return JsonUtility.FromJson<SessionResponse>(dummyJson);
    }

    // 퀘스트 미완료
    private TurnResponse GetDummyTurnResponse_Uncomplete()
    {
        string dummyJson = @"{
        ""isSuccess"": true,
        ""data"": {
            ""npcDialogue"": ""No worries at all! How can I help you today?"",
            ""npcDialogueAudio"": ""<base64 인코딩된 오디오>"",
            ""isTurnPassed"": true,
            ""turnEvaluation"": {
                ""contextRelevance"": 0.2,
                ""grammarAccuracy"": 1.0,
                ""expressionQuality"": 0.5,
                ""objectiveProgress"": [],
                ""isQuestComplete"": false,
                ""reason"": ""사용자의 말이 맥락과 관련이 없는 사과뿐이지만, 문법적으로 정확하다."",
                ""betterSuggestions"": [""hello"", ""hi""],
                ""recommendationReason"": ""대화를 자연스럽게 시작할 수 있는 인사말이 더 적절하다.""
            },
            ""questResult"": null
        }
    }";

        return JsonUtility.FromJson<TurnResponse>(dummyJson);
    }

    // 퀘스트 완료
    private TurnResponse GetDummyTurnResponse_Complete()
    {
        string dummyJson = @"{
        ""isSuccess"": true,
        ""data"": {
            ""npcDialogue"": ""Perfect! Your order is all set. Have a great day!"",
            ""npcDialogueAudio"": ""<base64 인코딩된 오디오>"",
            ""isTurnPassed"": true,
            ""turnEvaluation"": {
                ""contextRelevance"": 0.9,
                ""grammarAccuracy"": 0.95,
                ""expressionQuality"": 0.85,
                ""objectiveProgress"": [""최종 주문을 확인하세요""],
                ""isQuestComplete"": true,
                ""reason"": ""모든 주문 목표를 달성하고 자연스럽게 대화를 마무리했다."",
                ""betterSuggestions"": [""bye""],
                ""recommendationReason"": ""대화를 자연스럽게 끝낼 수 있는 인사말이 더 적절하다.""
            },
            ""questResult"": {
                ""averageContextRelevance"": 0.75,
                ""averageGrammarAccuracy"": 0.90,
                ""averageExpressionQuality"": 0.80,
                ""achievedObjectives"": [""음료 사이즈를 선택하세요"", ""최종 주문을 확인하세요""],
                ""isQuestSuccess"": true
            }
        }
    }";

        return JsonUtility.FromJson<TurnResponse>(dummyJson);
    }
    
}
