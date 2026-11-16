using System;
using System.Collections.Generic;
using InGame.Managers;
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

    // The combo index is incremented after each attack, so it is 2 right before the third hit.
    private const int ThirdAttackComboIndex = 2;

    [Header("References")]
    public Collider2D WeaponCollider;
    [SerializeField] LayerMask enemyLayer;
    private PlayerInputManager playerInput;

    [Header("Slash Effect")]
    [SerializeField] private float slashRadius = 2f;
    [SerializeField] private float slashAngle = 140f;
    private BloodLuckEffectManager effectManager;
    private BloodLuckPlayerStateManager playerStateManager;
    private BloodLuckMonsterManager monsterManager;
    private readonly List<BloodLuckMonster> hitMonsters = new();
    private PlayerMovement playerMovement;

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
        playerMovement = GetComponent<PlayerMovement>();

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

    public void InitializePlayerAttack(
        BloodLuckEffectManager manager,
        BloodLuckPlayerStateManager stateManager,
        BloodLuckMonsterManager monsterManager)
    {
        effectManager = manager;
        playerStateManager = stateManager;
        this.monsterManager = monsterManager;
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

        bool isThirdAttack = currentAttackComboIndex == ThirdAttackComboIndex;
        PlaySlashEffect(isThirdAttack ? Color.red : Color.white);
        float damage = isThirdAttack ? playerStateManager.ThirdAttack : playerStateManager.Attack;

        // Check if any enemies are in the collider
        results.Clear();
        WeaponCollider.Overlap(filter, results);
        hitMonsters.Clear();
        foreach (Collider2D col in results)
        {
            if (col.TryGetComponent(out BloodLuckMonsterComponent enemy))
            {
                hitMonsters.Add(enemy.Monster);
            }
        }

        monsterManager.ApplyChainLightning(hitMonsters, playerStateManager.ChainLightningDamage);

        foreach (var monster in hitMonsters)
        {
            monster.OnMonsterDamage(damage);
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

    private void PlaySlashEffect(Color color)
    {
        if (effectManager == null) return;

        Vector2 direction = playerMovement != null && playerMovement.lastMoveDir.sqrMagnitude > 0.0001f
            ? playerMovement.lastMoveDir
            : (Vector2)transform.right;

        // Alternate the swing direction every combo hit.
        bool clockwise = currentAttackComboIndex % 2 == 0;
        effectManager.PlaySlash(transform.position, direction, slashRadius, slashAngle, color, clockwise);
    }

    private void HandleSpecial()
    {
        // if (attackState != AttackState.Ready) return;

        // results.Clear();
        // WeaponCollider.Overlap(filter, results);
        // foreach (Collider2D col in results)
        // {
        //     if (col.TryGetComponent(out BloodLuckMonsterComponent enemy))
        //     {
        //         // TODO: DEAL DAMAGE TO enemy HERE (DEAL KNOCKBACK, FLASH, ETC)
        //     }
        // }

        // attackCooldown = specialAttackCooldown;
        // attackState = AttackState.Cooldown;

        // WE DIDN'T HAVE TO IMPLEMENT SPECIALS LOL
        return;
    }
}
