using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D rb;
    private PlayerInputManager playerInput;

    [Header("Movement Config")]
    [SerializeField] private float moveSpeed;
    private Vector2 lastMoveDir;

    [Header("Dash Config")]
    [SerializeField] private float maxDashDistance;
    private bool isDashing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInputManager>();
    }

    void OnEnable()
    {
        playerInput.Dash += HandleDash;
    }

    void OnDisable()
    {
        playerInput.Dash -= HandleDash;
    }


    void FixedUpdate()
    {
        if (isDashing)
        {
            Vector3 lastMoveDirVec3 = new(lastMoveDir.x, lastMoveDir.y, 0);
            transform.position = Vector3.MoveTowards(transform.position, transform.position + (lastMoveDirVec3 * maxDashDistance), maxDashDistance);
            isDashing = false;
        }

        if (!isDashing)
        {
            if (playerInput.MoveDir.magnitude != 0)
            {
                lastMoveDir = playerInput.MoveDir;
                Vector3 moveDirVec3 = new(playerInput.MoveDir.x, playerInput.MoveDir.y, 0);
                transform.position = Vector3.MoveTowards(transform.position, transform.position + moveDirVec3, moveSpeed * Time.deltaTime);
            }
        }
    }

    private void HandleDash()
    {
        // TODO: Play SFX
        // TODO: Play Animation
        isDashing = true;
    }
}
