using System.Collections;
using UnityEngine;

public class StageReactionController : MonoBehaviour
{
    [Header("Camera / Player")]
    [SerializeField] private GameCameraController cameraController;
    [SerializeField] private PlayerFadeController playerFadeController;
    [SerializeField] private GameObject guideObject;

    [Header("NPC")]
    [SerializeField] private Animator npcAnimator;
    [SerializeField] private Transform npcModel;

    [Header("Coffee Reward")]
    [SerializeField] private GameObject rewardCoffee;
    [SerializeField] private Transform coffeeGivePoint;
    [SerializeField] private float coffeeMoveDuration = 0.6f;

    [Header("Simple NPC Motion")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeAngle = 8f;
    [SerializeField] private float nodDuration = 0.45f;
    [SerializeField] private float nodAngle = 8f;

    private Coroutine npcMotionRoutine;

    public void OnInteractionStarted()
    {
        if (guideObject != null)
            guideObject.SetActive(false);
    }

    public void OnTalkStarted()
    {
        if (cameraController != null)
            cameraController.MoveToTalkView();

        if (playerFadeController != null)
            playerFadeController.FadeOut();
    }

    // 중간 대화 성공
    public void PlayTurnPassedReaction()
    {
        Debug.Log("[StageReaction] Turn Passed");

        if (npcAnimator != null)
            npcAnimator.SetTrigger("Nod");

        if (npcModel != null)
        {
            StopNpcMotionIfNeeded();
            npcMotionRoutine = StartCoroutine(CoNodNpc());
        }
    }

    // 대화 실패
    public void PlayTurnFailedReaction()
    {
        Debug.Log("[StageReaction] Turn Failed");

        if (npcAnimator != null)
            npcAnimator.SetTrigger("WhatHappened");

        if (npcModel != null)
        {
            StopNpcMotionIfNeeded();
            npcMotionRoutine = StartCoroutine(CoShakeNpc());
        }
    }

    // 최종 성공
    public void PlayQuestSuccessReaction()
    {
        Debug.Log("[StageReaction] Quest Success");

        if (npcAnimator != null)
            npcAnimator.SetTrigger("Nod");

        StartCoroutine(CoGiveCoffee());
    }

    // 최종 실패
    public void PlayQuestFailReaction()
    {
        Debug.Log("[StageReaction] Quest Fail");

        if (npcAnimator != null)
            npcAnimator.SetTrigger("WhatHappened");

        if (npcModel != null)
        {
            StopNpcMotionIfNeeded();
            npcMotionRoutine = StartCoroutine(CoShakeNpc());
        }
    }

    private void StopNpcMotionIfNeeded()
    {
        if (npcMotionRoutine != null)
        {
            StopCoroutine(npcMotionRoutine);
            npcMotionRoutine = null;
        }
    }

    private IEnumerator CoShakeNpc()
    {
        Quaternion originalRot = npcModel.localRotation;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shakeDuration);
            float angle = Mathf.Sin(t * Mathf.PI * 4f) * shakeAngle;

            npcModel.localRotation = originalRot * Quaternion.Euler(0f, angle, 0f);

            yield return null;
        }

        npcModel.localRotation = originalRot;
        npcMotionRoutine = null;
    }

    private IEnumerator CoNodNpc()
    {
        Quaternion originalRot = npcModel.localRotation;
        float elapsed = 0f;

        while (elapsed < nodDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / nodDuration);
            float angle = Mathf.Sin(t * Mathf.PI * 2f) * nodAngle;

            npcModel.localRotation = originalRot * Quaternion.Euler(angle, 0f, 0f);

            yield return null;
        }

        npcModel.localRotation = originalRot;
        npcMotionRoutine = null;
    }

    private IEnumerator CoGiveCoffee()
    {
        if (rewardCoffee == null || coffeeGivePoint == null)
            yield break;

        rewardCoffee.SetActive(true);

        Vector3 startPos = rewardCoffee.transform.position;
        Quaternion startRot = rewardCoffee.transform.rotation;

        Vector3 endPos = coffeeGivePoint.position;
        Quaternion endRot = coffeeGivePoint.rotation;

        float elapsed = 0f;

        while (elapsed < coffeeMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / coffeeMoveDuration);
            t = t * t * (3f - 2f * t);

            rewardCoffee.transform.position = Vector3.Lerp(startPos, endPos, t);
            rewardCoffee.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        rewardCoffee.transform.position = endPos;
        rewardCoffee.transform.rotation = endRot;
    }

    // 테스트용
    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            PlayTurnPassedReaction();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            PlayTurnFailedReaction();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            PlayQuestSuccessReaction();
    }*/
}