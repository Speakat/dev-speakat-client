using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUIController : MonoBehaviour
{
    [SerializeField]
    private Button replayButton;
    [SerializeField]
    private Button continueButton;

    public float totalScore;

    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI contextRelevanceScoreText;
    [SerializeField]
    private TextMeshProUGUI grammarAccuracyScoreText;
    [SerializeField]
    private TextMeshProUGUI expressionQualityScoreText;

    [SerializeField]
    private TextMeshProUGUI resultTitleText; // 성공 실패에 따른 텍스트 표시
    [SerializeField]
    private TextMeshProUGUI resultMessageText; // 성공 실패에 따른 텍스트 표시

    private string successTitle = "잘했어요!";
    private string successMessage = "퀘스트를 성공적으로 완료했어요!";
    private string failureTitle = "아쉽네요!";
    private string failureMessage = "퀘스트를 실패했어요. 다시 도전해 보세요!";

    [SerializeField]
    private TextMeshProUGUI questButtonText;
    private string suceessButtonText = "다음 퀘스트";
    private string failureButtonText = "퀘스트 목록";

    [SerializeField]
    private Image scoreImage;

    private void Awake()
    {
        replayButton.onClick.AddListener(ReplayQuest);
        continueButton.onClick.AddListener(ContinueQuest);
    }

    // 해당 퀘스트 재시작
    private void ReplayQuest()
    {
        GamePlayManager.Instance.Restart();
    }

    // 다음 퀘스트로 이동
    private void ContinueQuest()
    {
        SceneManager.LoadScene("QuestScene");
    }

    public void SetResultUI(float context, float grammar, float expression, bool isQuestSuccess)
    {
        SetScore(context, grammar, expression);

        if (isQuestSuccess)
        {
            resultTitleText.text = successTitle;
            resultMessageText.text = successMessage;
            questButtonText.text = suceessButtonText;
        }
        else
        {
            resultTitleText.text = failureTitle;
            resultMessageText.text = failureMessage;
            questButtonText.text = failureButtonText;
        }

        gameObject.SetActive(true);
    }

    public void SetScore(float context, float grammar, float expression)
    {
        contextRelevanceScoreText.text = $"{context * 100}%";
        grammarAccuracyScoreText.text = $"{grammar * 100}%";
        expressionQualityScoreText.text = $"{expression * 100}%";

        totalScore = (context + grammar + expression) / 3f;
        scoreText.text = $"{totalScore * 100:F0}%";

        scoreImage.fillAmount = totalScore;
    }
}
