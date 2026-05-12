using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class CalendarDayData
{
    public int day;        // 날짜
    public bool isAttended; // 출석 여부
}

// API에서 받아올 데이터용 (일단은 더미)
public class MyPageData
{
    public string nickname;
    //public int level;
    public string currentCourse;
    //public float expProgress; // Progress bar, 경험치 바 / 0.0f~1.0f
    public int currentStreak;
    public int scoreMeaning;
    public int scoreGrammar;
    public int scoreNaturalness;

    public int currentMonth;
    public int startDayOffset; // 1일 앞 빈칸 개수 (수욜 시작이면 앞에 2칸)
    public List<CalendarDayData> calendarDays; // 이번 달 전체 날짜 데이터
}

public class MyPageView : MonoBehaviour
{
    [Header("Profile Area")]
    [SerializeField] private TMP_Text nicknameText;
    //[SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text courseText;
    //[SerializeField] private Slider expProgressBar;

    [Header("Streak Area")]
    [SerializeField] private TMP_Text streakDaysText;

    [Header("Stats Area")]
    [SerializeField] private TMP_Text meaningText;
    [SerializeField] private TMP_Text grammarText;
    [SerializeField] private TMP_Text naturalnessText;

    [Header("Calendar Area")]
    [SerializeField] private TMP_Text monthText;
    [SerializeField] private Transform dateGrid;
    [SerializeField] private Transform streakContainer;
    [SerializeField] private GameObject dateCellPrefab;
    [SerializeField] private GameObject streakBarPrefab;
    [SerializeField] private GameObject singleCirclePrefab;

    [SerializeField] private float streakBarOffsetX = 20f;
    [SerializeField] private float streakBarOffsetY = 10f;
    [SerializeField] private float singleCircleOffsetX = 20f;
    [SerializeField] private float singleCircleOffsetY = 10f;

    //[SerializeField] private float cellSize = 100f;
    //[SerializeField] private float spacing = 20f;
    //[SerializeField] private float paddingTop = 0f;
    //[SerializeField] private float paddingLeft = 0f;
    private float cellSize;
    private float spacingX;
    private float spacingY;
    private float paddingTop;
    private float paddingLeft;

    void Start()
    {
        List<CalendarDayData> dummyDays = new List<CalendarDayData>();
        for (int i = 1; i <= 31; i++)
        {
            bool attended = (i >= 5 && i <= 10) || (i >= 18 && i <= 25);
            dummyDays.Add(new CalendarDayData { day = i, isAttended = attended });
        }

        MyPageData dummyData = new MyPageData
        {
            nickname = "스피캣",
            //level = 7,
            currentCourse = "A2 · 초급 영어",
            //expProgress = 0.7f, // 70% 정도
            currentStreak = 14,
            scoreMeaning = 87,
            scoreGrammar = 87,
            scoreNaturalness = 87,
            currentMonth = 10,
            startDayOffset = 3,
            calendarDays = dummyDays
        };

        UpdateUI(dummyData);
    }

    public void UpdateUI(MyPageData data)
    {
        nicknameText.text = $"{data.nickname} 님";
        //levelText.text = $"Lv.{data.level}";
        courseText.text = data.currentCourse;
        //expProgressBar.value = data.expProgress;

        streakDaysText.text = $"{data.currentStreak}일째 연속 학습!";

        meaningText.text = $"{data.scoreMeaning}%";
        grammarText.text = $"{data.scoreGrammar}%";
        naturalnessText.text = $"{data.scoreNaturalness}%";

        if (monthText != null) monthText.text = data.currentMonth.ToString();

        InitCalendar(data.calendarDays, data.startDayOffset);
    }

    private void InitCalendar(List<CalendarDayData> dayDataList, int offset)
    {
        var grid = dateGrid.GetComponent<GridLayoutGroup>();
        cellSize = grid.cellSize.x;
        spacingX = grid.spacing.x;
        spacingY = grid.spacing.y;
        paddingTop = grid.padding.top;
        paddingLeft = grid.padding.left;

        // 기존에 생성된 칸이 있다면 모두 초기화
        foreach (Transform child in dateGrid) Destroy(child.gameObject);
        foreach (Transform child in streakContainer) Destroy(child.gameObject);

        // 달이 시작하기 전 빈칸(Offset) 만큼 투명한 칸 생성해서 뒤로 밀어주기
        for (int i = 0; i < offset; i++)
        {
            GameObject emptyCell = Instantiate(dateCellPrefab, dateGrid);
            emptyCell.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false); // 글자 지우기
        }

        // 실제 날짜들 생성
        for (int i = 0; i < dayDataList.Count; i++)
        {
            GameObject cell = Instantiate(dateCellPrefab, dateGrid);
            TMP_Text cellText = cell.GetComponentInChildren<TMP_Text>();
            cellText.text = dayDataList[i].day.ToString();

            // 출석하지 않은 날은 글자색을 회색으로 변경
            if (!dayDataList[i].isAttended) cellText.color = new Color32(153, 153, 153, 255);
            else cellText.color = new Color32(255, 138, 61, 255);
        }

            DrawStreakBars(dayDataList, offset);
    }

    private void DrawStreakBars(List<CalendarDayData> days, int offset)
    {
        int startDayIndex = -1;

        for (int i = 0; i < days.Count; i++)
        {
            int gridIndex = i + offset; // 바둑판 전체에서의 실제 위치

            if (days[i].isAttended)
            {
                // 연속 학습 시작점 기록
                if (startDayIndex == -1)
                    startDayIndex = i;

                if (gridIndex % 7 == 6)
                {
                    CreateBar(startDayIndex, i, offset);
                    startDayIndex = -1; // 초기화
                }
            }
            else
            {
                // 출석을 안 했는데, 지금까지 이어지던 연속 학습이 있었다면 막대 생성 후 종료
                if (startDayIndex != -1)
                {
                    CreateBar(startDayIndex, i - 1, offset);
                    startDayIndex = -1;
                }
            }
        }

        // 월말까지 꽉 채워서 연속 학습 중인 경우 마지막 막대 생성
        if (startDayIndex != -1) CreateBar(startDayIndex, days.Count - 1, offset);
    }

    private void CreateBar(int start, int end, int offset)
    {
        int startIndex = start + offset;
        int endIndex = end + offset;

        float startX = paddingLeft + (startIndex % 7) * (cellSize + spacingX);
        float startY = -((startIndex / 7) * (cellSize + spacingY) + streakBarOffsetY); // -(paddingTop + (startIndex / 7) * (cellSize + spacingY))
        float endX = paddingLeft + (endIndex % 7) * (cellSize + spacingX);

        if (start == end)
        {
            GameObject circle = Instantiate(singleCirclePrefab, streakContainer);
            RectTransform rt = circle.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + singleCircleOffsetX, startY - singleCircleOffsetY);
            rt.sizeDelta = new Vector2((endX - startX) + cellSize, rt.sizeDelta.y);
        }
        else
        {
            GameObject bar = Instantiate(streakBarPrefab, streakContainer);
            RectTransform rt = bar.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + streakBarOffsetX, startY - streakBarOffsetY);
            rt.sizeDelta = new Vector2((endX - startX) + cellSize, rt.sizeDelta.y);
        }
    }
}