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
    [SerializeField] private TrailRenderer dashTrailRenderer;
    [SerializeField] private float maxDashDistance = 3f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField, Range(1f, 4f)] private float dashEasePower = 2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private int dashCharges = 1;
    [SerializeField] private float dashBufferTime = 0.12f;
    [HideInInspector] public float DashCooldown => dashCooldown;
    [HideInInspector] public int DashCharges => dashCharges;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private float skinWidth = 0.02f;

    private bool isDashing;
    private Vector2 dashDir;
    private float dashElapsed;
    private float dashProgress;
    private int currentCharges;
    private float rechargeTimer;
    private float dashBufferTimer;

    [Header("Rotation Settings")]
    [SerializeField] private float spriteAngleOffset;
    [SerializeField] private int facingIndex; // 0-7 for the 8 directions

    void Awake()
    {
        playerInput = GetComponent<PlayerInputManager>();
        playerAttack = GetComponent<PlayerAttack>();
        playerAnimation = GetComponent<PlayerAnimationController>();
        currentCharges = dashCharges;
    }

    public void InitializePlayerMovement(BloodLuckPlayerManager manager)
    {
        playerManager = manager;
    }

    void OnEnable()  { playerInput.Dash += HandleDash; }
    void OnDisable() { playerInput.Dash -= HandleDash; }

    public void GameUpdate(float deltaTime)
    {
        UpdateDashCharges(deltaTime);

        if (dashBufferTimer > 0f)
        {
            dashBufferTimer -= deltaTime;
            if (!isDashing && currentCharges > 0)
                StartDash();
        }

        if (isDashing)
        {
            UpdateDash(deltaTime);
            return;
        }

        if (!playerAttack.IsAttacking)
        {
            UpdateRotation();
        }

        if (!playerAttack.IsAttacking)
        {
            if (playerInput.MoveDir.sqrMagnitude > 0f)
            {
                stepTimer -= deltaTime;
                if (stepTimer <= 0f)
                {
                    AudioManager.Instance?.PlayFootstep();
                    stepTimer = stepInterval;
                }

                lastMoveDir = playerInput.MoveDir;
                MoveWithCollision(playerInput.MoveDir.normalized, moveSpeed * deltaTime);
                playerAnimation.UpdateAnimState(PlayerAnimationState.Run);
            }
            else
            {
                stepTimer = 0f;
                playerAnimation.UpdateAnimState(PlayerAnimationState.Idle);
            }
        }
    }

    private void HandleDash()
    {
        dashBufferTimer = dashBufferTime;
    }

    private void StartDash()
    {
        dashBufferTimer = 0f;
        currentCharges--;

        Vector2 dir = playerInput.MoveDir;
        if (dir.sqrMagnitude < 0.0001f) dir = lastMoveDir;
        if (dir.sqrMagnitude < 0.0001f)
        {
            float a = facingIndex * 45f * Mathf.Deg2Rad;
            dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        }
        dashDir = dir.normalized;
        lastMoveDir = dashDir;

        dashElapsed = 0f;
        dashProgress = 0f;
        isDashing = true;

        dashTrailRenderer.emitting = true;

        // TODO: cancel current attack here
        // TODO: Play SFX
        // TODO: Play dash animation
    }

    private void UpdateDash(float deltaTime)
    {
        dashElapsed += deltaTime;
        float t = Mathf.Clamp01(dashElapsed / dashDuration);
        float eased = 1f - Mathf.Pow(1f - t, dashEasePower);

        float step = maxDashDistance * (eased - dashProgress);
        dashProgress = eased;

        Vector3 dashStart = transform.position;
        bool blocked = MoveWithCollision(dashDir, step) < step - 0.0001f;

        if (t >= 1f || blocked)
        {
            isDashing = false;
            dashTrailRenderer.emitting = false;
            playerManager.HandlePlayerDash(dashStart, transform.position);
        }
    }

    private void UpdateDashCharges(float deltaTime)
    {
        if (currentCharges >= dashCharges) { 
            rechargeTimer = 0f; 
            return; 
        }

        rechargeTimer += deltaTime;
        if (rechargeTimer >= dashCooldown)
        {
            rechargeTimer -= dashCooldown;
            currentCharges = dashCharges;
        }
    }

    private float MoveWithCollision(Vector2 dir, float distance)
    {
        if (distance <= 0f) return 0f;

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, collisionRadius, dir, distance + skinWidth, obstacleMask);
        float allowed = hit.collider != null ? Mathf.Max(0f, hit.distance - skinWidth) : distance;

        transform.position += (Vector3)(dir * allowed);
        return allowed;
    }

    public void UpdateRotation()
    {
        Vector2 dir = playerInput.MoveDir;
        if (dir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        int step = Mathf.RoundToInt(angle / 45f);
        facingIndex = (step % 8 + 8) % 8;
        float snappedAngle = facingIndex * 45f + spriteAngleOffset;

        playerAnimation.UpdateDirection(facingIndex);
        playerAttack.WeaponCollider.transform.rotation = Quaternion.Euler(0f, 0f, snappedAngle);
    }

    public void SetDashCharges(int charges)
    {
        dashCharges = charges;
    }

    public void SetDashCooldown(float cooldown)
    {
        dashCooldown = cooldown;
    }
}