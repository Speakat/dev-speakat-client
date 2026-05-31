using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Speakat.Client;

[System.Serializable]
public class MyProfileData
{
    public string userId;
    public string nickname;
    public string profileImageUrl;
    public string englishLevel;
}

[System.Serializable]
public class MyStatsData
{
    public int? semanticScore;
    public int? grammarScore;
    public int? naturalnessScore;

    public double? avgSemanticScore;
    public double? avgGrammarScore;
    public double? avgNaturalnessScore;
}

[System.Serializable]
public class ApiResponseOfMyStatsData
{
    public bool? isSuccess;
    public MyStatsData data;
    public string code;
    public string message;
}

[System.Serializable]
public class ApiResponseOfMyProfileData
{
    public bool? isSuccess;
    public MyProfileData data;
    public string code;
    public string message;
}

[System.Serializable]
public class CalendarDayData
{
    public int day;
    public bool isAttended;

    public int sessionsCount;
    public int wordsLearned;
    public int? averageScore;
}

// API에서 받아올 데이터용
public class MyPageData
{
    public string profileImageUrl;
    public string nickname;
    public string currentCourse;
    public int currentStreak;

    public int scoreMeaning;
    public int scoreGrammar;
    public int scoreNaturalness;

    public int currentMonth;
    public int startDayOffset;
    public List<CalendarDayData> calendarDays;
}

public class MyPageView : MonoBehaviour
{
    [Header("Child Views")]
    [SerializeField] private ProfileView profileView;
    [SerializeField] private StatsView statsView;
    [SerializeField] private CalendarView calendarView;

    [Header("Own UI")]
    [SerializeField] private TMP_Text streakDaysText;
    [SerializeField] private Image myTabIcon;
    [SerializeField] private Color activeColor = new Color32(255, 138, 61, 255);
    [SerializeField] private Color inactiveColor = new Color32(170, 170, 170, 255);

    [Header("API")]
    [SerializeField] private MyPageApiService myPageApiService;
    [SerializeField] private bool useDummyOnApiFail = true;

    private int currentDisplayYear;
    private int currentDisplayMonth;

    private async void Start()
    {
        System.DateTime now = System.DateTime.Now;
        currentDisplayYear = now.Year;
        currentDisplayMonth = now.Month;

        SetMyTabActive(true);
        await RefreshDataAsync();
    }

    public async void OnClickPrevMonth()
    {
        currentDisplayMonth--;

        if (currentDisplayMonth < 1)
        {
            currentDisplayMonth = 12;
            currentDisplayYear--;
        }

        await RefreshDataAsync();
    }

    public async void OnClickNextMonth()
    {
        currentDisplayMonth++;

        if (currentDisplayMonth > 12)
        {
            currentDisplayMonth = 1;
            currentDisplayYear++;
        }

        await RefreshDataAsync();
    }

