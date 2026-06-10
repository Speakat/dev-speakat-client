using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Networking;

public class FlashCardView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Panels")]
    [SerializeField] private GameObject vocabularyMainPanel;
    [SerializeField] private GameObject flashCardPanel;

    [Header("Card Groups")]
    [SerializeField] private GameObject frontGroup;
    [SerializeField] private GameObject backGroup;

    [Header("Front UI")]
    [SerializeField] private TMP_Text frontWordText;

    [Header("Back UI")]
    [SerializeField] private TMP_Text backWordText;
    [SerializeField] private TMP_Text pronunciationText;
    [SerializeField] private TMP_Text meaningText;
    [SerializeField] private Button soundButton;

    [Header("Master UI")]
    [SerializeField] private VocabularyApiService vocabularyApiService;
    [SerializeField] private TMP_Text masterStatusText;
    [SerializeField] private Button knownButton;      // O
    [SerializeField] private Button unknownButton;    // X
    [SerializeField] private TMP_Text knownButtonText;
    [SerializeField] private TMP_Text unknownButtonText;

    [SerializeField] private bool autoMoveNextAfterChoice = true;

    [Header("Swipe")]
    [SerializeField] private float swipeThreshold = 80f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    private List<WordData> words = new List<WordData>();
    private int currentIndex = 0;
    private bool isFront = true;

    private Vector2 pointerDownPos;
    private bool didSwipe;

    private bool isUpdatingMasterState;
    private int feedbackIndex = -1;
    private string feedbackMessage = null;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (knownButton != null)
        {
            knownButton.onClick.RemoveAllListeners();
            knownButton.onClick.AddListener(() => OnClickMasterChoice(true));
        }

        if (unknownButton != null)
        {
            unknownButton.onClick.RemoveAllListeners();
            unknownButton.onClick.AddListener(() => OnClickMasterChoice(false));
        }
    }

    public void Open(List<WordData> wordList, int startIndex = 0)
    {
        words = wordList ?? new List<WordData>();

        if (words.Count == 0)
        {
            Debug.LogWarning("[FlashCardView] 단어 데이터가 비어 있음");
            return;
        }

        currentIndex = Mathf.Clamp(startIndex, 0, words.Count - 1);
        isFront = true;
        didSwipe = false;

        if (vocabularyMainPanel != null)
            vocabularyMainPanel.SetActive(false);

        if (flashCardPanel != null)
            flashCardPanel.SetActive(true);

        RefreshCard();

        Debug.Log($"[FlashCardView] Open: index={currentIndex}, word={words[currentIndex].word}");
    }

    public void Close()
    {
        Debug.Log("[FlashCardView] Close 호출");

        if (flashCardPanel != null)
            flashCardPanel.SetActive(false);

        if (vocabularyMainPanel != null)
            vocabularyMainPanel.SetActive(true);
    }

    public void ShowNext()
    {
        if (words.Count == 0) return;

        currentIndex = (currentIndex + 1) % words.Count;
        isFront = true;
        didSwipe = true;

        feedbackIndex = -1;
        feedbackMessage = null;

        RefreshCard();

        Debug.Log($"[FlashCardView] 다음 단어: index={currentIndex}, word={words[currentIndex].word}");
    }

    public void ShowPrev()
    {
        if (words.Count == 0) return;

        currentIndex = (currentIndex - 1 + words.Count) % words.Count;
        isFront = true;
        didSwipe = true;

        feedbackIndex = -1;
        feedbackMessage = null;

        RefreshCard();

        Debug.Log($"[FlashCardView] 이전 단어: index={currentIndex}, word={words[currentIndex].word}");
    }

    private void ToggleCard()
    {
        if (words.Count == 0) return;

        isFront = !isFront;
        RefreshCard();

        Debug.Log(isFront ? "[FlashCardView] 앞면 표시" : "[FlashCardView] 뒷면 표시");
    }

    private void RefreshCard()
    {
        if (words.Count == 0) return;

        WordData data = words[currentIndex];

        if (frontGroup != null)
            frontGroup.SetActive(isFront);

        if (backGroup != null)
            backGroup.SetActive(!isFront);

        if (frontWordText != null)
            frontWordText.text = data.word;

        if (backWordText != null)
            backWordText.text = data.word;

        if (pronunciationText != null)
            pronunciationText.text = data.pronunciation;

        if (meaningText != null)
            meaningText.text = data.meaning;

        if (soundButton != null)
        {
            soundButton.onClick.RemoveAllListeners();
            soundButton.onClick.AddListener(OnClickSoundButton);
        }

        RefreshMasterChoiceUI(data);
    }

    private async void OnClickSoundButton()
    {
        if (words == null || words.Count == 0)
            return;

        WordData currentWord = words[currentIndex];

        if (currentWord.flashcardId <= 0)
        {
            Debug.LogWarning("[FlashCardView] flashcardId가 없어 상세 조회를 할 수 없습니다.");
            return;
        }

        if (vocabularyApiService == null)
        {
            Debug.LogError("[FlashCardView] vocabularyApiService가 연결되지 않았습니다.");
            return;
        }

        if (soundButton != null)
            soundButton.interactable = false;

        try
        {
            Debug.Log($"[FlashCardView] 상세 조회 시작: flashcardId={currentWord.flashcardId}");

            WordData detail = await vocabularyApiService.GetFlashcardDetailAsync(currentWord.flashcardId);

            if (detail == null)
            {
                Debug.LogWarning("[FlashCardView] 상세 조회 결과가 null입니다.");
                return;
            }

            // 상세 응답으로 현재 데이터 갱신
            currentWord.word = detail.word;
            currentWord.meaning = detail.meaning;
            currentWord.pronunciation = detail.pronunciation;
            currentWord.audioUrl = detail.audioUrl;
            currentWord.isMastered = detail.isMastered;

            RefreshCard();

            if (string.IsNullOrWhiteSpace(currentWord.audioUrl))
            {
                Debug.LogWarning($"[FlashCardView] audioUrl이 없습니다: {currentWord.word}");
                return;
            }

            StartCoroutine(PlayAudioFromUrl(currentWord.audioUrl));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FlashCardView] 상세 조회 또는 오디오 재생 실패:\n{e}");
        }
        finally
        {
            if (soundButton != null)
                soundButton.interactable = true;
        }
    }

    private IEnumerator PlayAudioFromUrl(string audioUrl)
    {
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[FlashCardView] 오디오 로드 실패: {request.error}");
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

            if (clip == null)
            {
                Debug.LogWarning("[FlashCardView] AudioClip이 null입니다.");
                yield break;
            }

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.clip = clip;
            audioSource.Play();

            Debug.Log("[FlashCardView] 오디오 재생 시작");
        }
    }

    private void RefreshMasterChoiceUI(WordData data)
    {
        Debug.Log($"[FlashCardView] RefreshMasterChoiceUI: index={currentIndex}, word={data.word}, isMastered={data.isMastered}");

        if (data == null)
            return;

        if (masterStatusText != null)
        {
            if (feedbackIndex == currentIndex && !string.IsNullOrEmpty(feedbackMessage))
            {
                masterStatusText.text = feedbackMessage;
            }
            else
            {
                masterStatusText.text = data.isMastered
                    ? "마스터한 단어예요!"
                    : "이 단어를 알고 있나요?";
            }
        }

        if (knownButtonText != null)
            knownButtonText.text = data.isMastered ? "알고 있어요" : "잘 알아요!";

        if (unknownButtonText != null)
            unknownButtonText.text = data.isMastered ? "다시 복습할래요" : "아직 어려워요";

        bool canInteract = !isUpdatingMasterState;

        if (knownButton != null)
            knownButton.interactable = canInteract;

        if (unknownButton != null)
            unknownButton.interactable = canInteract;
    }

    private async void OnClickMasterChoice(bool isMastered)
    {
        if (isUpdatingMasterState)
            return;

        if (words == null || words.Count == 0)
            return;

        WordData currentWord = words[currentIndex];

        if (currentWord.flashcardId <= 0)
        {
            Debug.LogWarning("[FlashCardView] flashcardId가 없어 마스터 상태를 저장할 수 없습니다.");
            return;
        }

        if (vocabularyApiService == null)
        {
            Debug.LogError("[FlashCardView] vocabularyApiService가 연결되지 않았습니다.");
            return;
        }

        isUpdatingMasterState = true;
        SetMasterButtonsInteractable(false);

        try
        {
            bool savedIsMastered = await vocabularyApiService.SetMasteredAsync(
                currentWord.flashcardId,
                isMastered
            );

            currentWord.isMastered = savedIsMastered;

            Debug.Log(
                $"[FlashCardView] 마스터 상태 저장 성공: " +
                $"flashcardId={currentWord.flashcardId}, word={currentWord.word}, isMastered={savedIsMastered}"
            );

            feedbackIndex = currentIndex;
            feedbackMessage = savedIsMastered
                ? "마스터한 단어예요!"
                : "다음에 다시 복습해요!";

            RefreshMasterChoiceUI(currentWord);

            if (autoMoveNextAfterChoice && words.Count > 1)
            {
                await System.Threading.Tasks.Task.Delay(350);
                ShowNext();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FlashCardView] 마스터 상태 저장 실패:\n{e}");
        }
        finally
        {
            isUpdatingMasterState = false;

            if (words != null && words.Count > 0)
                RefreshMasterChoiceUI(words[currentIndex]);
        }
    }

    private void SetMasterButtonsInteractable(bool interactable)
    {
        if (knownButton != null)
            knownButton.interactable = interactable;

        if (unknownButton != null)
            unknownButton.interactable = interactable;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPos = eventData.position;
        didSwipe = false;

        Debug.Log($"[FlashCardView] PointerDown: {pointerDownPos}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Vector2 pointerUpPos = eventData.position;

        float deltaX = pointerUpPos.x - pointerDownPos.x;
        float deltaY = pointerUpPos.y - pointerDownPos.y;

        Debug.Log($"[FlashCardView] PointerUp: {pointerUpPos}, deltaX={deltaX}, deltaY={deltaY}");

        bool isHorizontalSwipe =
            Mathf.Abs(deltaX) >= swipeThreshold &&
            Mathf.Abs(deltaX) > Mathf.Abs(deltaY);

        if (!isHorizontalSwipe)
        {
            didSwipe = false;
            return;
        }

        didSwipe = true;

        if (deltaX < 0)
        {
            Debug.Log("[FlashCardView] 왼쪽 스와이프 → 다음 단어");
            ShowNext();
        }
        else
        {
            Debug.Log("[FlashCardView] 오른쪽 스와이프 → 이전 단어");
            ShowPrev();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerPress != null)
        {
            Button pressedButton = eventData.pointerPress.GetComponentInParent<Button>();

            if (pressedButton == knownButton || pressedButton == unknownButton || pressedButton == soundButton)
            {
                Debug.Log("[FlashCardView] 조작 버튼 클릭이므로 카드 뒤집기 무시");
                return;
            }
        }

        if (didSwipe)
        {
            Debug.Log("[FlashCardView] 스와이프 직후 클릭 무시");
            return;
        }

        Debug.Log("[FlashCardView] 카드 탭");
        ToggleCard();
    }
}