/* StageInteractionController.cs
클릭된 target 처리, 플레이어 이동 호출, 카메라 전환 호출, 도착 후 이벤트 실행 */

using UnityEngine;
using UnityEngine.InputSystem;

public class StageInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerClickMover playerMover;
    [SerializeField] private InteractionCameraController cameraController;
    [SerializeField] private Camera raycastCamera;

    [Header("Options")]
    [SerializeField] private bool disableInputWhileInteracting = true;

    private bool isInteracting;

    private void Awake()
    {
        if (raycastCamera == null)
            raycastCamera = Camera.main;
    }

    private void Update()
    {
        if (disableInputWhileInteracting && isInteracting)
            return;

        if (Pointer.current == null)
            return;

        if (Pointer.current.press.wasPressedThisFrame)
            TrySelectTarget();
    }

    private void TrySelectTarget()
    {
        if (raycastCamera == null)
        {
            Debug.LogWarning("[StageInteractionController] Raycast camera is null.");
            return;
        }

        Vector2 screenPosition = Pointer.current.position.ReadValue();
        Ray ray = raycastCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        InteractionTarget target = hit.collider.GetComponentInParent<InteractionTarget>();

        if (target == null)
            return;

        Interact(target);
    }

    public void Interact(InteractionTarget target)
    {
        if (target == null)
            return;

        if (!target.CanInteract)
        {
            Debug.Log($"[StageInteractionController] Already interacted target: {target.TargetId}");
            return;
        }

        if (playerMover == null)
        {
            Debug.LogError("[StageInteractionController] PlayerClickMover is not assigned.");
            return;
        }

        isInteracting = true;

        Debug.Log($"[StageInteractionController] Target clicked: {target.TargetId}");

        target.NotifyInteractionStarted();

        playerMover.MoveTo(target.InteractionPoint.position, () =>
        {
            Debug.Log($"[StageInteractionController] Player arrived at target: {target.TargetId}");

            if (cameraController != null && target.CameraPoint != null)
                cameraController.MoveTo(target.CameraPoint);

            target.NotifyPlayerArrived();

            isInteracting = false;
        });
    }
}