using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Speakat.Client;
using Newtonsoft.Json;

public class MyPageApiService : MonoBehaviour
{
    [SerializeField] private SpeakatApiProvider apiProvider;

    [System.Serializable]
    public class UpdateSettingsRequest
    {
        public bool? showNpcScript;
    }

    [System.Serializable]
    public class MySettingsData
    {
        public bool? showNpcScript;
    }

    [System.Serializable]
    public class ApiResponseOfMySettingsData
    {
        public bool? isSuccess;
        public MySettingsData data;
        public string code;
        public string message;
    }

    [System.Serializable]
    public class CheckNicknameRequest
    {
        public string nickname;
    }

    [System.Serializable]
    public class CheckNicknameData
    {
        public bool? available;
        public string suggestion;
    }

    [System.Serializable]
    public class ApiResponseOfCheckNicknameData
    {
        public bool? isSuccess;
        public CheckNicknameData data;
        public string code;
        public string message;
    }

    [System.Serializable]
    public class LogoutRequest
    {
        public string refreshToken;
    }

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

    public async Task<MySettingsData> GetSettingsAsync()
    {
        if (apiProvider == null)
            throw new Exception("[MyPageApiService] apiProvider가 연결되지 않았습니다.");

        string baseUrl = apiProvider.ApiBaseUrl;
        string url = baseUrl.TrimEnd('/') + "/users/me/settings";

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

            HttpResponseMessage responseMessage = await httpClient.GetAsync(url);
            string json = await responseMessage.Content.ReadAsStringAsync();

            Debug.Log($"[MyPageApiService] GET /users/me/settings response={json}");

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new Exception($"[MyPageApiService] GetSettings HTTP failed: status={responseMessage.StatusCode}, body={json}");
            }

            var response = JsonConvert.DeserializeObject<ApiResponseOfMySettingsData>(json);

            if (response == null || response.isSuccess != true || response.data == null)
            {
                throw new Exception($"[MyPageApiService] GetSettings failed: code={response?.code}, message={response?.message}");
            }

            return response.data;
        }
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
        if (apiProvider == null) throw new Exception("[MyPageApiService] apiProvider가 연결되지 않았습니다.");

        string baseUrl = apiProvider.ApiBaseUrl;
        string url = baseUrl.TrimEnd('/') + "/users/me";

        using (HttpClient httpClient = new HttpClient())
        {
            string accessToken = apiProvider.AccessTokenForRequest;

            if (!string.IsNullOrEmpty(accessToken))
            { httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim()); }

            HttpResponseMessage responseMessage = await httpClient.DeleteAsync(url);
            string body = await responseMessage.Content.ReadAsStringAsync();

            Debug.Log($"[MyPageApiService] DELETE /users/me status={(int)responseMessage.StatusCode}, body={body}");

            if (responseMessage.IsSuccessStatusCode)
                return;

            throw new Exception($"[MyPageApiService] DeleteMe HTTP failed: status={responseMessage.StatusCode}, body={body}");
        }
    }

    public async Task<MySettingsData> UpdateSettingsAsync(bool showNpcScript)
    {
        if (apiProvider == null) throw new Exception("[MyPageApiService] apiProvider가 연결되지 않았습니다.");

        string baseUrl = apiProvider.ApiBaseUrl;
        string url = baseUrl.TrimEnd('/') + "/users/me/settings";

        using (HttpClient httpClient = new HttpClient())
        {
            string accessToken = apiProvider.AccessTokenForRequest;

            if (!string.IsNullOrEmpty(accessToken))
            { httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim()); }

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var body = new UpdateSettingsRequest
            { showNpcScript = showNpcScript };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            { Content = content };

            HttpResponseMessage responseMessage = await httpClient.SendAsync(request);
            string json = await responseMessage.Content.ReadAsStringAsync();

            Debug.Log($"[MyPageApiService] PATCH /users/me/settings response={json}");

            if (!responseMessage.IsSuccessStatusCode) throw new Exception($"[MyPageApiService] UpdateSettings HTTP failed: status={responseMessage.StatusCode}, body={json}");

            var response = JsonConvert.DeserializeObject<ApiResponseOfMySettingsData>(json);

            if (response == null || response.isSuccess != true || response.data == null) throw new Exception($"[MyPageApiService] UpdateSettings failed: code={response?.code}, message={response?.message}");

            return response.data;
        }
    }

    public async Task<CheckNicknameData> CheckNicknameAsync(string nickname)
    {
        if (apiProvider == null)
            throw new Exception("[MyPageApiService] apiProvider가 연결되지 않았습니다.");

        string baseUrl = apiProvider.ApiBaseUrl;
        string url = baseUrl.TrimEnd('/') + "/auth/check-nickname";

        using (HttpClient httpClient = new HttpClient())
        {
            string accessToken = apiProvider.AccessTokenForRequest;

            if (!string.IsNullOrEmpty(accessToken))
            { httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim()); }

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            var body = new CheckNicknameRequest
            { nickname = nickname };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessage = await httpClient.PostAsync(url, content);
            string json = await responseMessage.Content.ReadAsStringAsync();

            Debug.Log($"[MyPageApiService] POST /auth/check-nickname response={json}");

            if (!responseMessage.IsSuccessStatusCode) throw new Exception($"[MyPageApiService] CheckNickname HTTP failed: status={responseMessage.StatusCode}, body={json}");

            var response = JsonConvert.DeserializeObject<ApiResponseOfCheckNicknameData>(json);

            if (response == null || response.isSuccess != true || response.data == null) throw new Exception($"[MyPageApiService] CheckNickname failed: code={response?.code}, message={response?.message}");

            return response.data;
        }
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (apiProvider == null)
            throw new Exception("[MyPageApiService] apiProvider가 연결되지 않았습니다.");

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new Exception("[MyPageApiService] refreshToken이 없습니다.");

        string baseUrl = apiProvider.ApiBaseUrl;
        string url = baseUrl.TrimEnd('/') + "/auth/logout";

        using (HttpClient httpClient = new HttpClient())
        {
            string accessToken = apiProvider.AccessTokenForRequest;

            if (!string.IsNullOrEmpty(accessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken.Trim());
            }

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var body = new LogoutRequest
            { refreshToken = refreshToken.Trim() };

            string jsonBody = JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessage = await httpClient.PostAsync(url, content);
            string responseBody = await responseMessage.Content.ReadAsStringAsync();

            Debug.Log($"[MyPageApiService] POST /auth/logout status={(int)responseMessage.StatusCode}, body={responseBody}");

            if (!responseMessage.IsSuccessStatusCode)
            { throw new Exception($"[MyPageApiService] Logout HTTP failed: status={responseMessage.StatusCode}, body={responseBody}"); }
        }
    }
}