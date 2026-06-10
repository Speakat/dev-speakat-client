using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class RecordButtonController : MonoBehaviour
{
    public Action<AudioClip> OnRecordingCompleted;  // 녹음 완료 시 Invoke

    private void FinishRecording()
    {
        #if !UNITY_WEBGL
                OnRecordingCompleted?.Invoke(_clip);
        #else
            Debug.LogWarning("[RecordButtonController] WebGL에서는 AudioClip 녹음 완료 처리를 사용하지 않습니다.");
        #endif
    }

    public Sprite defaultSprite;
    public Sprite recordingSprite;

    public Button recordButton;
    private float maxRecordingSeconds = 5f;

    public string microphoneDevice = "";
    public int sampleRate = 16000;
    public string formFieldName = "audio";
    public string authToken = "";

    #if !UNITY_WEBGL
        private AudioClip _clip;
    #endif

    private bool _isRecording;
    private Coroutine _autoStopCoroutine;
    private string _activeDevice;

    private Image _buttonImage;

    void Awake()
    {
        if (recordButton == null)
        {
            Debug.LogError("[RecordButtonController] recordButton이 연결되지 않았습니다.");
            return;
        }

        _buttonImage = recordButton.GetComponent<Image>();
        recordButton.onClick.AddListener(OnRecordButtonClicked);
        RequestMicrophonePermission();
    }

    void OnRecordButtonClicked()
    {
        #if UNITY_WEBGL
                Debug.LogWarning("[RecordButtonController] WebGL 빌드에서는 UnityEngine.Microphone을 사용할 수 없어 녹음을 건너뜁니다.");
                return;
        #else
                if (_isRecording)
                    StopRecording();
                else
                    StartRecording();
        #endif
    }

    public void SetRecordActive()
    {
        //Debug.Log("녹음 버튼 활성화");
        recordButton.interactable = true;
        _buttonImage.color = Color.white;
        _buttonImage.sprite = defaultSprite;
    }

    public void SetRecordInactive()
    {
        //Debug.Log("녹음 버튼 비활성화");
        recordButton.interactable = false;
        _buttonImage.sprite = defaultSprite;
    }

    void StartRecording()
    {
        #if UNITY_WEBGL
                Debug.LogWarning("[RecordButtonController] WebGL에서는 Microphone.Start를 호출하지 않습니다.");
                return;
        #else

        if (!HasMicrophonePermission())
        {
            Debug.LogWarning("마이크 권한이 허용되지 않았습니다.");
            RequestMicrophonePermission();
            return;
        }

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("연결된 마이크가 없습니다.");
            return;
        }

        _buttonImage.sprite = recordingSprite;

        _activeDevice = string.IsNullOrEmpty(microphoneDevice)
            ? Microphone.devices[0]
            : microphoneDevice;

        _clip = Microphone.Start(
            _activeDevice,
            false,
            Mathf.CeilToInt(maxRecordingSeconds),
            sampleRate
        );

        _isRecording = true;

        Debug.Log($"녹음 시작: 장치={_activeDevice}, 최대={maxRecordingSeconds}s");

        _autoStopCoroutine = StartCoroutine(AutoStopAfterTimeout());
        #endif
    }

    IEnumerator AutoStopAfterTimeout()
    {
        yield return new WaitForSeconds(maxRecordingSeconds);

        if (_isRecording)
        {
            StopRecording();
        }
    }

    void StopRecording()
    {
        #if UNITY_WEBGL
                Debug.LogWarning("[RecordButtonController] WebGL에서는 Microphone.End를 호출하지 않습니다.");
                _isRecording = false;
                return;
        #else

        if (!_isRecording) return;

        if (_autoStopCoroutine != null)
        {
            StopCoroutine(_autoStopCoroutine);
            _autoStopCoroutine = null;
        }

        int position = Microphone.GetPosition(_activeDevice);

        if (!Microphone.IsRecording(_activeDevice) && position == 0)
        {
            position = _clip != null ? _clip.samples : 0;
        }

        // Debug.Log($"[Record] 녹음 종료: position={position}, clipSamples={_clip?.samples}, channels={_clip?.channels}, frequency={_clip?.frequency}");

        Microphone.End(_activeDevice);
        _isRecording = false;

        if (_clip == null || position <= 0)
        {
            if (_clip != null) Destroy(_clip);
            return;
        }
        SetRecordInactive();

        AudioClip trimmedClip = TrimClip(_clip, position);
        byte[] wavBytes = AudioClipToWav(trimmedClip);

        Destroy(_clip);
        Destroy(trimmedClip);

        string base64Wav = System.Convert.ToBase64String(wavBytes);

        Debug.Log($"[Record] WAV 변환 완료: bytes={wavBytes.Length}, base64Length={base64Wav.Length}");

        StartCoroutine(UploadWav(base64Wav));
        #endif
    }

    IEnumerator UploadWav(string base64Wav)
    {
        // base64를 GamePlayManager로 넘겨서 API 호출
        //GamePlayManager.Instance.HandleRecordingCompletedWithBase64(base64Wav);

        //yield break;

        // Debug.Log($"[Record] GamePlayManager로 음성 전달: base64Length={base64Wav?.Length}");

        if (GamePlayManager.Instance == null)
        {
            Debug.LogError("[Record] GamePlayManager.Instance가 없습니다.");
            yield break;
        }

        GamePlayManager.Instance.HandleRecordingCompletedWithBase64(base64Wav);
    }

