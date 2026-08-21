using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Speakat.Client;

public class AuthApi : MonoBehaviour
{
    [SerializeField] private SpeakatApiProvider apiProvider;

    private SpeakatClient Client
    {
        get
        {
            if (apiProvider == null)
                throw new Exception("[AuthApi] apiProvider가 연결되지 않았습니다.");

            return apiProvider.Client;
        }
    }

    public IEnumerator LoginWithOAuth(
        string provider,
        string authorizationCode,
        Action<OAuthLoginResponse> onSuccess,
        Action<string> onFail)
    {
        Debug.Log($"[AuthApi] LoginWithOAuth called. provider={provider}, platform={Application.platform}");

#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("[AuthApi] WebGL fallback path selected");
        yield return LoginWithOAuthWebGL(provider, authorizationCode, onSuccess, onFail);
#else
        Debug.Log("[AuthApi] SDK path selected");

        var request = new OAuthLoginRequestDto
        {
            AuthorizationCode = authorizationCode
        };

        Debug.Log($"[AuthApi] POST auth/oauth/{provider}");

        Task<ApiResponseOfOAuthLoginResponseDto> task =
            Client.OauthAsync(provider, request);

        yield return WaitTask(
            task,
            response => ValidateApiResponse(
                response?.IsSuccess,
                response?.Data,
                response?.Code,
                response?.Message,
                "Login"
            ),
            response => onSuccess?.Invoke(ToLegacyOAuthLoginResponse(response)),
            onFail
        );
#endif
    }

    public IEnumerator RefreshToken(
        string refreshToken,
        Action<RefreshTokenResponse> onSuccess,
        Action<string> onFail)
    {
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        };

        Debug.Log("[AuthApi] POST auth/refresh");

        Task<ApiResponseOfRefreshTokenResponseDto> task =
            Client.RefreshAsync(request);

