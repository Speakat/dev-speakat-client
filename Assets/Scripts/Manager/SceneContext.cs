public static class SceneContext
{
    public static int SelectedStageId { get; private set; }
    public static int SelectedQuestId { get; private set; }

    public static void SetSelectedStage(int stageId)
    {
        SelectedStageId = stageId;
    }

    public static void SetSelectedQuest(int questId)
    {
        SelectedQuestId = questId;
    }

    public static void Clear()
    {
        SelectedStageId = 0;
    }  
}