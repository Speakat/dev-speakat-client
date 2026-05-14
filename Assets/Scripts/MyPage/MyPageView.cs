using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;

[System.Serializable]
public class CalendarDayData
{
    public int day;
    public bool isAttended;
}

// API에서 받아올 데이터용
public class MyPageData
{
    public string profileImageUrl;
    public string nickname;
    //public int level;
    public string currentCourse;
    //public float expProgress; // Progress bar, 경험치 바 / 0.0f~1.0f
    public int currentStreak;

    public int scoreMeaning;
    public int scoreGrammar;
    public int scoreNaturalness;

    public int currentMonth;
    public int startDayOffset; // 1일 앞 빈칸 개수 (수욜 시작인 경우 앞에 두 칸)
    public List<CalendarDayData> calendarDays; // 이번 달 전체 날짜 데이터
}

public class MyPageView : MonoBehaviour
{
    [Header("Child Views")]
    [SerializeField] private ProfileView profileView;
    [SerializeField] private StatsView statsView;
    [SerializeField] private CalendarView calendarView;

    [Header("Own UI ")]
    [SerializeField] private TMP_Text streakDaysText;
    [SerializeField] private Image myTabIcon;
    [SerializeField] private Color activeColor = new Color32(255, 138, 61, 255);
    [SerializeField] private Color inactiveColor = new Color32(170, 170, 170, 255);

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
            //expProgress = 0.7f, // 70%
            currentStreak = 14,
            scoreMeaning = 87,
            scoreGrammar = 87,
            scoreNaturalness = 87,
            currentMonth = 10,
            startDayOffset = 3,
            calendarDays = dummyDays
        };

        UpdateUI(dummyData);
        SetMyTabActive(true);
    }

    public void UpdateUI(MyPageData data)
    {
        if (profileView != null) profileView.Setup(data.nickname, data.currentCourse, data.profileImageUrl);
        if (statsView != null) statsView.Setup(data.scoreMeaning, data.scoreGrammar, data.scoreNaturalness);
        if (calendarView != null) calendarView.Setup(data.currentMonth, data.calendarDays, data.startDayOffset);

        if (streakDaysText != null) streakDaysText.text = $"{data.currentStreak}일째 연속 학습!";
    }

    public void SetMyTabActive(bool isActive)
    {
        if (myTabIcon != null)
        {
            myTabIcon.color = isActive ? activeColor : inactiveColor;

            var btn = myTabIcon.GetComponentInParent<Button>();
            if (btn != null) btn.interactable = !isActive;
        }
    }
}