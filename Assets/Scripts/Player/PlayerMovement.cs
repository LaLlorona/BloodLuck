using System;
using UnityEngine;
using BloodLuck;
using InGame.Managers;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    private PlayerInputManager playerInput;
    private PlayerAttack playerAttack;
    private PlayerAnimationController playerAnimation;
    private BloodLuckPlayerManager playerManager;

    [Header("Movement Config")]
    [SerializeField] private float moveSpeed;
    [SerializeField, Min(0.1f)] private float stepInterval = 0.35f;
    private float stepTimer;
    [HideInInspector] public Vector2 lastMoveDir;

    [Header("Dash Config")]
    [SerializeField] private float maxDashDistance;
    private bool isDashing;

    [Header("Rotation Settings")]
    [SerializeField] private float spriteAngleOffset;
    [SerializeField] private int facingIndex; // 0-7 for the 8 directions

    void Awake()
    {
        playerInput = GetComponent<PlayerInputManager>();
        playerAttack = GetComponent<PlayerAttack>();
        playerAnimation = GetComponent<PlayerAnimationController>();
    }

    public void InitializePlayerMovement(BloodLuckPlayerManager manager)
    {
        playerManager = manager;
    }

    void OnEnable()
    {
        playerInput.Dash += HandleDash;
    }

    void OnDisable()
    {
        playerInput.Dash -= HandleDash;
    }

    public void GameUpdate(float deltaTime)
    {
        if (!playerAttack.isAttacking)
        {
            UpdateRotation();
        }

        if (isDashing)
        {
            Vector3 lastMoveDirVec3 = new(lastMoveDir.x, lastMoveDir.y, 0);
            Vector3 dashStart = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, transform.position + (lastMoveDirVec3 * maxDashDistance), maxDashDistance);
            isDashing = false;
            playerManager.HandlePlayerDash(dashStart, transform.position);
        }

        if (!isDashing && !playerAttack.isAttacking)
        {
            if (playerInput.MoveDir.magnitude != 0)
            {
                stepTimer -= deltaTime;
                if (stepTimer <= 0f)
                {
                    AudioManager.Instance?.PlayFootstep();
                    stepTimer = stepInterval;
                }
                lastMoveDir = playerInput.MoveDir;
                Vector3 moveDirVec3 = new(playerInput.MoveDir.x, playerInput.MoveDir.y, 0);
                transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDirVec3, moveSpeed * deltaTime);
                playerAnimation.UpdateAnimState(PlayerAnimationState.Run);
            }
            else
            {
                stepTimer = 0f;
                playerAnimation.UpdateAnimState(PlayerAnimationState.Idle);
            }

        }
    }

    public void UpdateRotation()
    {
        Vector2 dir = playerInput.MoveDir;

        if (dir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        int step = Mathf.RoundToInt(angle / 45f);
        facingIndex = (step % 8 + 8) % 8;
        float snappedAngle = facingIndex * 45f + spriteAngleOffset;

        /*
            TODO: REPLACE THIS CODE WITH ONLY ROTATING THE WEAPON COLLIDER
            INSTEAD OF ROTATING THE WHOLE PLAYER. ALSO CHANGE THE SPRITE
            OF THE PLAYER DEPENDING ON WHICH DIRECTION IT IS FACING.
        */
        playerAnimation.UpdateDirection(facingIndex);
        playerAttack.WeaponCollider.transform.rotation = Quaternion.Euler(0f, 0f, snappedAngle);
    }

    private void HandleDash()
    {
        if (isDashing) return;
        // TODO: Play Animation
        isDashing = true;
        AudioManager.Instance?.PlayDash();
    }
}
