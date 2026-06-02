using System;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;
using Speakat.Client;

public class AuthApi : MonoBehaviour
{
    [SerializeField] private string baseUrl = "https://speakat.hyorim.shop";
    [SerializeField] private TokenStore tokenStore;

    private HttpClient httpClient;
    private SpeakatClient client;

    private void Awake()
    {
        httpClient = new HttpClient();

        client = new SpeakatClient(httpClient)
        {
            BaseUrl = baseUrl.TrimEnd('/') + "/"
        };
    }

    private void OnDestroy()
    {
        httpClient?.Dispose();
        httpClient = null;
        client = null;
    }

    private void SetAuthorizationHeader(string accessToken)
    {
        httpClient.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
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

    private string ToErrorMessage(Exception exception)
    {
        Exception root = exception?.GetBaseException();

        if (root is ApiException apiException)
        {
            return $"HTTP {apiException.StatusCode}: {apiException.Message}\n{apiException.Response}";
        }

        return root != null ? root.Message : "Unknown error";
    }

    public IEnumerator LoginWithOAuth(
        string provider,
        string authorizationCode,
        Action<OAuthLoginResponse> onSuccess,
        Action<string> onFail)
    {
        SetAuthorizationHeader(null);

        var request = new OAuthLoginRequestDto
        {
            AuthorizationCode = authorizationCode
        };

        Debug.Log($"[AuthApi] POST {client.BaseUrl}auth/oauth/{provider}");

        Task<ApiResponseOfOAuthLoginResponseDto> task =
            client.OauthAsync(provider, request);

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
    }

    public IEnumerator RefreshToken(
        string refreshToken,
        Action<RefreshTokenResponse> onSuccess,
        Action<string> onFail)
    {
        SetAuthorizationHeader(null);

        var request = new RefreshTokenRequestDto
        {
            RefreshToken = refreshToken
        };

        Debug.Log($"[AuthApi] POST {client.BaseUrl}auth/refresh");

        Task<ApiResponseOfRefreshTokenResponseDto> task =
            client.RefreshAsync(request);

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
        string accessToken = tokenStore != null ? tokenStore.AccessToken : null;
        SetAuthorizationHeader(accessToken);

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Debug.LogWarning("[AuthApi] accessToken is empty. Authorization header was not attached to CheckNickname.");
        }

        var request = new CheckNicknameRequestDto
        {
            Nickname = nickname
        };

        Debug.Log($"[AuthApi] POST {client.BaseUrl}auth/check-nickname");

        Task<ApiResponseOfCheckNicknameResponseDto> task =
            client.CheckNicknameAsync(request);

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
        string accessToken = tokenStore != null ? tokenStore.AccessToken : null;
        SetAuthorizationHeader(accessToken);

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Debug.LogWarning("[AuthApi] accessToken is empty. Authorization header was not attached to UpdateMyProfile.");
        }

        var request = new PatchUserRequestDto
        {
            Nickname = nickname,
            ProfileImageKey = null
        };

        Debug.Log($"[AuthApi] PATCH {client.BaseUrl}users/me");

        Task<ApiResponseOfPatchUserResultDto> task =
            client.MePATCHAsync(request);

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