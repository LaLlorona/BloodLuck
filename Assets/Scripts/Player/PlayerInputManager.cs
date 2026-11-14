using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    private PlayerControls playerControls;
    private Vector2 moveDir;
    public Vector2 MoveDir => moveDir;

    void Awake()
    {
        playerControls = new();
        playerControls.Enable();
    }

    void Update()
    {
        moveDir = playerControls.Player.Move.ReadValue<Vector2>().normalized;
        Debug.Log(moveDir);
    }
}