        yield return WaitTask(
            task,
            response => ValidateApiResponse(
                response?.IsSuccess,
                response?.Data,
                response?.Code,
                response?.Message,
                "Refresh"
            ),
            response => onSuccess?.Invoke(ToLegacyRefreshTokenResponse(response)),
            onFail
        );
    }

    public IEnumerator CheckNickname(
        string nickname,
        Action<CheckNicknameResponse> onSuccess,
        Action<string> onFail)
    {
        var request = new CheckNicknameRequestDto
        {
            Nickname = nickname
        };

        Debug.Log("[AuthApi] POST auth/check-nickname");

        Task<ApiResponseOfCheckNicknameResponseDto> task =
            Client.CheckNicknameAsync(request);

        yield return WaitTask(
            task,
            response => ValidateApiResponse(
                response?.IsSuccess,
                response?.Data,
                response?.Code,
                response?.Message,
                "CheckNickname"
            ),
            response => onSuccess?.Invoke(ToLegacyCheckNicknameResponse(response)),
            onFail
        );
    }

    public IEnumerator UpdateMyProfile(
        string nickname,
        Action<PatchUserResponse> onSuccess,
        Action<string> onFail)
    {
        var request = new PatchUserRequestDto
        {
            Nickname = nickname,
            ProfileImageKey = null
        };

        Debug.Log("[AuthApi] PATCH users/me");

        Task<ApiResponseOfPatchUserResultDto> task =
            Client.MePATCHAsync(request);

        yield return WaitTask(
            task,
            response => ValidateApiResponse(
                response?.IsSuccess,
                response?.Data,
                response?.Code,
                response?.Message,
                "UpdateMyProfile"
            ),
            response => onSuccess?.Invoke(ToLegacyPatchUserResponse(response)),
            onFail
        );
    }

    private IEnumerator LoginWithOAuthWebGL(
        string provider,
        string authorizationCode,
        Action<OAuthLoginResponse> onSuccess,
        Action<string> onFail)
    {
        string url = BuildUrl($"/auth/oauth/{provider}");

        OAuthLoginRequest requestBody = new OAuthLoginRequest
        {
            authorizationCode = authorizationCode
        };

        string json = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");

            Debug.Log($"[AuthApi:WebGL] POST {url}");
            Debug.Log($"[AuthApi:WebGL] Body: {json}");

            yield return req.SendWebRequest();

            string responseText = req.downloadHandler != null
                ? req.downloadHandler.text
                : "";

            Debug.Log($"[AuthApi:WebGL] Status: {req.responseCode}");
            Debug.Log($"[AuthApi:WebGL] Response: {responseText}");

            if (req.result != UnityWebRequest.Result.Success)
            {
                string error = $"HTTP {req.responseCode}: {req.error}\n{responseText}";
                Debug.LogError($"[AuthApi:WebGL] Login Error: {error}");
                onFail?.Invoke(error);
                yield break;
            }

            OAuthLoginResponse response = null;

            try
            {
                response = JsonUtility.FromJson<OAuthLoginResponse>(responseText);
            }
            catch (Exception e)
            {
                string error = $"Login response parse failed: {e.Message}\n{responseText}";
                Debug.LogError($"[AuthApi:WebGL] {error}");
                onFail?.Invoke(error);
                yield break;
            }

            if (response == null)
            {
                onFail?.Invoke("[AuthApi:WebGL] Login response is null");
                yield break;
            }

            if (!response.isSuccess)
            {
                string message = string.IsNullOrEmpty(response.message)
                    ? "Login failed"
                    : response.message;

                Debug.LogError($"[AuthApi:WebGL] API Fail: code={response.code}, message={message}");
                onFail?.Invoke(message);
                yield break;
            }

            if (response.data == null)
            {
                onFail?.Invoke("[AuthApi:WebGL] Login success but data is null");
                yield break;
            }

            onSuccess?.Invoke(response);
        }
    }

    private IEnumerator WaitTask<T>(
        Task<T> task,
        Func<T, string> validate,
        Action<T> onSuccess,
        Action<string> onFail)
    {
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsCanceled)
        {
            onFail?.Invoke("Request canceled");
            yield break;
        }

        if (task.IsFaulted)
        {
            string error = ToErrorMessage(task.Exception);
            Debug.LogError($"[AuthApi] Request failed: {error}");
            onFail?.Invoke(error);
            yield break;
        }

        T result = task.Result;
        string validationError = validate?.Invoke(result);

        if (!string.IsNullOrWhiteSpace(validationError))
        {
            Debug.LogError($"[AuthApi] API failed: {validationError}");
            onFail?.Invoke(validationError);
            yield break;
        }

        onSuccess?.Invoke(result);
    }

    private string ValidateApiResponse(
        bool? isSuccess,
        object data,
        string code,
        string message,
        string label)
    {
        if (isSuccess != true)
        {
            return string.IsNullOrWhiteSpace(message)
                ? $"{label} failed. code={code}"
                : message;
        }

        if (data == null)
        {
            return $"{label} success but data is null.";
        }

        return null;
    }

    private string ToErrorMessage(Exception exception)
    {
        Exception root = exception?.GetBaseException();

        if (root is ApiException apiException)
        {
            return $"HTTP {apiException.StatusCode}: {apiException.Message}\n{apiException.Response}";
        }

        return root != null ? root.Message : "Unknown error";
    }

    private string BuildUrl(string path)
    {
        if (apiProvider == null)
            throw new Exception("[AuthApi] apiProvider가 연결되지 않았습니다.");

        string root = apiProvider.ApiBaseUrl; // 하드코딩된 url 수정
        string p = path.StartsWith("/") ? path : "/" + path;
        
        return root.TrimEnd('/') + p;
    }

    private OAuthLoginResponse ToLegacyOAuthLoginResponse(
        ApiResponseOfOAuthLoginResponseDto response)
    {
        OAuthLoginResponseDto data = response.Data;

        return new OAuthLoginResponse
        {
            isSuccess = response.IsSuccess == true,
            code = response.Code,
            message = response.Message,
            data = new OAuthLoginData
            {
                userId = data.UserUuid,
                email = data.Email,
                nickname = data.Nickname,
                profileImageUrl = data.ProfileImageUrl,
                provider = data.Provider?.ToString(),
                accessToken = data.AccessToken,
                refreshToken = data.RefreshToken,
                isNewUser = data.IsNewUser == true
            }
        };
    }

    private RefreshTokenResponse ToLegacyRefreshTokenResponse(
        ApiResponseOfRefreshTokenResponseDto response)
    {
        RefreshTokenResponseDto data = response.Data;

        return new RefreshTokenResponse
        {
            isSuccess = response.IsSuccess == true,
            code = response.Code,
            message = response.Message,
            data = new RefreshTokenData
            {
                accessToken = data.AccessToken,
                refreshToken = data.RefreshToken
            }
        };
    }

    private CheckNicknameResponse ToLegacyCheckNicknameResponse(
        ApiResponseOfCheckNicknameResponseDto response)
    {
        CheckNicknameResponseDto data = response.Data;

        return new CheckNicknameResponse
        {
            isSuccess = response.IsSuccess == true,
            code = response.Code,
            message = response.Message,
            data = new CheckNicknameData
            {
                available = data.Available == true,
                suggestion = data.Suggestion
            }
        };
    }

    private PatchUserResponse ToLegacyPatchUserResponse(
        ApiResponseOfPatchUserResultDto response)
    {
        PatchUserResultDto data = response.Data;

        return new PatchUserResponse
        {
            isSuccess = response.IsSuccess == true,
            code = response.Code,
            message = response.Message,
            data = new PatchUserData
            {
                userId = data.UserId,
                nickname = data.Nickname,
                profileImageUrl = data.ProfileImageUrl
            }
        };
    }
}