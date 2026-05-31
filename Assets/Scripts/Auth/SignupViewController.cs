using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignupViewController : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private Button submitButton;
    [SerializeField] private AuthApi authApi;

    private bool isSubmitting;

    private void Start()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
        }
        else
        {
            Debug.LogError("[Signup] Submit Button 연결 풀림");
        }

        SetError("");
    }

    private void OnSubmitClicked()
    {
        if (isSubmitting)
            return;

        if (nicknameInput == null)
        {
            Debug.LogError("[Signup] Nickname Input 연결 끊어짐");
            return;
        }

        if (authApi == null)
        {
            Debug.LogError("[Signup] AuthApi가 연결되지 않았습니다.");
            SetError("서비스 연결에 문제가 발생했습니다.");
            return;
        }

        string nickname = nicknameInput.text?.Trim();

        if (string.IsNullOrWhiteSpace(nickname))
        {
            SetError("닉네임을 입력해주세요.");
            return;
        }

        if (nickname.Length > 50)
        {
            SetError("닉네임은 50자 이하로 입력해주세요.");
            return;
        }

        SetSubmitting(true);
        SetError("");

        StartCoroutine(authApi.CheckNickname(
            nickname,
            response => OnCheckNicknameSuccess(nickname, response),
            OnCheckNicknameFail
        ));
    }

    private void OnCheckNicknameSuccess(string nickname, CheckNicknameResponse response)
    {
        if (response == null || response.data == null)
        {
            SetSubmitting(false);
            SetError("닉네임 확인 결과를 불러오지 못했습니다.");
            return;
        }

        if (!response.data.available)
        {
            SetSubmitting(false);

            string suggestion = response.data.suggestion;

            if (string.IsNullOrWhiteSpace(suggestion))
                SetError("이미 사용 중인 닉네임입니다.");
            else
                SetError($"이미 사용 중인 닉네임입니다.\n추천: {suggestion}");

            return;
        }

        StartCoroutine(authApi.UpdateMyProfile(
            nickname,
            OnUpdateProfileSuccess,
            OnUpdateProfileFail
        ));
    }

    private void OnCheckNicknameFail(string error)
    {
        SetSubmitting(false);
        Debug.LogError($"[Signup] 닉네임 중복 확인 실패: {error}");
        SetError("닉네임 확인 중 오류가 발생했습니다.");
    }

    private void OnUpdateProfileSuccess(PatchUserResponse response)
    {
        SetSubmitting(false);

        Debug.Log($"[Signup] 프로필 수정 성공: nickname={response?.data?.nickname}");

        OnSignupSuccess();
    }

    private void OnUpdateProfileFail(string error)
    {
        SetSubmitting(false);
        Debug.LogError($"[Signup] 프로필 수정 실패: {error}");
        SetError("닉네임 저장 중 오류가 발생했습니다.");
    }

    private void SetSubmitting(bool submitting)
    {
        isSubmitting = submitting;

        if (submitButton != null)
            submitButton.interactable = !submitting;

        if (nicknameInput != null)
            nicknameInput.interactable = !submitting;
    }

    private void SetError(string message)
    {
        if (errorText == null)
            return;

        errorText.text = message ?? "";
        errorText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }

    private void OnSignupSuccess()
    {
        Debug.Log("[Signup] 추가 정보 입력 완료");

        AuthManager aliveAuthManager = FindObjectOfType<AuthManager>();

        if (aliveAuthManager != null)
            aliveAuthManager.GoToNextScene();
        else
            Debug.LogError("[Signup] AuthManager를 찾을 수 없음");
    }
}