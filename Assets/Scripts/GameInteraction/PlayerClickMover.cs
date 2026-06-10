using System;
using UnityEngine;

public class PlayerClickMover : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 25f;
    private float arriveDistance = 3f;

    [Header("Optional")]
    [SerializeField] private Animator animator;

    [SerializeField] private bool setIdleOnArrive = false;

    private Vector3 targetPosition;
    private bool isMoving;
    private Action onArrived;

    public void MoveTo(Vector3 position, Action arrivedCallback)
    {
        targetPosition = position;
        targetPosition.y = transform.position.y;

        onArrived = arrivedCallback;
        isMoving = true;

        if (animator != null)
            animator.SetBool("IsWalking", true);
    }

    private void Update()
    {
        if (!isMoving) return;

        Vector3 current = transform.position;

        Vector3 direction = targetPosition - current;
        direction.y = 0f;

        float distance = direction.magnitude;

        // 도착 범위 안이면 더 이상 회전/이동하지 않고 멈춤
        if (distance <= arriveDistance)
        {
            StopMoving();
            return;
        }

        Vector3 moveDirection = direction.normalized;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // 방향이 충분히 클 때만 회전
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    private void StopMoving()
    {
        isMoving = false;

        if (animator != null && setIdleOnArrive)
            animator.SetBool("IsWalking", false);

        onArrived?.Invoke();
        onArrived = null;
    }
}