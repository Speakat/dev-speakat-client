using System;
using NUnit.Framework;
using UnityEngine;

public class ApiServiceValidationTests
{
    private GameObject testObject;

    [SetUp]
    public void SetUp() => testObject = new GameObject(nameof(ApiServiceValidationTests));

    [TearDown]
    public void TearDown() => UnityEngine.Object.DestroyImmediate(testObject);

    [TestCase(0)]
    [TestCase(-1)]
    public void QuestApiService_GetStageDetailAsync_RejectsNonPositiveStageId(int stageId)
    {
        QuestApiService service = testObject.AddComponent<QuestApiService>();
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await service.GetStageDetailAsync(stageId));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void QuestApiService_GetQuestDetailAsync_RejectsNonPositiveQuestId(int questId)
    {
        QuestApiService service = testObject.AddComponent<QuestApiService>();
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await service.GetQuestDetailAsync(questId));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void GamePlayApiService_StartSessionAsync_RejectsNonPositiveQuestId(int questId)
    {
        GamePlayApiService service = testObject.AddComponent<GamePlayApiService>();
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await service.StartSessionAsync(questId));
    }

    [Test]
    public void GamePlayApiService_SubmitSpeechAsync_RejectsMissingSessionId()
    {
        GamePlayApiService service = testObject.AddComponent<GamePlayApiService>();
        Assert.ThrowsAsync<ArgumentException>(async () => await service.SubmitSpeechAsync(null, 1, 1, nameof(service)));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void GamePlayApiService_SubmitSpeechAsync_RejectsNonPositiveQuestId(int questId)
    {
        GamePlayApiService service = testObject.AddComponent<GamePlayApiService>();
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await service.SubmitSpeechAsync(nameof(service), questId, 1, nameof(service)));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void GamePlayApiService_SubmitSpeechAsync_RejectsNonPositiveTurn(int turn)
    {
        GamePlayApiService service = testObject.AddComponent<GamePlayApiService>();
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await service.SubmitSpeechAsync(nameof(service), 1, turn, nameof(service)));
    }

    [Test]
    public void GamePlayApiService_SubmitSpeechAsync_RejectsMissingAudio()
    {
        GamePlayApiService service = testObject.AddComponent<GamePlayApiService>();
        Assert.ThrowsAsync<ArgumentException>(async () => await service.SubmitSpeechAsync(nameof(service), 1, 1, null));
    }

    [Test]
    public void GamePlayApiService_EndSessionAsync_RejectsMissingSessionId()
    {
        GamePlayApiService service = testObject.AddComponent<GamePlayApiService>();
        Assert.ThrowsAsync<ArgumentException>(async () => await service.EndSessionAsync(null));
    }

    [Test]
    public void FlashcardApiService_SaveAsync_RequiresApiProvider()
    {
        FlashcardApiService service = testObject.AddComponent<FlashcardApiService>();
        Assert.ThrowsAsync<InvalidOperationException>(async () => await service.SaveAsync(1, nameof(service), null));
    }
}
