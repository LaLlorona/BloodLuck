using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    private PlayerControls playerControls;

    private Vector2 moveDir;
    public Vector2 MoveDir => moveDir;

    public Action Dash;
    public Action BasicAttack;
    public Action SpecialAttack;

    void Awake()
    {
        playerControls = new();
    }

    void OnEnable()
    {
        playerControls.Enable();
        playerControls.Player.Dash.performed += HandleDash;
        playerControls.Player.Attack.performed += HandleAttack;
        playerControls.Player.SpecialAttack.performed += HandleSpecial;
    }

    void OnDisable()
    {
        playerControls.Disable();
        playerControls.Player.Dash.performed -= HandleDash;
        playerControls.Player.Attack.performed -= HandleAttack;
        playerControls.Player.SpecialAttack.performed -= HandleSpecial;
    }

    private void HandleDash(InputAction.CallbackContext context)
    {
        Dash?.Invoke();
    }

    private void HandleAttack(InputAction.CallbackContext context)
    {
        BasicAttack?.Invoke();
    }

    private void HandleSpecial(InputAction.CallbackContext context)
    {
        SpecialAttack?.Invoke();
    }

    public void GameUpdate()
    {
        moveDir = playerControls.Player.Move.ReadValue<Vector2>().normalized;
    }
}
