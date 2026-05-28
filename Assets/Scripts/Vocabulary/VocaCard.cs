using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VocaCard : MonoBehaviour
{
    [SerializeField] private TMP_Text wordText;
    [SerializeField] private TMP_Text meaningText;
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private Button soundBtn;

    public void Setup(WordData data)
    {
        wordText.text = data.word;
        meaningText.text = data.meaning;
        questNameText.text = data.questName;

        soundBtn.onClick.RemoveAllListeners();
        soundBtn.onClick.AddListener(() =>
        {
            Debug.Log($"{data.word} 발음 듣기 재생~!");
        });
    }
}