#if !UNITY_WEBGL
    AudioClip TrimClip(AudioClip source, int sampleCount)
    {
        float[] data = new float[sampleCount * source.channels];
        source.GetData(data, 0);

        AudioClip trimmed = AudioClip.Create(
            "RecordedVoice",
            sampleCount,
            source.channels,
            source.frequency,
            false
        );

        trimmed.SetData(data, 0);
        return trimmed;
    }

    byte[] AudioClipToWav(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        short[] pcm = new short[samples.Length];

        for (int i = 0; i < samples.Length; i++)
        {
            float clamped = Mathf.Clamp(samples[i], -1f, 1f);
            pcm[i] = (short)(clamped * short.MaxValue);
        }

        using MemoryStream ms = new MemoryStream();
        using BinaryWriter bw = new BinaryWriter(ms);

        int pcmBytes = pcm.Length * 2;
        int channels = clip.channels;
        int freq = clip.frequency;
        int byteRate = freq * channels * 2;

        bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + pcmBytes);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);
        bw.Write((short)1);
        bw.Write((short)channels);
        bw.Write(freq);
        bw.Write(byteRate);
        bw.Write((short)(channels * 2));
        bw.Write((short)16);

        bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        bw.Write(pcmBytes);

        foreach (short s in pcm)
        {
            bw.Write(s);
        }

        return ms.ToArray();
    }

    protected virtual void OnUploadSuccess(string responseJson) { }
    protected virtual void OnUploadFailed(string error) { }
#endif

    void OnDestroy()
    {
#if !UNITY_WEBGL
        if (_isRecording)
        {
            Microphone.End(_activeDevice);
        }
#endif
    }

    private bool HasMicrophonePermission()
    {
        #if UNITY_WEBGL
                return false;
        #elif UNITY_ANDROID
                return Permission.HasUserAuthorizedPermission(Permission.Microphone);
        #elif UNITY_IOS
                return Application.HasUserAuthorization(UserAuthorization.Microphone);
        #else
                return true;
        #endif
    }

    private void RequestMicrophonePermission()
    {
        #if UNITY_WEBGL
                Debug.LogWarning("[RecordButtonController] WebGL에서는 UnityEngine.Microphone 권한 요청을 건너뜁니다.");
        #elif UNITY_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {
                    Permission.RequestUserPermission(Permission.Microphone);
                }
        #elif UNITY_IOS
                if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
                {
                    Application.RequestUserAuthorization(UserAuthorization.Microphone);
                }
        #endif
            }
}