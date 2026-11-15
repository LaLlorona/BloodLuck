using System;
using System.Collections.Generic;
using InGame.Monsters;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    enum AttackType
    {
        BasicAttack,
        SpecialAttack
    }

    enum AttackState
    {
        Cooldown,
        Ready
    }

    [Header("References")]
    [SerializeField] private Collider2D weaponCollider;
    [SerializeField] LayerMask enemyLayer;
    private PlayerInputManager playerInput;

    [Header("Attack Settings")]
    [SerializeField] private float comboTime;
    [SerializeField] private float basicAttackCooldown;
    [SerializeField] private float specialAttackCooldown;
    private float attackCooldown;
    private AttackState attackState;
    private AttackType lastAttackType;
    private float lastBasicAttackTime;
    private int currentAttackComboIndex;
    ContactFilter2D filter;
    List<Collider2D> results = new();

    void Awake()
    {
        playerInput = GetComponent<PlayerInputManager>();

        filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
    }

    void OnEnable()
    {
        playerInput.BasicAttack += HandleAttack;
        playerInput.SpecialAttack += HandleSpecial;
    }

    void OnDisable()
    {
        playerInput.BasicAttack -= HandleAttack;
        playerInput.SpecialAttack -= HandleSpecial;
    }

    public void GameUpdate(float deltaTime)
    {
        HandleAttackState(deltaTime);
        if (attackCooldown > 0f)
        {
            attackCooldown -= deltaTime;
            if (attackCooldown < 0f)
            {
                attackCooldown = 0f;
            }
        }
    }

    void HandleAttackState(float deltaTime)
    {
        switch (attackState)
        {
            case AttackState.Cooldown:
                attackCooldown -= deltaTime;
                if (attackCooldown <= 0f)
                {
                    attackCooldown = 0f;
                    attackState = AttackState.Ready;
                }
                break;
            default:
                break;
        }
    }

    private void HandleAttack()
    {
        // TODO: PLAY ANIMATION BASED ON currentAttackComboIndex
        if (attackState != AttackState.Ready) return;

        // Check if any enemies are in the collider
        results.Clear();
        weaponCollider.Overlap(filter, results);
        foreach (Collider2D col in results)
        {
            if (col.TryGetComponent(out BloodLuckMonsterComponent enemy))
            {
                // TODO: DEAL DAMAGE TO enemy HERE (DEAL KNOCKBACK, FLASH, ETC)
                //ToDO: calculatePlayerDamage
                enemy.OnMonsterDamage(5.0f);
                
            }
        }

        // Increment combo
        if (lastAttackType == AttackType.BasicAttack && Time.time - lastBasicAttackTime <= comboTime)
        {
            currentAttackComboIndex++;
            if (currentAttackComboIndex >= 3)
            {
                currentAttackComboIndex = 0;
            }
        }
        else
        {
            currentAttackComboIndex = 1;
        }

        lastBasicAttackTime = Time.time;
        attackCooldown = basicAttackCooldown;
        attackState = AttackState.Cooldown;
    }

    private void HandleSpecial()
    {
        if (attackState != AttackState.Ready) return;

        results.Clear();
        weaponCollider.Overlap(filter, results);
        foreach (Collider2D col in results)
        {
            if (col.TryGetComponent(out BloodLuckMonsterComponent enemy))
            {
                // TODO: DEAL DAMAGE TO enemy HERE (DEAL KNOCKBACK, FLASH, ETC)
            }
        }

        attackCooldown = specialAttackCooldown;
        attackState = AttackState.Cooldown;
    }
}
