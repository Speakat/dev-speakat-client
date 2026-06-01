using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NpcAudioController : MonoBehaviour
{
    AudioSource npcAudio;
    public float currentAudioClipLength = 0f;

    private void Awake()
    {
        npcAudio = GetComponent<AudioSource>();
    }

    public IEnumerator PlayAudioFromBase64(string base64Audio)
    {
        if (string.IsNullOrEmpty(base64Audio))
        {
            GamePlayManager.Instance.WaitForAudioClipPlay(0);
            yield break; // 오디오 없는 경우 재생 스킵
        }

        byte[] audioBytes = System.Convert.FromBase64String(base64Audio);

        // 임시 파일로 저장
        string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, "npc_audio.mp3");
        System.IO.File.WriteAllBytes(tempPath, audioBytes);

        // UnityWebRequest로 MP3 로드
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                currentAudioClipLength = clip.length; // 오디오 길이 저장
                npcAudio.clip = clip;
                npcAudio.Play();
            }
            else
            {
                Debug.LogError("오디오 로드 실패: " + www.error);
            }
        }
        GamePlayManager.Instance.WaitForAudioClipPlay(currentAudioClipLength);
    }
}
