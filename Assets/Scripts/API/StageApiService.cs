using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Speakat.Client;
using UnityEngine;

public class StageApiService : MonoBehaviour
{
    [SerializeField] private SpeakatApiProvider apiProvider;

    private SpeakatClient Client
    {
        get
        {
            if (apiProvider == null)
            {
                throw new InvalidOperationException("[StageApiService] apiProvider is not assigned.");
            }

            return apiProvider.Client;
        }
    }

    public async Task<StageList> GetStageListAsync()
    {
        ApiResponseOfStageListDto response = await Client.StagesAsync();

        if (response == null)
        {
            throw new InvalidOperationException("[StageApiService] response is null.");
        }

        if (response.IsSuccess != true)
        {
            throw new InvalidOperationException(
                $"[StageApiService] API failed: code={response.Code}, message={response.Message}");
        }

        if (response.Data == null)
        {
            throw new InvalidOperationException("[StageApiService] response data is null.");
        }

        IEnumerable<StageItemDto> items = response.Data.Items ?? Array.Empty<StageItemDto>();
        return new StageList { items = ToStageItems(items) };
    }

    private static List<StageItem> ToStageItems(IEnumerable<StageItemDto> items)
    {
        var result = new List<StageItem>();

        foreach (StageItemDto item in items)
        {
            try
            {
                result.Add(ToStageItem(item));
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning($"[StageApiService] Skipping invalid stage item: {ex.Message}");
            }
        }

        return result;
    }

    private static StageItem ToStageItem(StageItemDto item)
    {
        if (item == null)
        {
            throw new InvalidOperationException("[StageApiService] stage item is null.");
        }

        if (!item.StageId.HasValue || item.StageId.Value <= 0 || item.StageId.Value > int.MaxValue)
        {
            throw new InvalidOperationException($"[StageApiService] invalid stageId={item.StageId}");
        }

        StageStatus status;

        if (item.Status.HasValue && Enum.IsDefined(typeof(StageStatus), item.Status.Value))
        {
            status = (StageStatus)item.Status.Value;
        }
        else
        {
            status = StageStatus.Locked;
            Debug.LogWarning(
                $"[StageApiService] Unknown status={item.Status} for stageId={item.StageId}, defaulting to Locked.");
        }

        return new StageItem
        {
            stageId = (int)item.StageId.Value,
            title = item.Title ?? string.Empty,
            description = item.Description ?? string.Empty,
            status = status,
            questCount = item.QuestCount.GetValueOrDefault(),
            completedQuestCount = item.CompletedQuestCount.GetValueOrDefault()
        };
    }
}
