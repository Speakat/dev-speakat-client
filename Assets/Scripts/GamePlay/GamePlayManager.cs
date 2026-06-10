using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance { get; private set; }
    public DialoguePanelUIController dialoguePanel;
    public RecordButtonController recordButton;
    public FeedbackPopupUIController feedbackPopup;
    public ResultUIController resultUI;
    public NpcAudioController npcAudioController;
    public FailurePopupUIController failurePopup;
    public LogPanelUIController logPanel;

    private string dialogue;
    private int stageId;
    private int questId = 1;
    private string sessionId;
    public int turnCount = 0;

    private const string SessionEndpoint = "/sessions";
    private const string SpeechEndpoint = "/sessions/{0}/speech";
    private const string SessionEndEndpoint = "/sessions/{0}/end";

    SessionResponse sessionResponse;

    private bool isCompleted = false;
    public bool isAudioPlaying = false;
    private bool isSubmittingSpeech = false;

    [SerializeField] private StageReactionController stageReactionController;

    QuestResult questResult;

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
    }

    [SerializeField] private bool startSessionOnSceneStart = false;

    // NPC 클릭 - 플레이어 이동 - 카메라 전환 후 세션 시작되도록 수정
    private async void Start()
    {
        if (startSessionOnSceneStart)
            await GameSessionStartAsync();
    }

    // NPC 도착 후 호출
    public async void StartQuestSessionFromNpc()
    {
        await GameSessionStartAsync();
    }

    public async void Restart()
    {
        ResetState();
        await GameSessionStartAsync();
    }

    private void ResetState()
    {
        stageId = 0;
        questId = 0;
        sessionId = "";
        turnCount = 0;
        isCompleted = false;
        isAudioPlaying = false;

        if (resultUI != null)
            resultUI.gameObject.SetActive(false);

        logPanel.ClearLogs();
    }

    public void WaitForAudioClipPlay(float clipLength)
    {
        StartCoroutine(CoWaitForAudioClipPlay(clipLength));
    }

    private IEnumerator CoWaitForAudioClipPlay(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);

        isAudioPlaying = false;

        if (isCompleted)
        {
            if (recordButton != null)
                recordButton.SetRecordInactive();

            SetResultUI(questResult);
        }
        else
        {
            if (recordButton != null)
                recordButton.SetRecordActive();
        }
    }

    public async void HandleRecordingCompletedWithBase64(string base64Wav)
    {
        if (isCompleted)
        {
            Debug.LogWarning("[GamePlayManager] 이미 완료된 세션이라 음성 제출을 무시합니다.");
            return;
        }

        if (isSubmittingSpeech)
        {
            Debug.LogWarning("[GamePlayManager] 이미 음성 제출 중이라 중복 제출을 무시합니다.");
            return;
        }

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            Debug.LogError("[GamePlayManager] sessionId가 없어 음성 제출을 중단합니다. 세션 시작 성공 여부를 확인하세요.");

            if (recordButton != null)
                recordButton.SetRecordActive();

            return;
        }

        try
        {
            isSubmittingSpeech = true;

            Debug.Log($"[GamePlayManager] 음성 제출 시작, base64Length={base64Wav?.Length}");

            string json = await PostSpeechAsync(base64Wav);

            Debug.Log($"[GamePlayManager] speech response={json}");

            TurnResponse response = JsonUtility.FromJson<TurnResponse>(json);
            HandleTurnResponse(response);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GamePlayManager] speech 제출 실패: {e.Message}");

            if (!isCompleted && recordButton != null)
                recordButton.SetRecordActive();
        }
        finally
        {
            isSubmittingSpeech = false;
        }
    }

    private void HandleTurnResponse(TurnResponse response)
    {
        if (!response.isSuccess) return;

        TurnData data = response.data;

        Debug.Log($"[TurnEvaluation] userText='{data.userText}', npcDialogue='{data.npcDialogue}'");
        Debug.Log($"[TurnEvaluation] isTurnPassed: {data.isTurnPassed}, isQuestComplete: {data.turnEvaluation.isQuestComplete}");

        string progress = data.turnEvaluation.objectiveProgress != null
            ? string.Join(", ", data.turnEvaluation.objectiveProgress)
            : "";

        string achieved = data.questResult != null && data.questResult.achievedObjectives != null
            ? string.Join(", ", data.questResult.achievedObjectives)
            : "";

        Debug.Log($"[TurnEvaluation] progressObject: {progress}, achievedObjectives: {achieved}");

        // 실패 발화든 성공 발화든 사용자가 말한 문장은 먼저 표시
        if (!string.IsNullOrWhiteSpace(data.userText))
        {
            SetAnswer(data.userText);
        }

        // 퀘스트 완료를 먼저 처리
        if (data.turnEvaluation.isQuestComplete)
        {
            Debug.Log("[GamePlayManager] 퀘스트 완료!");

            CompleteQuest();
            questResult = data.questResult;

            if (recordButton != null)
                recordButton.SetRecordInactive();

            if (stageReactionController != null && questResult != null)
            {
                if (questResult.isQuestSuccess)
                    stageReactionController.PlayQuestSuccessReaction();
                else
                    stageReactionController.PlayQuestFailReaction();
            }

            // 마지막 NPC 대사가 있으면 표시
            if (!string.IsNullOrWhiteSpace(data.npcDialogue))
            {
                SetQuestion(data.npcDialogue);
            }

            // 마지막 NPC 오디오가 있으면 재생
            if (!string.IsNullOrWhiteSpace(data.npcDialogueAudio) && npcAudioController != null)
            {
                npcAudioController.StartCoroutine(npcAudioController.PlayAudioFromBase64(data.npcDialogueAudio));
            }
            else
            {
                SetResultUI(questResult);
            }

            return;
        }

        // 아직 퀘스트가 끝나지 않은 턴만 처리
        if (data.isTurnPassed)
        {
            turnCount++;

            if (stageReactionController != null)
                stageReactionController.PlayTurnPassedReaction();

            if (!string.IsNullOrWhiteSpace(data.npcDialogueAudio) && npcAudioController != null)
                npcAudioController.StartCoroutine(npcAudioController.PlayAudioFromBase64(data.npcDialogueAudio));

            if (!string.IsNullOrWhiteSpace(data.npcDialogue))
                SetQuestion(data.npcDialogue);
        }
        else if (data.turnEvaluation.betterSuggestions == null || data.turnEvaluation.betterSuggestions.Count == 0)
        {
            if (stageReactionController != null)
                stageReactionController.PlayTurnFailedReaction();

            failurePopup.ShowPopup();
            recordButton.SetRecordActive();
        }
        else
        {
            if (stageReactionController != null)
                stageReactionController.PlayTurnFailedReaction();

            feedbackPopup.SetFeedbackPopup(data.turnEvaluation.recommendationReason, data.turnEvaluation.betterSuggestions);
            recordButton.SetRecordActive();
        }
    }

    private void SetQuestion(string question)
    {
        dialoguePanel.SetQuestionPanel(question);
        logPanel.AddLog(LogType.Question, question);
    }

    private void SetAnswer(string answer)
    {
        dialoguePanel.SetAnswerPanel(answer);
        logPanel.AddLog(LogType.Answer, answer);
    }

    private async Task GameSessionStartAsync()
    {
        stageId = SceneContext.SelectedStageId != 0 ? SceneContext.SelectedStageId : 1;
        questId = SceneContext.SelectedQuestId != 0 ? SceneContext.SelectedQuestId : 1;

        dialoguePanel.ClearPanels();
        //recordButton.SetRecordInactive();

        Debug.Log($"[GamePlayManager] 세션 시작: stageId={stageId}, questId={questId}");

        try
        {
            string json = await PostSessionAsync(questId);
            SessionResponse response = JsonUtility.FromJson<SessionResponse>(json);

            if (response.isSuccess)
            {
                sessionId = response.data.sessionId;
                dialogue = response.data.npcDialogue;

                Debug.Log($"[GamePlayManager] 세션 시작 성공: sessionId={sessionId}, questId={questId}");

                SetQuestion(response.data.npcDialogue);

                if (recordButton != null)
                    recordButton.SetRecordActive();
            }
            else
            {
                Debug.LogError("[GamePlayManager] 세션 시작 실패 응답");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GamePlayManager] 세션 시작 실패: {e.Message}");
        }
    }

    private void CompleteQuest()
    {
        isCompleted = true;
    }

    private void SetResultUI(QuestResult result)
    {
        if (resultUI != null)
            resultUI.SetResultUI(
                result.averageContextRelevance,
                result.averageGrammarAccuracy,
                result.averageExpressionQuality,
                result.isQuestSuccess);
    }

    private async Task<string> PostSessionAsync(int questId)
    {
        string url = "https://speakat.hyorim.shop" + SessionEndpoint;
        string body = JsonUtility.ToJson(new SessionRequest { quest_id = questId });

        Debug.Log($"[GamePlayManager] PostSession url={url}, body={body}");

        return await PostAsync(url, body);
    }

    private async Task<string> PostSpeechAsync(string base64Wav)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new System.Exception("[GamePlayManager] sessionId가 비어 있어 speech 요청을 보낼 수 없습니다.");
        }

        string url = "https://speakat.hyorim.shop" + string.Format(SpeechEndpoint, sessionId);
        string body = JsonUtility.ToJson(new SpeechRequest
        {
            quest_id = questId,
            turn = turnCount + 1,
            audio = base64Wav
        });

        Debug.Log($"[GamePlayManager] PostSpeech url={url}");
        Debug.Log($"[GamePlayManager] PostSpeech sessionId={sessionId}, questId={questId}, turn={turnCount + 1}");

        return await PostAsync(url, body);
    }

    public async Task EndSessionAsync()
    {
        try
        {
            string url = "https://speakat.hyorim.shop" + string.Format(SessionEndEndpoint, sessionId);
            string json = await PostAsync(url, "{}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GamePlayManager] 세션 종료 실패: {e.Message}");
        }
    }

    private Task<string> PostAsync(string url, string bodyJson)
    {
        var tcs = new TaskCompletionSource<string>();
        StartCoroutine(PostCoroutine(url, bodyJson, tcs));
        return tcs.Task;
    }

    private IEnumerator PostCoroutine(string url, string bodyJson, TaskCompletionSource<string> tcs)
    {
        string token = TokenStore.Instance.AccessToken.Trim();
        byte[] bodyBytes = Encoding.UTF8.GetBytes(bodyJson);

        using UnityWebRequest req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(bodyBytes);
        req.uploadHandler.contentType = "application/json";
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Authorization", $"Bearer {token}");
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            tcs.SetResult(req.downloadHandler.text);
            recordButton.SetRecordActive();
        }
        else
            tcs.SetException(new System.Exception($"[{req.responseCode}] {req.error} — {req.downloadHandler.text}"));
    }

    // 더미 폴백용 — 실제 호출 금지
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

    // 더미 폴백용 — 실제 호출 금지
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

    // 더미 폴백용 — 실제 호출 금지
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