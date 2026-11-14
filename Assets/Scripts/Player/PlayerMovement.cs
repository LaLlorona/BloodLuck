using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D rb;
    private PlayerInputManager playerInput;

    [Header("Movement Config")]
    [SerializeField] private float moveSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInputManager>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = playerInput.MoveDir * moveSpeed;
    }

}
