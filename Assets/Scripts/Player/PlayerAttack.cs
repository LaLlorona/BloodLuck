using System.Collections.Generic;
using InGame.Managers;
using InGame.Monsters;
using BloodLuck;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    enum AttackType
    {
        BasicAttack,
        SpecialAttack
    }

    public enum AttackState
    {
        Cooldown,
        Ready,
        Attack1,
        Attack2,
        Attack3,
    }

    // The combo index is incremented after each attack, so it is 2 right before the third hit.
    private const int ThirdAttackComboIndex = 2;

    [Header("References")]
    public Collider2D WeaponCollider;
    [SerializeField] LayerMask enemyLayer;
    private PlayerInputManager playerInput;
    private PlayerAnimationController playerAnimation;

    [Header("Slash Effect")]
    [SerializeField] private float slashRadius = 2f;
    [SerializeField] private float slashAngle = 140f;
    private BloodLuckEffectManager effectManager;
    private BloodLuckPlayerStateManager playerStateManager;
    private BloodLuckMonsterManager monsterManager;
    private readonly List<BloodLuckMonster> hitMonsters = new();
    private PlayerMovement playerMovement;

    [Header("Attack Settings")]
    public bool IsAttacking;
    [SerializeField] private float comboTime;
    [SerializeField] private float basicAttackCooldown;
    [SerializeField] private float specialAttackCooldown;
    public bool IsNextAttackQueued;
    public bool IsAttackInterruptable;
    private float attackCooldown;
    private AttackState attackState;
    private int currentAttackComboIndex;
    ContactFilter2D filter;
    List<Collider2D> results = new();

    private bool IsThirdAttack => attackState == AttackState.Attack3;

    void Awake()
    {
        playerInput = GetComponent<PlayerInputManager>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAnimation = GetComponent<PlayerAnimationController>();

        filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);

        attackState = AttackState.Ready;
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
        playerInput.InitializeInputManager(playerStateManager);

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
            case AttackState.Attack1:
                Debug.Log($"Attack Queued: {IsNextAttackQueued} | Attack Interruptable: {IsAttackInterruptable}");
                if (IsNextAttackQueued && IsAttackInterruptable)
                {
                    attackState = AttackState.Ready;
                    IsNextAttackQueued = false;
                    IsAttackInterruptable = false;
                    playerMovement.UpdateRotation();
                    HandleAttack();
                }
                break;
            case AttackState.Attack2:
                if (IsNextAttackQueued && IsAttackInterruptable)
                {
                    attackState = AttackState.Ready;
                    IsNextAttackQueued = false;
                    IsAttackInterruptable = false;
                    playerMovement.UpdateRotation();
                    HandleAttack();
                }
                break;
            default:
                break;
        }
    }

    private void HandleAttack()
    {

        if (attackState == AttackState.Attack1 || attackState == AttackState.Attack2 || attackState == AttackState.Attack3)
        {
            IsNextAttackQueued = true;
        }

        if (attackState != AttackState.Ready) return;

        if (currentAttackComboIndex == 0)
        {
            playerAnimation.UpdateAnimState(PlayerAnimationState.Attack1, playerStateManager.AttackSpeed);
        }
        else if (currentAttackComboIndex == 1)
        {
            playerAnimation.UpdateAnimState(PlayerAnimationState.Attack2, playerStateManager.AttackSpeed);
        }
        else if (currentAttackComboIndex == 2)
        {
            playerAnimation.UpdateAnimState(PlayerAnimationState.Attack3, playerStateManager.AttackSpeed);
        }

        // note: PlaySlashEffect has been moved to the attack animation itself
        AudioManager.Instance?.PlaySwing();
        float damage = IsThirdAttack ? playerStateManager.ThirdAttack : playerStateManager.Attack;

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

        foreach (var monster in hitMonsters)
        {
            monster.OnMonsterDamage(damage, BloodLuckDamageSource.Attack);
        }
        if (hitMonsters.Count > 0) AudioManager.Instance?.PlayHit();

        // Increment combo
        currentAttackComboIndex++;
        // if (lastAttackType == AttackType.BasicAttack && Time.time - lastBasicAttackTime <= comboTime)
        // {
        //     currentAttackComboIndex++;
        //     if (currentAttackComboIndex >= 3)
        //     {
        //         currentAttackComboIndex = 0;
        //     }
        // }
        // else
        // {
        //     currentAttackComboIndex = 1;
        // }

        // lastBasicAttackTime = Time.time;
        // attackCooldown = basicAttackCooldown;
        // attackState = AttackState.Cooldown;
    }

    public void StartAttack(AttackState state)
    {
        attackState = state;
        IsAttacking = true;
    }

    // This is a function that an animation event can use to indicate the attack in the animation has made "contact"
    public void AttackContact()
    {
        IsAttackInterruptable = true;
    }

    public void FinishAttack()
    {
        if (IsNextAttackQueued)
        {
            attackState = AttackState.Ready;
            IsNextAttackQueued = false;
            if (currentAttackComboIndex >= 3)
            {
                currentAttackComboIndex = 0;
                IsAttacking = false;
                attackState = AttackState.Ready;
            }
            else
            {
                HandleAttack();
            }
        }
        else
        {
            currentAttackComboIndex = 0;
            IsAttacking = false;
            attackState = AttackState.Ready;
        }
    }


    public void PlaySlashEffect()
    {
        if (effectManager == null) return;

        Color color = Color.white;
        if (IsThirdAttack)
        {
            color = Color.red;
        }

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
