using UnityEngine;
using System;

public class KakaoLoginService : MonoBehaviour
{
    [SerializeField] private string restApiKey;
    [SerializeField] private string redirectUri = "myapp://oauth/kakao";

    public void StartLogin()
    {
        string authUrl =
            "https://kauth.kakao.com/oauth/authorize" +
            "?client_id=" + restApiKey +
            "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
            "&response_type=code";
        Application.OpenURL(authUrl);
    }
}
