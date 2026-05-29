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

    public void SetResultUI(float context, float grammar, float expression)
    {
        SetScore(context, grammar, expression);
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
