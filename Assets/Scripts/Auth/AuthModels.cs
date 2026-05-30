using System;

[Serializable]
public class OAuthLoginRequest
{
    public string authorizationCode;
}

[Serializable]
public class OAuthLoginResponse
{
    public bool isSuccess;
    public OAuthLoginData data;
    public string code;
    public string message;
}

[Serializable]
public class OAuthLoginData
{
    public string userUuid;
    public string email;
    public string nickname;
    public string profileImageUrl;
    public int provider;
    public string accessToken;
    public string refreshToken;
    public bool isNewUser;
}

[Serializable]
public class RefreshTokenRequest
{
    public string refreshToken;
}

[Serializable]
public class RefreshTokenResponse
{
    public bool isSuccess;
    public RefreshTokenData data;
    public string code;
    public string message;
}

[Serializable]
public class RefreshTokenData
{
    public string accessToken;
    public string refreshToken;
}