    private async System.Threading.Tasks.Task RefreshDataAsync()
    {
        if (myPageApiService == null)
        {
            Debug.LogWarning("[MyPageView] myPageApiService가 연결되지 않아 더미 데이터를 사용합니다.");
            LoadDummyData();
            return;
        }

        try
        {
            Debug.Log("[MyPageView] GET /users/me 시작");
            var profile = await myPageApiService.GetMyProfileAsync();
            Debug.Log($"[MyPageView] GET /users/me 성공: nickname={profile.nickname}, level={profile.englishLevel}");

            Debug.Log("[MyPageView] GET /users/me/stats 시작");
            var stats = await myPageApiService.GetStatsAsync();
            Debug.Log("[MyPageView] GET /users/me/stats 성공");

            Debug.Log("[MyPageView] GET /users/me/streak 시작");
            var streak = await myPageApiService.GetStreakAsync();
            Debug.Log("[MyPageView] GET /users/me/streak 성공");

            Debug.Log($"[MyPageView] GET /users/me/calendar 시작: year={currentDisplayYear}, month={currentDisplayMonth}");
            var calendar = await myPageApiService.GetCalendarAsync(currentDisplayYear, currentDisplayMonth);
            Debug.Log("[MyPageView] GET /users/me/calendar 성공");

            MyPageData data = new MyPageData
            {
                profileImageUrl = profile.profileImageUrl,
                nickname = profile.nickname ?? "학습자",
                currentCourse = ConvertEnglishLevel(profile.englishLevel),

                currentStreak = streak.CurrentStreak.GetValueOrDefault(),

                scoreMeaning = GetScore(stats.semanticScore, stats.avgSemanticScore),
                scoreGrammar = GetScore(stats.grammarScore, stats.avgGrammarScore),
                scoreNaturalness = GetScore(stats.naturalnessScore, stats.avgNaturalnessScore),

                currentMonth = currentDisplayMonth,
                startDayOffset = GetStartDayOffset(currentDisplayYear, currentDisplayMonth),
                calendarDays = ConvertCalendarDays(currentDisplayYear, currentDisplayMonth, calendar)
            };

            UpdateUI(data);

            Debug.Log("[MyPageView] 마이페이지 API 로드 성공");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MyPageView] 마이페이지 API 로드 실패 상세:\n{e}");

            if (useDummyOnApiFail)
            {
                LoadDummyData();
            }
        }
    }

    private void LoadDummyData()
    {
        List<CalendarDayData> newDays = new List<CalendarDayData>();

        int daysInMonth = System.DateTime.DaysInMonth(currentDisplayYear, currentDisplayMonth);

        for (int i = 1; i <= daysInMonth; i++)
        {
            bool attended = i % 3 == 0 || i % 7 == 0;

            newDays.Add(new CalendarDayData
            {
                day = i,
                isAttended = attended,
                sessionsCount = attended ? 1 : 0,
                wordsLearned = attended ? 5 : 0,
                averageScore = attended ? 80 : null
            });
        }

        MyPageData dummyData = new MyPageData
        {
            nickname = "스피캣",
            currentCourse = "A2 · 초급 영어",
            currentStreak = 14,
            scoreMeaning = 87,
            scoreGrammar = 87,
            scoreNaturalness = 87,
            currentMonth = currentDisplayMonth,
            startDayOffset = GetStartDayOffset(currentDisplayYear, currentDisplayMonth),
            calendarDays = newDays
        };

        UpdateUI(dummyData);
    }

    private string ConvertEnglishLevel(object englishLevel)
    {
        if (englishLevel == null)
            return "레벨 정보 없음";

        return englishLevel.ToString() switch
        {
            "BEGINNER" => "입문 영어",
            "ELEMENTARY" => "초급 영어",
            "INTERMEDIATE" => "중급 영어",
            "UPPER_INTERMEDIATE" => "중상급 영어",
            "ADVANCED" => "고급 영어",
            _ => englishLevel.ToString()
        };
    }

    private int GetStartDayOffset(int year, int month)
    {
        System.DateTime firstDay = new System.DateTime(year, month, 1);
        return (int)firstDay.DayOfWeek;
    }

    private List<CalendarDayData> ConvertCalendarDays(int year, int month, UserCalendarDto calendar)
    {
        int daysInMonth = System.DateTime.DaysInMonth(year, month);
        List<CalendarDayData> result = new List<CalendarDayData>();

        Dictionary<int, CalendarDayData> recordByDay = new Dictionary<int, CalendarDayData>();

        if (calendar?.Days != null)
        {
            foreach (var record in calendar.Days)
            {
                if (record?.Date == null)
                    continue;

                int day = record.Date.Value.Day;
                int sessionCount = record.SessionCount.GetValueOrDefault();

                recordByDay[day] = new CalendarDayData
                {
                    day = day,
                    isAttended = sessionCount > 0,
                    sessionsCount = sessionCount,

                    // 현재 SDK CalendarDayDto에는 없음
                    wordsLearned = 0,
                    averageScore = null
                };
            }
        }

        for (int day = 1; day <= daysInMonth; day++)
        {
            if (recordByDay.TryGetValue(day, out var record))
            {
                result.Add(record);
            }
            else
            {
                result.Add(new CalendarDayData
                {
                    day = day,
                    isAttended = false,
                    sessionsCount = 0,
                    wordsLearned = 0,
                    averageScore = null
                });
            }
        }

        return result;
    }

    private int GetScore(int? directScore, double? averageScore)
    {
        if (directScore.HasValue)
            return directScore.Value;

        if (averageScore.HasValue)
            return Mathf.RoundToInt((float)averageScore.Value);

        return 0;
    }

    public void UpdateUI(MyPageData data)
    {
        if (profileView != null)
            profileView.Setup(data.nickname, data.currentCourse, data.profileImageUrl);

        if (statsView != null)
            statsView.Setup(data.scoreMeaning, data.scoreGrammar, data.scoreNaturalness);

        if (calendarView != null)
            calendarView.Setup(data.currentMonth, data.calendarDays, data.startDayOffset);

        if (streakDaysText != null)
            streakDaysText.text = $"{data.currentStreak}일째 연속 학습!";
    }

    public void SetMyTabActive(bool isActive)
    {
        if (myTabIcon != null)
        {
            myTabIcon.color = isActive ? activeColor : inactiveColor;

            var btn = myTabIcon.GetComponentInParent<Button>();
            if (btn != null)
                btn.interactable = !isActive;
        }
    }
}