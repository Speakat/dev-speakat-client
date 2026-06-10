using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ProfileImageLoader : MonoBehaviour
{
    [SerializeField] private Image profileImage;

    public void LoadProfileImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            Debug.LogWarning("[ProfileImageLoader] imageUrl이 비어 있습니다.");
            return;
        }

        StartCoroutine(CoLoadProfileImage(imageUrl));
    }

    private IEnumerator CoLoadProfileImage(string imageUrl)
    {
        Debug.Log($"[ProfileImageLoader] 이미지 로드 시작: {imageUrl}");

        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[ProfileImageLoader] 이미지 로드 실패: {request.responseCode} / {request.error}");
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        profileImage.sprite = sprite;
        profileImage.preserveAspect = true;

        Debug.Log("[ProfileImageLoader] 이미지 로드 성공");
    }
}