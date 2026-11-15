using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    private PlayerControls playerControls;
    private Vector2 moveDir;
    public Vector2 MoveDir => moveDir;
    public Action Dash;

    void Awake()
    {
        playerControls = new();
    }

    void OnEnable()
    {
        playerControls.Enable();
        playerControls.Player.Dash.performed += HandleDash;
    }

    void OnDisable()
    {
        playerControls.Disable();
        playerControls.Player.Dash.performed -= HandleDash;
    }

    private void HandleDash(InputAction.CallbackContext context)
    {
        Dash.Invoke();
    }

    void Update()
    {
        moveDir = playerControls.Player.Move.ReadValue<Vector2>().normalized;
    }
}
