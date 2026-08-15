/* InteractionTarget.cs
NPC 정보, interactionPoint, cameraPoint, onInteractionStarted 이벤트 관련 코드 */

using UnityEngine;
using UnityEngine.Events;

public class InteractionTarget : MonoBehaviour
{
    [Header("Target Info")]
    [SerializeField] private string targetId = "npc";

    [Header("Points")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private Transform cameraPoint;

    [Header("Options")]
    [SerializeField] private bool interactOnlyOnce = true;
    [SerializeField] private Collider targetCollider;

    [Header("Events")]
    [SerializeField] private UnityEvent onInteractionStarted;
    [SerializeField] private UnityEvent onPlayerArrived;

    private bool hasInteracted;

    public string TargetId => targetId;
    public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;
    public Transform CameraPoint => cameraPoint;
    public Collider TargetCollider => targetCollider;
    public bool CanInteract => !interactOnlyOnce || !hasInteracted;

    private void Awake()
    {
        if (targetCollider == null)
            targetCollider = GetComponent<Collider>();
    }

    public void NotifyInteractionStarted()
    {
        Debug.Log($"[InteractionTarget] Interaction started: {targetId}");
        onInteractionStarted?.Invoke();
    }

    public void NotifyPlayerArrived()
    {
        hasInteracted = true;

        if (targetCollider != null && interactOnlyOnce)
            targetCollider.enabled = false;

        Debug.Log($"[InteractionTarget] Player arrived: {targetId}");
        onPlayerArrived?.Invoke();
    }
}