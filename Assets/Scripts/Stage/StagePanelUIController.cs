using TMPro;
using UnityEngine;

public class StagePanelUIController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI stageNameText; // 스테이지 이름 텍스트

    public void SetStageName(string stageName)
    {
        stageNameText.text = stageName;
    }
}
