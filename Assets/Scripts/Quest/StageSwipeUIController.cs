using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StageSwipeUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject pagePrefab; // 페이지 프리팹
    [SerializeField]
    private GameObject indicatorPrefab; // 페이지 인디케이터 프리팹

    [SerializeField]
    private Scrollbar scrollBar; // 현재 페이지 검사(Scrollbar 위치 기반)
    [SerializeField]
    private Transform indicatorParent; // 페이지 인디케이터의 부모 Transform

    [SerializeField]
    private Transform[] indicatorContents;	// 현재 페이지를 나타내는 UI들의 Transform
    [SerializeField]
    private float swipeTime = 0.2f; // 스와이프 되는 시간
    [SerializeField]
    private float swipeDistance = 50.0f; // 다음 페이지 넘어가기 위한 스와이프 거리

    private float[] scrollPageValues; // 각 페이지 위치 값
    private float valueDistance; // 페이지 사이 거리
    private int currentPage = 0; // 현재 페이지
    private int maxPage = 0; // 최대 페이지
    private float startTouchPositionX; // 터치 시작 위치
    private float endTouchPositionX; // 터치 종료 위치
    private bool isSwiping = false; // 현재 스와이프 중인지
    private float circleContentScale = 0.5f;    // 현재 페이지의 원 크기(배율)

    // 더미데이터
    private string stageDetailDummyData = @"
    {
      ""isSuccess"": true,
      ""data"": {
        ""stageId"": 1,
        ""title"": ""카페에서 주문하기"",
        ""description"": ""카페에서 음료를 주문하는 상황을 연습합니다."",
        ""status"": ""UNLOCKED"",
        ""quests"": [
          {
            ""questId"": 1,
            ""title"": ""인사하고 메뉴판 받기"",
            ""description"": ""점원에게 인사하고 메뉴판을 요청하세요."",
            ""sortOrder"": 1,
            ""isCompleted"": true,
            ""attemptCount"": 2
          },
          {
            ""questId"": 2,
            ""title"": ""음료 주문하기"",
            ""description"": ""원하는 음료와 수량을 말하세요."",
            ""sortOrder"": 2,
            ""isCompleted"": false,
            ""attemptCount"": 1
          },
          {
            ""questId"": 3,
            ""title"": ""결제 및 인사"",
            ""description"": ""결제 수단을 선택하고 작별 인사를 하세요."",
            ""sortOrder"": 3,
            ""isCompleted"": false,
            ""attemptCount"": 0
          }
        ]
      },
      ""code"": null,
      ""message"": null
    }";

    public void Start()
    {
        Init(5, 2); // 테스트용 코드
    }

    public void Init(int stageCount, int selectedStageId)
    {
        StageDetailResponse stageDetailResponse = JsonUtility.FromJson<StageDetailResponse>(stageDetailDummyData);
        // TODO : api 호출해서 페이지 수 세팅
        maxPage = stageCount;

        SetPrefabs(maxPage);

        int pageCount = transform.childCount;
        scrollPageValues = new float[pageCount];

        if (pageCount == 0) // 페이지가 없는 경우
        {
            Debug.LogError("페이지가 존재하지 않습니다.");
        }
        else if (pageCount == 1) // 페이지 1개인 경우
        {
            scrollPageValues[0] = 0;
        }
        else // 페이지 2개 이상의 경우
        {
            // 페이지 사이의 거리(ScrollView: 0~1 사이 값)
            valueDistance = 1f / (pageCount - 1f);

            // 페이지의 각 value 위치 설정 (0 <= value <= 1)
            for (int i = 0; i < pageCount; ++i)
            {
                scrollPageValues[i] = valueDistance * i;
            }
        }

        // 인디케이터 설정
        indicatorContents = new Transform[indicatorParent.childCount];
        for (int i = 0; i < indicatorParent.childCount; ++i)
        {
            indicatorContents[i] = indicatorParent.GetChild(i);
        }

        // 선택된 스테이지 설정
        int targetIndex = selectedStageId - 1;
        if (targetIndex < 0 || targetIndex >= maxPage)
        {
            targetIndex = 0;
        }
        SetScrollBarValue(targetIndex);
    }

    private void SetPrefabs(int count)
    {
        for (int i = 0; i < count; ++i)
        {
            GameObject page = Instantiate(pagePrefab, transform);
            GameObject indicator = Instantiate(indicatorPrefab, indicatorParent);

            page.GetComponent<QuestPanelUIController>().SetStageName($"Stage {i + 1}");
        }
    }

    public void SetScrollBarValue(int index)
    {
        currentPage = index;
        scrollBar.value = scrollPageValues[index];
    }

    private void Update()
    {
        UpdateInput();

        UpdateCircleContent();
    }

    private void UpdateInput()
    {
        if (isSwiping == true) return;

// 유니티 에디터 테스트용
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPositionX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            endTouchPositionX = Input.mousePosition.x;
            UpdateSwipe();
        }
#endif

#if UNITY_ANDROID
		if ( Input.touchCount == 1 )
		{
			Touch touch = Input.GetTouch(0);

			if ( touch.phase == TouchPhase.Began )
			{
				startTouchPositionX = touch.position.x;
			}
			else if ( touch.phase == TouchPhase.Ended )
			{
				endTouchPositionX = touch.position.x;

				UpdateSwipe();
			}
		}
#endif
    }

    private void UpdateSwipe()
    {
        // 너무 작은 거리를 움직였을 때는 Swipe X
        if (Mathf.Abs(startTouchPositionX - endTouchPositionX) < swipeDistance)
        {
            // 원래 페이지로 돌아가기
            StartCoroutine(OnSwipeOneStep(currentPage));
            return;
        }

        // Swipe 방향
        bool isLeft = startTouchPositionX < endTouchPositionX ? true : false;

        if (isLeft == true) // 이동 방향 : 왼쪽
        {
            if (currentPage == 0) return;
            currentPage--;
        }
        else // 이동 방향 : 오른쪽
        {
            if (currentPage == maxPage - 1) return;
            currentPage++;
        }

        // currentIndex번째 페이지로 Swipe해서 이동
        StartCoroutine(OnSwipeOneStep(currentPage));
    }

    // Swipe 효과
    private IEnumerator OnSwipeOneStep(int index)
    {
        float start = scrollBar.value;
        float current = 0;
        float percent = 0;

        isSwiping = true;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / swipeTime;

            scrollBar.value = Mathf.Lerp(start, scrollPageValues[index], percent);

            yield return null;
        }

        isSwiping = false;
    }

    private void UpdateCircleContent()
    {
        // 아래에 배치된 페이지 버튼 크기, 색상 제어 (현재 머물고 있는 페이지의 버튼만 수정)
        for (int i = 0; i < scrollPageValues.Length; ++i)
        {
            indicatorContents[i].localScale = Vector2.one * circleContentScale;
            indicatorContents[i].GetComponent<Image>().color = Color.white;

            // 페이지의 절반을 넘어가면 현재 페이지 원을 바꾸도록
            if (scrollBar.value < scrollPageValues[i] + (valueDistance / 2) && scrollBar.value > scrollPageValues[i] - (valueDistance / 2))
            {
                indicatorContents[i].localScale = Vector2.one * circleContentScale;
                indicatorContents[i].GetComponent<Image>().color = Color.black;
            }
        }
    }
}
