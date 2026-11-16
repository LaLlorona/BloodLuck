using System;
using InGame.Managers;
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
    private BloodLuckPlayerStateManager _playerStateManager;

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

    public void InitializeInputManager(BloodLuckPlayerStateManager playerStateManager)
    {
        _playerStateManager = playerStateManager;
    }

    private void HandleDash(InputAction.CallbackContext context)
    {
        if (_playerStateManager.IsGameProceeding == false)
        {
            return;
        }
        Dash?.Invoke();
    }

    private void HandleAttack(InputAction.CallbackContext context)
    {
        if (_playerStateManager.IsGameProceeding == false)
        {
            return;
        }
        BasicAttack?.Invoke();
    }

    private void HandleSpecial(InputAction.CallbackContext context)
    {
        if (_playerStateManager.IsGameProceeding == false)
        {
            return;
        }
        SpecialAttack?.Invoke();
    }

    public void GameUpdate()
    {
        moveDir = playerControls.Player.Move.ReadValue<Vector2>().normalized;
    }
}
