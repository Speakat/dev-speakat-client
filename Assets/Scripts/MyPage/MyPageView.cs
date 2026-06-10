using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Speakat.Client;
using UnityEngine.SceneManagement;

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

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button editProfileButton;
    [SerializeField] private Button deleteAccountButton;
    [SerializeField] private Toggle showNpcScriptToggle;

    [Header("Delete Account Popup")]
    [SerializeField] private GameObject deleteConfirmPopup;
    [SerializeField] private Button confirmDeleteButton;
    [SerializeField] private Button cancelDeleteButton;
    [SerializeField] private string loginSceneName = "Login";

    [Header("Edit Profile Popup")]
    [SerializeField] private GameObject editProfilePopup;
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private Button saveProfileButton;
    [SerializeField] private Button cancelProfileButton;

    [SerializeField] private TMP_Text nicknameErrorText;

    [SerializeField] private Button logoutButton;

    private string currentNickname;

    private bool isUpdatingShowNpcScript;

    private int currentDisplayYear;
    private int currentDisplayMonth;

    private async void Start()
    {
        System.DateTime now = System.DateTime.Now;
        currentDisplayYear = now.Year;
        currentDisplayMonth = now.Month;

        SetMyTabActive(true);
        BindSettingsEvents();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (deleteConfirmPopup != null)
            deleteConfirmPopup.SetActive(false);

        if (editProfilePopup != null)
            editProfilePopup.SetActive(false);

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
            currentNickname = profile.nickname ?? "학습자";
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

            MyPageApiService.MySettingsData settings = null;

            try
            {
                Debug.Log("[MyPageView] GET /users/me/settings 시작");
                settings = await myPageApiService.GetSettingsAsync();
                Debug.Log("[MyPageView] GET /users/me/settings 성공");
            }
            catch (System.Exception settingsError)
            {
                Debug.LogWarning($"[MyPageView] 설정 조회 실패. 기본값으로 진행합니다.\n{settingsError}");
            }

            MyPageData data = new MyPageData
            {
                profileImageUrl = profile.profileImageUrl,
                nickname = profile.nickname ?? "학습자",
                // currentCourse = ConvertEnglishLevel(profile.englishLevel),
                currentCourse = GetLearningLevelText(stats),

                currentStreak = streak.CurrentStreak.GetValueOrDefault(),

                scoreMeaning = GetScore(stats.semanticScore, stats.avgSemanticScore),
                scoreGrammar = GetScore(stats.grammarScore, stats.avgGrammarScore),
                scoreNaturalness = GetScore(stats.naturalnessScore, stats.avgNaturalnessScore),

                currentMonth = currentDisplayMonth,
                startDayOffset = GetStartDayOffset(currentDisplayYear, currentDisplayMonth),
                calendarDays = ConvertCalendarDays(currentDisplayYear, currentDisplayMonth, calendar)
            };

            UpdateUI(data);

            if (showNpcScriptToggle != null)
            {
                bool showNpcScript = true;

                if (settings != null)
                    showNpcScript = settings.showNpcScript.GetValueOrDefault(true);

                showNpcScriptToggle.SetIsOnWithoutNotify(showNpcScript);
            }

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

    private string GetLearningLevelText(MyStatsData stats)
    {
        if (stats == null)
            return "학습 통계 없음";

        int meaning = GetScore(stats.semanticScore, stats.avgSemanticScore);
        int grammar = GetScore(stats.grammarScore, stats.avgGrammarScore);
        int naturalness = GetScore(stats.naturalnessScore, stats.avgNaturalnessScore);

        string meaningLevel = ConvertScoreToLevel(meaning);
        string grammarLevel = ConvertScoreToLevel(grammar);
        string naturalnessLevel = ConvertScoreToLevel(naturalness);

        return $"의미 {meaningLevel} · 문법 {grammarLevel} · 자연스러움 {naturalnessLevel}";
    }

    private string ConvertScoreToLevel(int score)
    {
        if (score >= 80)
            return "상";

        if (score >= 50)
            return "중";

        return "하";
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

    private void BindSettingsEvents()
    {
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OpenSettingsPanel);
            Debug.Log("[MyPageView] SettingsButton 연결 완료");
        }
        else
        {
            Debug.LogWarning("[MyPageView] SettingsButton이 연결되지 않았습니다.");
        }

        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.RemoveAllListeners();
            settingsBackButton.onClick.AddListener(CloseSettingsPanel);
        }
        else
        {
            Debug.LogWarning("[MyPageView] SettingsBackButton이 연결되지 않았습니다.");
        }

        if (editProfileButton != null)
        {
            editProfileButton.onClick.RemoveAllListeners();
            editProfileButton.onClick.AddListener(OnClickEditProfile);
        }

        if (deleteAccountButton != null)
        {
            deleteAccountButton.onClick.RemoveAllListeners();
            deleteAccountButton.onClick.AddListener(OnClickDeleteAccount);
        }

        if (showNpcScriptToggle != null)
        {
            showNpcScriptToggle.onValueChanged.RemoveAllListeners();
            showNpcScriptToggle.onValueChanged.AddListener(OnToggleShowNpcScript);
        }

        if (confirmDeleteButton != null)
        {
            confirmDeleteButton.onClick.RemoveAllListeners();
            confirmDeleteButton.onClick.AddListener(OnConfirmDeleteAccount);
        }

        if (cancelDeleteButton != null)
        {
            cancelDeleteButton.onClick.RemoveAllListeners();
            cancelDeleteButton.onClick.AddListener(CloseDeleteConfirmPopup);
        }

        if (saveProfileButton != null)
        {
            saveProfileButton.onClick.RemoveAllListeners();
            saveProfileButton.onClick.AddListener(OnClickSaveProfile);
        }

        if (cancelProfileButton != null)
        {
            cancelProfileButton.onClick.RemoveAllListeners();
            cancelProfileButton.onClick.AddListener(CloseEditProfilePopup);
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.RemoveAllListeners();
            logoutButton.onClick.AddListener(OnClickLogout);
        }
    }

    public void OpenSettingsPanel()
    {
        Debug.Log("[MyPageView] Settings 버튼 클릭됨");

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        else
            Debug.LogWarning("[MyPageView] SettingsPanel이 연결되지 않았습니다.");
    }

    private void CloseSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnClickEditProfile()
    {
        Debug.Log("[MyPageView] 프로필 수정 클릭");

        if (nicknameInputField != null)
            nicknameInputField.text = currentNickname ?? "";

        if (editProfilePopup != null)
            editProfilePopup.SetActive(true);

        if (nicknameErrorText != null)
            nicknameErrorText.text = "";
    }

    private void CloseEditProfilePopup()
    {
        if (editProfilePopup != null)
            editProfilePopup.SetActive(false);
    }

    private async void OnClickSaveProfile()
    {
        if (myPageApiService == null)
        {
            Debug.LogError("[MyPageView] myPageApiService가 연결되지 않았습니다.");
            return;
        }

        if (nicknameInputField == null)
        {
            Debug.LogError("[MyPageView] nicknameInputField가 연결되지 않았습니다.");
            return;
        }

        if (nicknameErrorText != null)
            nicknameErrorText.text = "";

        string newNickname = nicknameInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(newNickname))
        {
            if (nicknameErrorText != null)
                nicknameErrorText.text = "닉네임을 입력해 주세요.";

            Debug.LogWarning("[MyPageView] 닉네임이 비어 있습니다.");
            return;
        }

        if (!string.IsNullOrEmpty(currentNickname) && newNickname == currentNickname)
        {
            Debug.Log("[MyPageView] 기존 닉네임과 동일하여 수정하지 않습니다.");

            if (nicknameErrorText != null)
                nicknameErrorText.text = "현재 사용 중인 닉네임과 같습니다.";

            return;
        }

        if (saveProfileButton != null)
            saveProfileButton.interactable = false;

        try
        {
            await myPageApiService.UpdateProfileAsync(newNickname);

            Debug.Log("[MyPageView] 프로필 수정 성공");

            currentNickname = newNickname;

            CloseEditProfilePopup();
            CloseSettingsPanel();

            await RefreshDataAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MyPageView] 프로필 수정 실패:\n{e}");

            if (nicknameErrorText != null)
                nicknameErrorText.text = GetProfileUpdateErrorMessage(e);
        }
        finally
        {
            if (saveProfileButton != null)
                saveProfileButton.interactable = true;
        }
    }

    private void OnClickDeleteAccount()
    {
        Debug.Log("[MyPageView] 회원 탈퇴 클릭");

        if (deleteConfirmPopup != null)
            deleteConfirmPopup.SetActive(true);
    }

    private void CloseDeleteConfirmPopup()
    {
        if (deleteConfirmPopup != null)
            deleteConfirmPopup.SetActive(false);
    }

    private async void OnConfirmDeleteAccount()
    {
        Debug.Log("[MyPageView] 회원 탈퇴 확인");

        if (myPageApiService == null)
        {
            Debug.LogError("[MyPageView] myPageApiService가 연결되지 않았습니다.");
            return;
        }

        if (confirmDeleteButton != null)
            confirmDeleteButton.interactable = false;

        try
        {
            await myPageApiService.DeleteMeAsync();

            Debug.Log("[MyPageView] 회원 탈퇴 API 성공");

            ClearLocalAuthData();

            SceneManager.LoadScene(loginSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MyPageView] 회원 탈퇴 실패:\n{e}");

            if (confirmDeleteButton != null)
                confirmDeleteButton.interactable = true;
        }
    }

    private async void OnToggleShowNpcScript(bool isOn)
    {
        if (isUpdatingShowNpcScript)
            return;

        Debug.Log($"[MyPageView] NPC 대사 표시 설정 변경: {isOn}");

        if (myPageApiService == null)
        {
            Debug.LogError("[MyPageView] myPageApiService가 연결되지 않았습니다.");
            return;
        }

        isUpdatingShowNpcScript = true;

        if (showNpcScriptToggle != null)
            showNpcScriptToggle.interactable = false;

        try
        {
            await myPageApiService.UpdateSettingsAsync(isOn);
            Debug.Log("[MyPageView] NPC 대사 표시 설정 저장 성공");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MyPageView] NPC 대사 표시 설정 저장 실패:\n{e}");

            // 실패하면 UI를 원래 상태로 되돌림
            if (showNpcScriptToggle != null)
                showNpcScriptToggle.SetIsOnWithoutNotify(!isOn);
        }
        finally
        {
            if (showNpcScriptToggle != null)
                showNpcScriptToggle.interactable = true;

            isUpdatingShowNpcScript = false;
        }
    }

    private async void OnClickLogout()
    {
        Debug.Log("[MyPageView] 로그아웃 클릭");

        if (myPageApiService == null)
        {
            Debug.LogError("[MyPageView] myPageApiService가 연결되지 않았습니다.");
            return;
        }

        string refreshToken = GetRefreshTokenFromStore();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            Debug.LogWarning("[MyPageView] refreshToken이 없어 서버 로그아웃 없이 로그인 화면으로 이동합니다.");
            ClearLocalAuthData();
            SceneManager.LoadScene(loginSceneName);
            return;
        }

        if (logoutButton != null)
            logoutButton.interactable = false;

        try
        {
            await myPageApiService.LogoutAsync(refreshToken);

            Debug.Log("[MyPageView] 로그아웃 성공");

            ClearLocalAuthData();

            SceneManager.LoadScene(loginSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MyPageView] 로그아웃 실패:\n{e}");
        }
        finally
        {
            if (logoutButton != null)
                logoutButton.interactable = true;
        }
    }

    private string GetRefreshTokenFromStore()
    {
        if (TokenStore.Instance == null)
            return null;

        return TokenStore.Instance.RefreshToken;
    }

    private void ClearLocalAuthData()
    {
        if (TokenStore.Instance == null)
            return;

        TokenStore.Instance.Clear();
    }

    private string GetProfileUpdateErrorMessage(System.Exception e)
    {
        string message = e.ToString();

        if (message.Contains("DUPLICATE_NICKNAME"))
            return "이미 사용 중인 닉네임입니다.";

        if (message.Contains("INVALID_REQUEST"))
            return "닉네임 형식이 올바르지 않습니다.";

        return "프로필 수정에 실패했습니다. 잠시 후 다시 시도해 주세요.";
    }
}