using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Speakat.Client;
using UnityEngine;

public class GamePlayApiService : MonoBehaviour
{
    [SerializeField] private SpeakatApiProvider apiProvider;

    private SpeakatClient Client
    {
        get
        {
            if (apiProvider == null)
            {
                throw new InvalidOperationException("[GamePlayApiService] apiProvider is not assigned.");
            }

            return apiProvider.Client;
        }
    }

    public async Task<SessionData> StartSessionAsync(int questId)
    {
        if (questId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(questId));
        }

        ApiResponseOfCreateSessionResponseDto response = await Client.SessionsAsync(
            new CreateSessionRequestDto { Quest_id = questId });

        if (response == null || response.IsSuccess != true || response.Data == null)
        {
            throw new InvalidOperationException(
                $"[GamePlayApiService] start session failed: code={response?.Code}, message={response?.Message}");
        }

        if (string.IsNullOrWhiteSpace(response.Data.SessionId))
        {
            throw new InvalidOperationException("[GamePlayApiService] sessionId is empty.");
        }

        return new SessionData
        {
            sessionId = response.Data.SessionId,
            npcDialogue = response.Data.NpcDialogue ?? string.Empty
        };
    }

    public async Task<TurnData> SubmitSpeechAsync(
        string sessionId,
        int questId,
        int turn,
        string base64Audio)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("sessionId is required.", nameof(sessionId));
        }

        if (questId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(questId));
        }

        if (turn <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(turn));
        }

        if (string.IsNullOrWhiteSpace(base64Audio))
        {
            throw new ArgumentException("base64Audio is required.", nameof(base64Audio));
        }

        ApiResponseOfEvaluateResponseDto response = await Client.SpeechAsync(
            sessionId,
            new EvaluateRequestDto
            {
                Quest_id = questId,
                Turn = turn,
                Audio = base64Audio
            });

        if (response == null || response.IsSuccess != true || response.Data == null)
        {
            throw new InvalidOperationException(
                $"[GamePlayApiService] submit speech failed: code={response?.Code}, message={response?.Message}");
        }

        if (response.Data.TurnEvaluation == null)
        {
            throw new InvalidOperationException("[GamePlayApiService] turnEvaluation is null.");
        }

        return ToTurnData(response.Data);
    }

    public async Task EndSessionAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("sessionId is required.", nameof(sessionId));
        }

        ApiResponseOfEndSessionResponseDto response = await Client.EndAsync(sessionId);
        if (response == null || response.IsSuccess != true)
        {
            throw new InvalidOperationException(
                $"[GamePlayApiService] end session failed: code={response?.Code}, message={response?.Message}");
        }
    }

    private static TurnData ToTurnData(EvaluateResponseDto data)
    {
        TurnEvaluationDto evaluation = data.TurnEvaluation;

        return new TurnData
        {
            userText = ReadAdditionalString(data.AdditionalProperties, "userText"),
            npcDialogue = data.NpcDialogue ?? string.Empty,
            npcDialogueAudio = data.NpcDialogueAudio ?? string.Empty,
            isTurnPassed = data.IsTurnPassed,
            turnEvaluation = new TurnEvaluation
            {
                contextRelevance = evaluation.ContextRelevance,
                grammarAccuracy = evaluation.GrammarAccuracy,
                expressionQuality = evaluation.ExpressionQuality,
                objectiveProgress = ToList(evaluation.ObjectiveProgress),
                isQuestComplete = evaluation.IsQuestComplete,
                reason = evaluation.Reason ?? string.Empty,
                betterSuggestions = ToList(evaluation.BetterSuggestions),
                recommendationReason = evaluation.RecommendationReason ?? string.Empty
            },
            questResult = ToQuestResult(data.QuestResult)
        };
    }

    private static QuestResult ToQuestResult(QuestResultDto result)
    {
        if (result == null)
        {
            return null;
        }

        return new QuestResult
        {
            averageContextRelevance = result.AverageContextRelevance,
            averageGrammarAccuracy = result.AverageGrammarAccuracy,
            averageExpressionQuality = result.AverageExpressionQuality,
            achievedObjectives = ToList(result.AchievedObjectives),
            isQuestSuccess = result.IsQuestSuccess
        };
    }

    private static List<string> ToList(IEnumerable<string> values)
    {
        return values?.Where(value => value != null).ToList() ?? new List<string>();
    }

    private static string ReadAdditionalString(IDictionary<string, object> values, string key)
    {
        if (values == null || !values.TryGetValue(key, out object value) || value == null)
        {
            return string.Empty;
        }

        return value is JValue token ? token.Value<string>() ?? string.Empty : value.ToString();
    }
}
