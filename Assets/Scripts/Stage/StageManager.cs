using System;
using System.Threading.Tasks;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private StageScrollUIController stageScrollUIController;
    [SerializeField] private StageApiService stageApiService;

    private async void Start()
    {
        await RefreshStageList();
    }

    public async Task RefreshStageList()
    {
        try
        {
            if (stageApiService == null)
            {
                throw new InvalidOperationException("[StageManager] stageApiService is not assigned.");
            }

            if (stageScrollUIController == null)
            {
                throw new InvalidOperationException("[StageManager] stageScrollUIController is not assigned.");
            }

            StageList stageList = await stageApiService.GetStageListAsync();
            SceneContext.SetStageListData(stageList);
            stageScrollUIController.SetStageScrollUI(stageList);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[StageManager] 스테이지 목록 로드 실패: {ApiErrorMessage.From(exception)}");
        }
    }
}
