using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Speakat.Client;

public class VocabularyApiService : MonoBehaviour
{
    [SerializeField] private SpeakatApiProvider apiProvider;

    public async Task<VocabularyData> GetFlashcardsAsync(string cursor = null, int size = 20, long? questId = null)
    {
        if (apiProvider == null)
        {
            throw new Exception("[VocabularyApiService] apiProvider가 연결되지 않았습니다.");
        }

        Debug.Log($"[VocabularyApiService] GET /flashcards 호출: cursor={cursor}, size={size}, questId={questId}");
        Debug.Log($"[VocabularyApiService] BaseUrl={apiProvider.Client.BaseUrl}");

        var response = await apiProvider.Client.FlashcardsGETAsync(cursor, size, questId);

        if (response == null)
        {
            throw new Exception("[VocabularyApiService] response is null");
        }

        if (response.IsSuccess != true)
        {
            throw new Exception($"[VocabularyApiService] API failed: code={response.Code}, message={response.Message}");
        }

        var listData = response.Data;

        if (listData == null || listData.Items == null)
        {
            return new VocabularyData
            {
                wordList = new List<WordData>(),
                questFilters = new List<string> { "전체" },
                nextCursor = null,
                hasMore = false,
                wordsToReviewCount = 0
            };
        }

        List<WordData> words = listData.Items.Select(item =>
        {
            long flashcardId = item.FlashcardId.GetValueOrDefault();
            long itemQuestId = item.QuestId.GetValueOrDefault();
            bool isMastered = item.IsMastered.GetValueOrDefault();

            string questTitle = item.QuestTitle;
            string questName = string.IsNullOrEmpty(questTitle)
                ? $"Quest {itemQuestId}"
                : questTitle;

            return new WordData
            {
                flashcardId = flashcardId,
                word = item.Word ?? "",
                meaning = item.Meaning ?? "",
                pronunciation = item.Phonetic ?? "",
                isMastered = isMastered,
                questId = itemQuestId,
                questName = questName,
                audioUrl = null
            };
        }).ToList();

        List<QuestFilterData> questFilterDataList = new List<QuestFilterData>
        {
            new QuestFilterData("전체", null)
        };

        questFilterDataList.AddRange(
            words
                .Where(w => w.questId > 0)
                .GroupBy(w => w.questId)
                .Select(g =>
                {
                    WordData first = g.First();
                    string label = string.IsNullOrEmpty(first.questName)
                        ? $"Quest {first.questId}"
                        : first.questName;

                    return new QuestFilterData(label, first.questId);
                })
        );

        List<string> filters = questFilterDataList
            .Select(f => f.label)
            .Distinct()
            .ToList();

        return new VocabularyData
        {
            wordList = words,
            questFilters = filters,
            questFilterDataList = questFilterDataList,
            nextCursor = listData.NextCursor,
            hasMore = listData.HasMore.GetValueOrDefault(),
            wordsToReviewCount = words.Count(w => !w.isMastered)
        };
    }

    public async Task<WordData> GetFlashcardDetailAsync(long flashcardId)
    {
        if (apiProvider == null)
        {
            throw new Exception("[VocabularyApiService] apiProvider가 연결되지 않았습니다.");
        }

        var response = await apiProvider.Client.FlashcardsGET2Async(flashcardId);

        if (response == null)
        {
            throw new Exception("[VocabularyApiService] detail response is null");
        }

        if (response.IsSuccess != true)
        {
            throw new Exception($"[VocabularyApiService] Detail API failed: code={response.Code}, message={response.Message}");
        }

        var data = response.Data;

        if (data == null)
        {
            return null;
        }

        return new WordData
        {
            flashcardId = data.FlashcardId.GetValueOrDefault(),
            word = data.Word ?? "",
            meaning = data.Meaning ?? "",
            pronunciation = data.Phonetic ?? "",
            audioUrl = data.AudioUrl,
            isMastered = data.IsMastered.GetValueOrDefault()
        };
    }

    public async Task<bool> SetMasteredAsync(long flashcardId, bool isMastered)
    {
        if (apiProvider == null)
        {
            throw new Exception("[VocabularyApiService] apiProvider가 연결되지 않았습니다.");
        }

        var body = new PatchFlashcardRequestDto
        {
            IsMastered = isMastered
        };

        var response = await apiProvider.Client.FlashcardsPATCHAsync(flashcardId, body);

        if (response == null)
        {
            throw new Exception("[VocabularyApiService] SetMastered response is null");
        }

        if (response.IsSuccess != true || response.Data == null)
        {
            throw new Exception($"[VocabularyApiService] SetMastered failed: code={response.Code}, message={response.Message}");
        }

        return response.Data.IsMastered.GetValueOrDefault();
    }
}