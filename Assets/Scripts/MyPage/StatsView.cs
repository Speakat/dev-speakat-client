using UnityEngine;
using TMPro;

public class StatsView : MonoBehaviour
{
    [SerializeField] private TMP_Text meaningText;
    [SerializeField] private TMP_Text grammarText;
    [SerializeField] private TMP_Text naturalnessText;

    public void Setup(int meaning, int grammar, int naturalness)
    {
        meaningText.text = $"{meaning}%";
        grammarText.text = $"{grammar}%";
        naturalnessText.text = $"{naturalness}%";
    }
}
