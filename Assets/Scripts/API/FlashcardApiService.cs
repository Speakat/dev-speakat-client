using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class FlashcardApiService : MonoBehaviour
{
    [SerializeField] private SpeakatApiProvider apiProvider;

    public async Task SaveAsync(int questId, string word, string recommendationReason)
    {
        if (apiProvider == null)
        {
            throw new InvalidOperationException("[FlashcardApiService] apiProvider is not assigned.");
        }

        if (questId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(questId));
        }

        if (string.IsNullOrWhiteSpace(word))
        {
            throw new ArgumentException("word is required.", nameof(word));
        }

        string url = apiProvider.ApiBaseUrl.TrimEnd('/') + "/flashcards";
        string json = JsonConvert.SerializeObject(new FlashcardRequest
        {
            questId = questId,
            word = word.Trim(),
            recommendationReason = recommendationReason ?? string.Empty
        });

        using (var request = new HttpRequestMessage(HttpMethod.Post, url))
        {
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await apiProvider.SendAsync(request))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"[FlashcardApiService] save failed: status={(int)response.StatusCode}");
                }
            }
        }
    }
}
