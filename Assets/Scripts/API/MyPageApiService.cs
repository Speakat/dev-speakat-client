using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;
using Speakat.Client;
using Newtonsoft.Json;

public class MyPageApiService : MonoBehaviour
{
    [SerializeField] private SpeakatApiProvider apiProvider;

    private SpeakatClient Client
    {
        get
        {
            if (apiProvider == null)
                throw new Exception("[MyPageApiService] apiProvider가 연결되지 않았습니다.");

            return apiProvider.Client;
        }
    }

    public async Task<MyProfileData> GetMyProfileAsync()
    {
        if (apiProvider == null)
            throw new Exception("[MyPageApiService] apiProvider가 연결되지 않았습니다.");

        string baseUrl = apiProvider.ApiBaseUrl;
        string url = baseUrl.TrimEnd('/') + "/users/me";

        using (HttpClient httpClient = new HttpClient())
        {
            string accessToken = apiProvider.AccessTokenForRequest;

            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken.Trim());
            }

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            string json = await httpClient.GetStringAsync(url);

            var response = JsonConvert.DeserializeObject<ApiResponseOfMyProfileData>(json);

            if (response == null || response.isSuccess != true || response.data == null)
            {
                throw new Exception(
                    $"[MyPageApiService] GetMyProfile failed: code={response?.code}, message={response?.message}"
                );
            }

            return response.data;
        }
    }

    public async Task<UserSettingsDto> GetSettingsAsync()
    {
        var response = await Client.SettingsAsync();

        if (response == null || response.IsSuccess != true || response.Data == null)
            throw new Exception($"[MyPageApiService] GetSettings failed: code={response?.Code}, message={response?.Message}");

        return response.Data;
    }

    public async Task<MyStatsData> GetStatsAsync()
    {
        if (apiProvider == null)
            throw new Exception("[MyPageApiService] apiProvider가 연결되지 않았습니다.");

        string baseUrl = apiProvider.ApiBaseUrl;
        string url = baseUrl.TrimEnd('/') + "/users/me/stats";

        using (HttpClient httpClient = new HttpClient())
        {
            string accessToken = apiProvider.AccessTokenForRequest;

            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken.Trim());
            }

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            string json = await httpClient.GetStringAsync(url);

            Debug.Log($"[MyPageApiService] GET /users/me/stats response={json}");

            var response = JsonConvert.DeserializeObject<ApiResponseOfMyStatsData>(json);

            if (response == null || response.isSuccess != true || response.data == null)
            {
                throw new Exception(
                    $"[MyPageApiService] GetStats failed: code={response?.code}, message={response?.message}"
                );
            }

            return response.data;
        }
    }

    public async Task<UserStreakDto> GetStreakAsync()
    {
        var response = await Client.StreakAsync();

        if (response == null || response.IsSuccess != true || response.Data == null)
            throw new Exception($"[MyPageApiService] GetStreak failed: code={response?.Code}, message={response?.Message}");

        return response.Data;
    }

    public async Task<UserCalendarDto> GetCalendarAsync(int year, int month)
    {
        var response = await Client.CalendarAsync(year, month);

        if (response == null || response.IsSuccess != true || response.Data == null)
            throw new Exception($"[MyPageApiService] GetCalendar failed: code={response?.Code}, message={response?.Message}");

        return response.Data;
    }

    public async Task<PatchUserResultDto> UpdateProfileAsync(string nickname, string profileImageKey = null)
    {
        var body = new PatchUserRequestDto
        {
            Nickname = string.IsNullOrWhiteSpace(nickname) ? null : nickname.Trim(),
            ProfileImageKey = string.IsNullOrWhiteSpace(profileImageKey) ? null : profileImageKey.Trim()
        };

        var response = await Client.MePATCHAsync(body);

        if (response == null || response.IsSuccess != true || response.Data == null)
            throw new Exception($"[MyPageApiService] UpdateProfile failed: code={response?.Code}, message={response?.Message}");

        return response.Data;
    }

    public async Task DeleteMeAsync()
    {
        await Client.MeDELETEAsync();
    }
}