using UnityEngine;
using Newtonsoft.Json;

public class GamePlayManager : MonoBehaviour
{
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
