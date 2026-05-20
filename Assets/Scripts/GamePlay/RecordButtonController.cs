using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class RecordButtonController : MonoBehaviour
{
    public Button recordButton;
    public float maxRecordingSeconds = 10f;

    public string microphoneDevice = "";
    public int sampleRate = 16000;
    public string formFieldName = "audio";
    public string authToken = "";

    private AudioClip _clip;
    private bool _isRecording;
    private Coroutine _autoStopCoroutine;
    private string _activeDevice;

    private Image _buttonImage;

    void Start()
    {
        _buttonImage = recordButton.GetComponent<Image>();
        recordButton.onClick.AddListener(OnRecordButtonClicked);

        RequestMicrophonePermission();
    }

    void OnRecordButtonClicked()
    {
        if (_isRecording)
            StopRecording();
        else
            StartRecording();
    }

    void StartRecording()
    {
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

        _activeDevice = string.IsNullOrEmpty(microphoneDevice)
            ? Microphone.devices[0]
            : microphoneDevice;

        _clip = Microphone.Start(_activeDevice, false, Mathf.CeilToInt(maxRecordingSeconds), sampleRate);
        _isRecording = true;

        Debug.Log($"녹음 시작: 장치={_activeDevice}, 최대={maxRecordingSeconds}s");

        _autoStopCoroutine = StartCoroutine(AutoStopAfterTimeout());
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
        if (!_isRecording) return;

        if (_autoStopCoroutine != null)
        {
            StopCoroutine(_autoStopCoroutine);
            _autoStopCoroutine = null;
        }

        int position = Microphone.GetPosition(_activeDevice);
        if (!Microphone.IsRecording(_activeDevice) && position == 0)
        {
            position = _clip.samples;
        }

        Microphone.End(_activeDevice);
        _isRecording = false;

        if (_clip == null || position <= 0)
        {
            if (_clip != null) Destroy(_clip); 
            return;
        }
        recordButton.interactable = false;

        AudioClip trimmedClip = TrimClip(_clip, position);
        byte[] wavBytes = AudioClipToWav(trimmedClip);

        Destroy(_clip);
        Destroy(trimmedClip);
        
        StartCoroutine(UploadWav());
    }

    IEnumerator UploadWav()
    {
        recordButton.interactable = true;
    }

    AudioClip TrimClip(AudioClip source, int sampleCount)
    {
        float[] data = new float[sampleCount * source.channels];
        source.GetData(data, 0);

        AudioClip trimmed = AudioClip.Create(
            "RecordedVoice",
            sampleCount,
            source.channels,
            source.frequency,
            false);
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
            bw.Write(s);

        return ms.ToArray();
    }
    
    protected virtual void OnUploadSuccess(string responseJson) { }
    protected virtual void OnUploadFailed(string error) { }

    void OnDestroy()
    {
        if (_isRecording) Microphone.End(_activeDevice);
    }

    private bool HasMicrophonePermission()
    {
#if UNITY_ANDROID
        return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#elif UNITY_IOS
        return Application.HasUserAuthorization(UserAuthorization.Microphone);
#else
        return true;
#endif
    }

    private void RequestMicrophonePermission()
    {
#if UNITY_ANDROID
        // 안드로이드 권한 요청
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#elif UNITY_IOS
        // iOS 권한 요청
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }
#endif
    }
}