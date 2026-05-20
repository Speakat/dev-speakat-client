using UnityEngine;
using UnityEngine.UI;

public class ResultUIController : MonoBehaviour
{
    [SerializeField]
    private Button replayButton;
    [SerializeField]
    private Button continueButton;

    private void Awake()
    {
        replayButton.onClick.AddListener(ReplayQuest);
        continueButton.onClick.AddListener(ContinueQuest);
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
}
