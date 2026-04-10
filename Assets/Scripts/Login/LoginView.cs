using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginView : MonoBehaviour
{
    private const string LOBBY_SCENE = "Lobby";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
        
    //}

    //// Update is called once per frame
    //void Update()
    //{
        
    //}

    public void OnClickKakaoLogin()
    {
        Debug.Log("Kakao mock Login");
        SceneManager.LoadScene(LOBBY_SCENE);
    }

    public void OnClickGoogleLogin()
    {
        Debug.Log("Google mock Login");
        SceneManager.LoadScene(LOBBY_SCENE);
    }
}
