/* InteractionCameraController.cs
기존 GameCameraController의 공통 버전 (원하는 transform으로 이동할 수 있게 수정) */

using System.Collections;
using UnityEngine;

public class InteractionCameraController : MonoBehaviour
{
    [SerializeField] private Transform defaultCameraPoint;
    [SerializeField] private float transitionDuration = 0.5f;

    private Coroutine transitionRoutine;

    public void MoveToDefaultView()
    {
        MoveTo(defaultCameraPoint);
    }

    public void MoveTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("[InteractionCameraController] Camera target is null.");
            return;
        }

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(MoveRoutine(target));
    }

    private IEnumerator MoveRoutine(Transform target)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;
        transitionRoutine = null;
    }
}