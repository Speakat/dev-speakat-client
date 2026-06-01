using System.Collections;
using UnityEngine;

public class InteractableNpc : MonoBehaviour
{
    [Header("NPC Info")]
    [SerializeField] private string npcId = "barista";

    [Header("References")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private PlayerClickMover player;
    [SerializeField] private GameCameraController cameraController;
    [SerializeField] private GameObject guideObject;
    [SerializeField] private PlayerFadeController playerFadeController;

    [Header("Options")]
    [SerializeField] private bool interactOnlyOnce = false;
    [SerializeField] private float fadeDelay = 0.1f;

    private bool hasInteracted;

    private void OnMouseDown()
    {
        if (interactOnlyOnce && hasInteracted)
        {
            Debug.Log($"[InteractableNpc] 이미 상호작용한 NPC입니다: {npcId}");
            return;
        }

        if (player == null)
        {
            Debug.LogError("[InteractableNpc] PlayerClickMover가 연결되지 않았습니다.");
            return;
        }

        Transform target = interactionPoint != null ? interactionPoint : transform;

        Debug.Log($"[InteractableNpc] NPC 클릭: {npcId}");

        if (guideObject != null)
            guideObject.SetActive(false);

        player.MoveTo(target.position, () =>
        {
            hasInteracted = true;

            Debug.Log($"[InteractableNpc] NPC 도착 완료: {npcId}");

            if (cameraController != null)
                cameraController.MoveToTalkView();

            StartCoroutine(FadePlayerAfterDelay(fadeDelay));

            Debug.Log("[InteractableNpc] 기존 GamePlayManager 대화 시작 함수 호출할 부분");
        });
    }

    private IEnumerator FadePlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerFadeController != null)
            playerFadeController.FadeOut();
    }
}