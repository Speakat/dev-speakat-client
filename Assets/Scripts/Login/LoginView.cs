using UnityEngine;
//using UnityEngine.SceneManagement;

public class LoginView : MonoBehaviour
{
    //private const string LOBBY_SCENE = "Lobby";
    [SerializeField] private AuthManager authManager;

    public void OnClickKakaoLogin()
    {
        //Debug.Log("Kakao mock Login");
        //SceneManager.LoadScene(LOBBY_SCENE);
        authManager.StartKakaoLogin();
    }

    public void OnClickGoogleLogin()
    {
        //Debug.Log("Google mock Login");
        //SceneManager.LoadScene(LOBBY_SCENE);

        //authManager.StartGoogleLogin();
        authManager.StartGoogleLogin();
    }
}
