using UnityEngine;
using UnityEngine.UI;
using TMPro;

// API에서 받아올 데이터용 (일단은 더미)
public class MyPageData
{
    public string nickname;
    public int level;
    public string currentCourse;
    public float expProgress; // Progress bar, 경험치 바 / 0.0f~1.0f
    public int currentStreak;
    public int scoreMeaning;
    public int scoreGrammar;
    public int scoreNaturalness;
}

public class MyPageView : MonoBehaviour
{
    [Header("Profile Area")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text courseText;
    [SerializeField] private Slider expProgressBar;

    [Header("Streak Area")]
    [SerializeField] private TMP_Text streakDaysText;

    [Header("Stats Area")]
    [SerializeField] private TMP_Text meaningText;
    [SerializeField] private TMP_Text grammarText;
    [SerializeField] private TMP_Text naturalnessText;

    void Start()
    {
        // 일단
        MyPageData dummyData = new MyPageData
        {
            nickname = "스피캣",
            level = 7,
            currentCourse = "A2 초급 영어",
            expProgress = 0.7f, // 70% 정도
            currentStreak = 14,
            scoreMeaning = 87,
            scoreGrammar = 87,
            scoreNaturalness = 87
        };

        UpdateUI(dummyData);
    }

    public void UpdateUI(MyPageData data)
    {
        nicknameText.text = $"{data.nickname} 님";
        levelText.text = $"Lv.{data.level}";
        courseText.text = data.currentCourse;
        expProgressBar.value = data.expProgress;

        streakDaysText.text = $"{data.currentStreak}일째 연속 학습!";

        meaningText.text = $"{data.scoreMeaning}%";
        grammarText.text = $"{data.scoreGrammar}%";
        naturalnessText.text = $"{data.scoreNaturalness}%";
    }
}
