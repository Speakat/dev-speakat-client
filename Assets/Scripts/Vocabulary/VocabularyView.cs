using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Pool;

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

    private IObjectPool<GameObject> vocaCardPool;
    private List<GameObject> activeCards = new List<GameObject>();

    private List<FilterTab> activeTabs = new List<FilterTab>();

    void Awake()
    {
        // 1. 단어 카드 풀링 초기화
        vocaCardPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(vocaCardPrefab, wordListContent),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: 10, maxSize: 50);
    }

    void Start()
    {
        if (vocabTabIcon != null) vocabTabIcon.color = new Color32(255, 138, 61, 255);

        // 서버 연동 시 API 호출로 대체
        LoadDummyData();
    }

    private void LoadDummyData()
    {
        // 서버에서 왔다고 가정하는 가짜 데이터
        VocabularyData dummyData = new VocabularyData
        {
            questFilters = new List<string> { "전체", "즐겨찾기", "방금", "마트 가기", "카페 가기" },
            wordList = new List<WordData>
            {
                new WordData { word = "market", meaning = "시장, 마트", questName = "Quest 1" },
                new WordData { word = "avocado", meaning = "아보카도", questName = "Quest 1" },
                new WordData { word = "receipt", meaning = "영수증", questName = "Quest 1" },
                new WordData { word = "milk", meaning = "우유", questName = "Quest 1" },
                new WordData { word = "aisle", meaning = "통로, 복도", questName = "Quest 3" }
            }
        };

        UpdateUI(dummyData);
    }

    public void UpdateUI(VocabularyData data)
    {
        // 1. 필터 탭 동적 생성 (나중에 풀링으로 바꿔도 됨)
        foreach (Transform child in filterTabContent) Destroy(child.gameObject); // 기존 탭 초기화
        activeTabs.Clear();

        for (int i = 0; i < data.questFilters.Count; i++)
        {
            GameObject tabObj = Instantiate(filterTabPrefab, filterTabContent);
            FilterTab tabScript = tabObj.GetComponent<FilterTab>();

            if (tabScript != null)
            {
                // 첫 번째 탭(i == 0)만 기본으로 선택되게(true) 세팅
                bool isFirstTab = (i == 0);
                tabScript.Setup(data.questFilters[i], isFirstTab, OnFilterTabClicked);

                activeTabs.Add(tabScript);
            }
        }

        foreach (string filter in data.questFilters)
        {
            GameObject tab = Instantiate(filterTabPrefab, filterTabContent);
            tab.GetComponentInChildren<TMP_Text>().text = filter;
        }

        // 2. 단어 카드 생성 (오브젝트 풀링 사용)
        foreach (var card in activeCards) vocaCardPool.Release(card);
        activeCards.Clear();

        foreach (WordData wordData in data.wordList)
        {
            GameObject cardObj = vocaCardPool.Get();
            cardObj.transform.SetAsLastSibling(); // 순서 꼬임 방지

            VocaCard cardScript = cardObj.GetComponent<VocaCard>();
            if (cardScript != null) cardScript.Setup(wordData);

            activeCards.Add(cardObj);
        }
    }

    private void OnFilterTabClicked(FilterTab clickedTab)
    {
        foreach (var tab in activeTabs) tab.SetSelected(tab == clickedTab);

        Debug.Log($"[{clickedTab.FilterName}] 필터 적용");
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
}