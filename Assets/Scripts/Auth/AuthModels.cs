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
}

[Serializable]
public class OAuthLoginData
{
    public string userId;
    public string email;
    public string nickname;
    public string profileImageUrl;
    public string provider;
    public string accessToken;
    public string refreshToken;
    public bool isNewUser;
}