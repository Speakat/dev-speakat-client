using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class ProfileView : MonoBehaviour
{
    [SerializeField] private Image profilePic;
    [SerializeField] private TMP_Text nicknameText;
    //[SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text courseText;
    //[SerializeField] private Slider expProgressBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Setup(string nickname, string course, string url)
    {
        // nicknameText.text = $"{nickname} 님";
        nicknameText.text = FormatNickname(nickname);
        //levelText.text = $"Lv.{data.level}";
        courseText.text = course;
        //expProgressBar.value = data.expProgress;
        
        if (!string.IsNullOrEmpty(url)) StartCoroutine(LoadProfileImage(url));
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

    IEnumerator LoadProfileImage(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                profilePic.sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
            else
            {
                Debug.LogWarning($"[ProfileView] 프로필 이미지 로드 실패: {uwr.error}");
            }
        }
    }
}
