using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VocabularyView : MonoBehaviour
{
    // 추후 요구사항에 맞게 서브 뷰들이 추가될 자리를 미리 비워둠
    // [Header("Child Views")]
    // [SerializeField] private VocabularyMainView vocabMainView;
    // [SerializeField] private WordReviewView wordReviewView;

    [Header("UI References")]

    [SerializeField] private Image vocabTabIcon; // 단어장 탭 아이콘 변수명 변경

    [SerializeField] private Color activeColor = new Color32(255, 138, 61, 255);
    [SerializeField] private Color inactiveColor = new Color32(170, 170, 170, 255);

    [SerializeField] private Transform filterTabContent; // 필터 탭들이 들어갈 부모
    [SerializeField] private Transform wordListContent;  // 단어 카드들이 들어갈 부모

    [Header("Prefabs")]
    [SerializeField] private GameObject filterTabPrefab; // 필터 탭 버튼 프리팹
    [SerializeField] private GameObject vocaCardPrefab;

    [SerializeField] private FlashCardView flashCardView;

    [SerializeField] private VocabularyApiService vocabularyApiService;
    [SerializeField] private bool useDummyOnApiFail = true;

    [Header("Navigation")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button flashCardBackButton;
    [SerializeField] private GameObject flashCardPanel;
    [SerializeField] private string previousSceneName = "Lobby";

    private IObjectPool<GameObject> vocaCardPool;
    private List<GameObject> activeCards = new List<GameObject>();

    private List<FilterTab> activeTabs = new List<FilterTab>();

    private VocabularyData currentData;

    private List<WordData> allWords = new List<WordData>();

    private long? selectedQuestId = null;
    private List<QuestFilterData> cachedQuestFilters = new List<QuestFilterData>();

    void Awake()
    {
        vocaCardPool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(vocaCardPrefab, wordListContent, false);

                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;
                rt.anchoredPosition = Vector2.zero;

                return obj;
            },
            actionOnGet: (obj) =>
            {
                obj.transform.SetParent(wordListContent, false);
                obj.transform.SetAsLastSibling();

                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;
                rt.anchoredPosition = Vector2.zero;

                obj.SetActive(true);
            },
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: 10,
            maxSize: 50
        );
    }

    private async void Start()
    {
        if (vocabTabIcon != null)
            vocabTabIcon.color = new Color32(255, 138, 61, 255);

        BindNavigationEvents();

        await LoadApiData();
    }
    private void BindNavigationEvents()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnClickBack);
            //Debug.Log("[VocabularyView] Main BackButton 연결 완료");
        }
        else
        {
            Debug.LogWarning("[VocabularyView] Main BackButton이 연결되지 않았습니다.");
        }

        if (flashCardBackButton != null)
        {
            flashCardBackButton.onClick.RemoveAllListeners();
            flashCardBackButton.onClick.AddListener(OnClickBack);
            //Debug.Log("[VocabularyView] FlashCard BackButton 연결 완료");
        }
        else
        {
            Debug.LogWarning("[VocabularyView] FlashCard BackButton이 연결되지 않았습니다.");
        }
    }

    public void OnClickBack()
    {
        bool isFlashCardOpen = flashCardPanel != null && flashCardPanel.activeSelf;

        if (isFlashCardOpen)
        {
            if (flashCardView != null)
                flashCardView.Close();
            else
                Debug.LogWarning("[VocabularyView] flashCardView가 연결되지 않았습니다.");

            return;
        }

        SceneManager.LoadScene(previousSceneName);
    }

    private void LoadDummyData()
    {
        // 서버에서 왔다고 가정하는 가짜 데이터
        VocabularyData dummyData = new VocabularyData
        {
            questFilterDataList = new List<QuestFilterData>
            {
                new QuestFilterData("전체", null),
                new QuestFilterData("마트 가기", 1),
                new QuestFilterData("카페 가기", 2)
            },
            wordList = new List<WordData>
            {
                new WordData { word = "market", pronunciation = "/ˈmɑːrkɪt/", meaning = "1. 시장, 가게\n2. 판매하다, 마케팅하다, 내놓다", questName = "Quest 1" },
                new WordData { word = "avocado", pronunciation = "/ˌævəˈkɑːdoʊ/", meaning = "1. 아보카도", questName = "Quest 1" },
                new WordData { word = "receipt", pronunciation = "/rɪˈsiːt/", meaning = "1. 영수증\n2. 수령", questName = "Quest 1" },
                new WordData { word = "milk", pronunciation = "/mɪlk/", meaning = "1. 우유\n2. 짜내다", questName = "Quest 1" },
                new WordData { word = "aisle", pronunciation = "/aɪl/", meaning = "1. 통로, 복도", questName = "Quest 3" },
                new WordData { word = "cart", pronunciation = "/kɑːrt/", meaning = "1. 카트, 수레", questName = "마트 가기" },
                new WordData { word = "cashier", pronunciation = "/kæˈʃɪr/", meaning = "1. 계산원", questName = "마트 가기" },
                new WordData { word = "discount", pronunciation = "/ˈdɪskaʊnt/", meaning = "1. 할인\n2. 할인하다", questName = "마트 가기" },
                new WordData { word = "checkout", pronunciation = "/ˈtʃekaʊt/", meaning = "1. 계산대\n2. 결제", questName = "마트 가기" },
                new WordData { word = "coupon", pronunciation = "/ˈkuːpɑːn/", meaning = "1. 쿠폰", questName = "마트 가기" },
                new WordData { word = "bakery", pronunciation = "/ˈbeɪkəri/", meaning = "1. 빵집, 제과점", questName = "카페 가기" },
                new WordData { word = "order", pronunciation = "/ˈɔːrdər/", meaning = "1. 주문\n2. 주문하다", questName = "카페 가기" },
                new WordData { word = "menu", pronunciation = "/ˈmenjuː/", meaning = "1. 메뉴", questName = "카페 가기" },
                new WordData { word = "takeout", pronunciation = "/ˈteɪkaʊt/", meaning = "1. 포장 음식\n2. 포장", questName = "카페 가기" },
                new WordData { word = "straw", pronunciation = "/strɔː/", meaning = "1. 빨대", questName = "카페 가기" }
            }
        };

        currentData = dummyData;
        cachedQuestFilters = dummyData.questFilterDataList;
        UpdateUI(dummyData);
        RefreshFilterTabs();
    }

    public void UpdateUI(VocabularyData data)
    {
        currentData = data;
        allWords = data.wordList != null ? new List<WordData>(data.wordList) : new List<WordData>();

        RebuildWordCards(data.wordList);
    }

    // 단어 카드 생성
    private void RebuildWordCards(List<WordData> words)
    {
        foreach (var card in activeCards)
            vocaCardPool.Release(card);

        activeCards.Clear();

        foreach (WordData wordData in words)
        {
            GameObject cardObj = vocaCardPool.Get();
            cardObj.transform.SetAsLastSibling();

            RectTransform rt = cardObj.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            rt.anchoredPosition = Vector2.zero;

            VocaCard cardScript = cardObj.GetComponent<VocaCard>();
            if (cardScript != null)
                cardScript.Setup(wordData);

            activeCards.Add(cardObj);
        }

        Canvas.ForceUpdateCanvases();

        if (wordListContent is RectTransform wordRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(wordRect);
        }
    }

    public void SetVocabTabActive(bool isActive)
    {
        if (vocabTabIcon != null)
        {
            vocabTabIcon.color = isActive ? activeColor : inactiveColor;

            var btn = vocabTabIcon.GetComponentInParent<Button>();
            if (btn != null) btn.interactable = !isActive;
        }
    }

    public void OpenFlashCards()
    {
        if (flashCardView == null)
        {
            Debug.LogError("[Vocabulary View] flashCardView 연결 안됨");
            return;
        }

        if (currentData == null || currentData.wordList == null || currentData.wordList.Count == 0)
        {
            Debug.LogWarning("[Vocabulary View] flashCard로 보여줄 단어 X");
            return;
        }

        flashCardView.Open(currentData.wordList, 0);
    }

    private async System.Threading.Tasks.Task LoadApiData(long? questId = null)
    {
        if (vocabularyApiService == null)
        {
            Debug.LogWarning("[VocabularyView] vocabularyApiService 연결 안됨. 더미데이터 사용");
            LoadDummyData();
            return;
        }

        try
        {
            selectedQuestId = questId;

            VocabularyData data = await vocabularyApiService.GetFlashcardsAsync(
                cursor: null,
                size: 20,
                questId: selectedQuestId
            );

            currentData = data;

            // 전체 조회일 때만 필터 목록 캐싱
            // 특정 questId로 재조회하면 응답이 해당 퀘스트만 오기 때문에 필터가 줄어드는 걸 방지
            if (selectedQuestId == null && data.questFilterDataList != null)
            {
                cachedQuestFilters = data.questFilterDataList;
            }

            UpdateUI(data);

            RefreshFilterTabs();

            Debug.Log($"[VocabularyView] API 데이터 로드 성공: questId={selectedQuestId}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[VocabularyView] API 데이터 로드 실패:\n{e}");

            if (useDummyOnApiFail)
                LoadDummyData();
        }
    }

    private void RefreshFilterTabs()
    {
        if (filterTabContent == null || filterTabPrefab == null)
            return;

        foreach (Transform child in filterTabContent)
            Destroy(child.gameObject);

        activeTabs.Clear();

        List<QuestFilterData> filters = cachedQuestFilters;

        if (filters == null || filters.Count == 0)
        {
            filters = new List<QuestFilterData>
            {
                new QuestFilterData("전체", null),
                new QuestFilterData("마트 가기", 1),
                new QuestFilterData("카페 가기", 2) // 더미에서는 필터 안 뜰 수 있어서 추가
            };
        }

        foreach (QuestFilterData filter in filters)
        {
            GameObject tabObj = Instantiate(filterTabPrefab, filterTabContent, false);

            RectTransform tabRt = tabObj.GetComponent<RectTransform>();
            if (tabRt != null)
            {
                tabRt.localScale = Vector3.one;
                tabRt.localRotation = Quaternion.identity;
                tabRt.anchoredPosition = Vector2.zero;
            }

            FilterTab tabScript = tabObj.GetComponent<FilterTab>();

            if (tabScript != null)
            {
                bool isSelected = selectedQuestId == filter.questId;

                tabScript.Setup(filter.label, isSelected, clickedTab =>
                {
                    OnClickQuestFilter(filter.questId);
                });

                activeTabs.Add(tabScript);
            }
            else
            {
                Button button = tabObj.GetComponent<Button>();
                TMP_Text text = tabObj.GetComponentInChildren<TMP_Text>();

                if (text != null)
                    text.text = filter.label;

                if (button != null)
                {
                    long? questId = filter.questId;

                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() =>
                    {
                        OnClickQuestFilter(questId);
                    });
                }

                Image image = tabObj.GetComponent<Image>();
                if (image != null)
                {
                    bool isSelected = selectedQuestId == filter.questId;
                    image.color = isSelected ? activeColor : inactiveColor;
                }
            }
        }
    }

    private async void OnClickQuestFilter(long? questId)
    {
        Debug.Log($"[VocabularyView] 필터 클릭: questId={questId}");
        await LoadApiData(questId);
    }
}