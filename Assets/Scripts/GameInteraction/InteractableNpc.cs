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
    [SerializeField] private GamePlayManager gamePlayManager;
    [SerializeField] private Collider npcCollider;
    [SerializeField] private StageReactionController stageReactionController;

    [Header("Options")]
    [SerializeField] private bool interactOnlyOnce = true;
    [SerializeField] private float fadeDelay = 0.1f;

    private bool hasInteracted;
    private bool isInteracting;

    private void Awake()
    {
        if (npcCollider == null)
            npcCollider = GetComponent<Collider>();
    }

    private void OnMouseDown()
    {
        if (isInteracting)
            return;

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

        isInteracting = true;

        Transform target = interactionPoint != null ? interactionPoint : transform;

        Debug.Log($"[InteractableNpc] NPC 클릭: {npcId}");

        if (stageReactionController != null)
            stageReactionController.OnInteractionStarted();
        else if (guideObject != null)
            guideObject.SetActive(false);

        player.MoveTo(target.position, () =>
        {
            hasInteracted = true;

            Debug.Log($"[InteractableNpc] NPC 도착 완료: {npcId}");

            if (npcCollider != null)
                npcCollider.enabled = false;

            if (stageReactionController != null)
            {
                stageReactionController.OnTalkStarted();
            }
            else
            {
                if (cameraController != null)
                    cameraController.MoveToTalkView();

                StartCoroutine(FadePlayerAfterDelay(fadeDelay));
            }

            if (gamePlayManager != null)
                gamePlayManager.StartQuestSessionFromNpc();
        });
    }

    private IEnumerator FadePlayerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerFadeController != null)
            playerFadeController.FadeOut();
    }
}