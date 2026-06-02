using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VocaCard : MonoBehaviour
{
    [SerializeField] private TMP_Text wordText;
    [SerializeField] private TMP_Text meaningText;
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private Button soundBtn;

    private WordData currentData;
    private Action<WordData> onClickSound;

    public void Setup(WordData data, Action<WordData> onClickSoundCallback = null)
    {
        currentData = data;
        onClickSound = onClickSoundCallback;

        if (wordText != null)
            wordText.text = data.word;

        if (meaningText != null)
            meaningText.text = data.meaning;

        if (questNameText != null)
            questNameText.text = data.questName;

        if (soundBtn != null)
        {
            soundBtn.onClick.RemoveAllListeners();
            soundBtn.onClick.AddListener(() =>
            {
                if (currentData == null)
                {
                    Debug.LogWarning("[VocaCard] currentData가 없습니다.");
                    return;
                }

                Debug.Log($"[VocaCard] 발음 버튼 클릭: word={currentData.word}, flashcardId={currentData.flashcardId}, audioUrl={currentData.audioUrl}");

                if (onClickSound != null)
                    onClickSound.Invoke(currentData);
                else
                    Debug.LogWarning("[VocaCard] onClickSound 콜백이 연결되지 않았습니다.");
            });
        }
    }

    public void SetSoundButtonInteractable(bool interactable)
    {
        if (soundBtn != null)
            soundBtn.interactable = interactable;
    }
}