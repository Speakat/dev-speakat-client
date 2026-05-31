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

[Serializable]
public class CheckNicknameRequest
{
    public string nickname;
}

[Serializable]
public class CheckNicknameResponse
{
    public bool isSuccess;
    public CheckNicknameData data;
    public string code;
    public string message;
}

[Serializable]
public class CheckNicknameData
{
    public bool available;
    public string suggestion;
}

[Serializable]
public class PatchUserRequest
{
    public string nickname;
    public string profileImageKey;
}

[Serializable]
public class PatchUserResponse
{
    public bool isSuccess;
    public PatchUserData data;
    public string code;
    public string message;
}

[Serializable]
public class PatchUserData
{
    public string userId;
    public string nickname;
    public string profileImageUrl;
}