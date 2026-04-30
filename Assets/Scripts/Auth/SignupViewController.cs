using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignupViewController : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button submitButton;
    //[SerializeField] private AuthManager authManager; // 씬 전환용 연결

    private void Start()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
        }
        else Debug.LogError("Submit Button 연결 풀림");
    }

    private void OnSubmitClicked()
    {
        if (nicknameInput == null)
        {
            Debug.LogError("Nickname Input 연결 끊어짐");
            return;
        }

        string nickname = nicknameInput.text;
        //Debug.Log($"[Unity Read: {nickname}, 글자 수: {nickname.Length}");

        if (string.IsNullOrEmpty(nickname))
        {
            Debug.LogWarning("[Signup] 닉네임 입력 (null)");
            return;
        }

        Debug.Log($"[Signup] 입력된 닉네임: {nickname}. 백엔드 전송");

        OnSignupSuccess();
    }

    private void OnSignupSuccess()
    {
        Debug.Log("[Signup] 추가 정보 입력 완료");

        AuthManager aliveAuthManager = FindObjectOfType<AuthManager>();

        if (aliveAuthManager != null) aliveAuthManager.GoToNextScene();
        else Debug.LogError("Signup: Authmanger 찾을 수 없음");
    }
}
