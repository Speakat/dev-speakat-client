using TMPro;
using UnityEngine;
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

    private void Start()
    {
        SetScore(0.85f, 0.9f, 0.8f); // 예시 점수 설정
    }

    // 해당 퀘스트 재시작
    private void ReplayQuest()
    {
        Debug.Log("퀘스트를 재시작합니다.");
    }

    // 다음 퀘스트로 이동
    private void ContinueQuest()
    {
        Debug.Log("다음 퀘스트로 넘어갑니다.");
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
