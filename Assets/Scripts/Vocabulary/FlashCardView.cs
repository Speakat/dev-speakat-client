using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [Header("Swipe")]
    [SerializeField] private float swipeThreshold = 80f;

    private List<WordData> words = new List<WordData>();
    private int currentIndex = 0;
    private bool isFront = true;

    private Vector2 pointerDownPos;
    private bool didSwipe;

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

        RefreshCard();

        Debug.Log($"[FlashCardView] 다음 단어: index={currentIndex}, word={words[currentIndex].word}");
    }

    public void ShowPrev()
    {
        if (words.Count == 0) return;

        currentIndex = (currentIndex - 1 + words.Count) % words.Count;
        isFront = true;
        didSwipe = true;

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
            soundButton.onClick.AddListener(() =>
            {
                Debug.Log($"[FlashCardView] {data.word} 발음 재생");
            });
        }
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
        if (didSwipe)
        {
            Debug.Log("[FlashCardView] 스와이프 직후 클릭 무시");
            return;
        }

        Debug.Log("[FlashCardView] 카드 탭");
        ToggleCard();
    }
}