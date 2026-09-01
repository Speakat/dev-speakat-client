using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Speakat.Client;
using UnityEngine;

public class QuestApiService : MonoBehaviour
{
    [SerializeField] private SpeakatApiProvider apiProvider;

    private SpeakatClient Client
    {
        get
        {
            if (apiProvider == null)
            {
                throw new InvalidOperationException("[QuestApiService] apiProvider is not assigned.");
            }

            return apiProvider.Client;
        }
    }

    public async Task<StageDetailData> GetStageDetailAsync(int stageId)
    {
        if (stageId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stageId));
        }

        ApiResponseOfStageDetailDto response = await Client.Stages2Async(stageId);
        StageDetailDto data = RequireData(response, "stage detail");

        return new StageDetailData
        {
            stageId = ToPositiveInt(data.StageId, "stageId"),
            title = data.Title ?? string.Empty,
            description = data.Description ?? string.Empty,
            status = ToStageStatusString(data.Status),
            quests = ToQuestItems(data.Quests)
        };
    }

    public async Task<QuestDetailData> GetQuestDetailAsync(int questId)
    {
        if (questId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(questId));
        }

        ApiResponseOfQuestDetailDto response = await Client.QuestsAsync(questId);
        QuestDetailDto data = RequireData(response, "quest detail");

        return new QuestDetailData
        {
            questId = ToPositiveInt(data.QuestId, "questId"),
            stageId = ToPositiveInt(data.StageId, "stageId"),
            title = data.Title ?? string.Empty,
            description = data.Description ?? string.Empty,
            thumbnailUrl = data.ThumbnailUrl ?? string.Empty,
            objectives = (data.Objectives ?? Array.Empty<string>()).ToList(),
            status = ToStageStatusString(data.Status),
            bestScore = data.BestScore.GetValueOrDefault(),
            attemptCount = data.AttemptCount.GetValueOrDefault()
        };
    }

    private static StageDetailDto RequireData(ApiResponseOfStageDetailDto response, string operation)
    {
        if (response == null || response.IsSuccess != true || response.Data == null)
        {
            throw new InvalidOperationException(
                $"[QuestApiService] {operation} failed: code={response?.Code}, message={response?.Message}");
        }

        return response.Data;
    }

    private static QuestDetailDto RequireData(ApiResponseOfQuestDetailDto response, string operation)
    {
        if (response == null || response.IsSuccess != true || response.Data == null)
        {
            throw new InvalidOperationException(
                $"[QuestApiService] {operation} failed: code={response?.Code}, message={response?.Message}");
        }

        return response.Data;
    }

    private static List<QuestItem> ToQuestItems(ICollection<QuestItemDto> items)
    {
        var result = new List<QuestItem>();

        foreach (QuestItemDto item in items ?? Array.Empty<QuestItemDto>())
        {
            try
            {
                result.Add(ToQuestItem(item));
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning($"[QuestApiService] Skipping invalid quest item: {ex.Message}");
            }
        }

        return result;
    }

    private static QuestItem ToQuestItem(QuestItemDto item)
    {
        if (item == null)
        {
            throw new InvalidOperationException("[QuestApiService] quest item is null.");
        }

        return new QuestItem
        {
            questId = ToPositiveInt(item.QuestId, "questId"),
            title = item.Title ?? string.Empty,
            description = item.Description ?? string.Empty,
            sortOrder = item.SortOrder.GetValueOrDefault(),
            isCompleted = item.IsCompleted.GetValueOrDefault(),
            attemptCount = item.AttemptCount.GetValueOrDefault()
        };
    }

    private static string ToStageStatusString(int? value)
    {
        if (value.HasValue && Enum.IsDefined(typeof(StageStatus), value.Value))
        {
            return ((StageStatus)value.Value).ToString();
        }

        return value?.ToString() ?? string.Empty;
    }

    private static int ToPositiveInt(long? value, string fieldName)
    {
        if (!value.HasValue || value.Value <= 0 || value.Value > int.MaxValue)
        {
            throw new InvalidOperationException($"[QuestApiService] invalid {fieldName}={value}");
        }

        return (int)value.Value;
    }
}
