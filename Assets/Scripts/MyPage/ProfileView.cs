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

    public void Setup(string nickname, string course, string url)
    {
        if (nicknameText != null) nicknameText.text = $"{nickname} 님";
        //levelText.text = $"Lv.{data.level}";
        if (courseText != null) courseText.text = course;
        //expProgressBar.value = data.expProgress;

        Debug.Log($"[ProfileView] Setup nickname={nickname}, profileImageUrl={url}");

        if (!string.IsNullOrEmpty(url))
            StartCoroutine(LoadProfileImage(url));
    }

    IEnumerator LoadProfileImage(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ProfileView] 프로필 이미지 로드 실패: {uwr.responseCode} / {uwr.error}");
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

            profilePic.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            profilePic.preserveAspect = true;

            Debug.Log("[ProfileView] 프로필 이미지 로드 성공");
        }
    }
}
