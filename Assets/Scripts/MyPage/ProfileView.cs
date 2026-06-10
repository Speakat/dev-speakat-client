using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class ProfileView : MonoBehaviour
{
    [SerializeField] private Image profilePic;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text courseText;

    [Header("Profile Image")]
    [SerializeField] private Sprite defaultProfileSprite;

    private Coroutine profileImageCoroutine;

    public void Setup(string nickname, string course, string url)
    {
        if (nicknameText != null)
            nicknameText.text = FormatNickname(nickname);

        if (courseText != null)
            courseText.text = course;

        SetDefaultProfileImage();

        if (profileImageCoroutine != null)
        {
            StopCoroutine(profileImageCoroutine);
            profileImageCoroutine = null;
        }

        if (!string.IsNullOrWhiteSpace(url))
            profileImageCoroutine = StartCoroutine(LoadProfileImage(url));
    }

    private string FormatNickname(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            return "학습자 님";

        string displayName = nickname.Trim();

        if (displayName.Length > 12)
            displayName = displayName.Substring(0, 12) + "...";

        return $"{displayName} 님";
    }

    private void SetDefaultProfileImage()
    {
        if (profilePic == null)
            return;

        if (defaultProfileSprite != null)
            profilePic.sprite = defaultProfileSprite;

        profilePic.preserveAspect = true;
    }

    private IEnumerator LoadProfileImage(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[ProfileView] 프로필 이미지 로드 실패: {uwr.error}");
                SetDefaultProfileImage();
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

            if (texture == null)
            {
                Debug.LogWarning("[ProfileView] 프로필 이미지 texture가 null입니다.");
                SetDefaultProfileImage();
                yield break;
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            if (profilePic != null)
            {
                profilePic.sprite = sprite;
                profilePic.preserveAspect = true;
            }
        }
    }
}