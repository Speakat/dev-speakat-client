using System.Collections;
using UnityEngine;

public class GameCameraController : MonoBehaviour
{
    [SerializeField] private Transform exploreCameraPoint;
    [SerializeField] private Transform talkCameraPoint;
    [SerializeField] private float transitionDuration = 0.5f;

    private Coroutine transitionRoutine;

    public void MoveToExploreView()
    {
        MoveTo(exploreCameraPoint);
    }

    public void MoveToTalkView()
    {
        MoveTo(talkCameraPoint);
    }

    private void MoveTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("[GameCameraController] Camera target is null.");
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

            // 부드러운 ease in/out
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