using System;
using UnityEngine;

namespace InGame.System
{
    [Serializable]
    public class BloodLuckInGameParameter
    {
        [SerializeField] private float _monsterSpawnRadius = 5;
        [SerializeField, Min(0f)] private float _monsterMoveSpeed = 2f;

        [Header("Monster HP Trail")]
        [Tooltip("Seconds the white trail waits at the old HP after the first damage.")]
        [SerializeField, Min(0f)] private float _hpTrailHoldSeconds = 0.4f;
        [Tooltip("Minimum drain speed of the trail (hp ratio per second).")]
        [SerializeField, Min(0f)] private float _hpTrailMinSpeed = 0.3f;
        [Tooltip("Extra drain speed per unit of gap, so big bursts drain faster.")]
        [SerializeField, Min(0f)] private float _hpTrailSpeedPerGap = 2f;

        [Header("Monster Attack (sector)")]
        [Tooltip("When the monster gets this close to the player, it stops and starts charging its attack.")]
        [SerializeField, Min(0f)] private float _monsterAttackTriggerDistance = 2f;
        [Tooltip("Seconds the sector is shown before the damage is applied.")]
        [SerializeField, Min(0.01f)] private float _monsterAttackChargeSeconds = 1.5f;
        [Tooltip("Seconds the monster stays idle after its attack before chasing again.")]
        [SerializeField, Min(0f)] private float _monsterAttackWaitSeconds = 1f;
        [SerializeField, Min(0.1f)] private float _monsterAttackRadius = 3f;
        [SerializeField, Range(1f, 360f)] private float _monsterAttackAngle = 90f;

        [Header("Monster Ranged Attack")]
        [Tooltip("The ranged monster fires when the player is within this distance.")]
        [SerializeField, Min(0f)] private float _monsterRangedAttackRange = 6f;
        [Tooltip("Seconds the ranged monster stands still after firing.")]
        [SerializeField, Min(0f)] private float _monsterRangedStopSeconds = 1.5f;
        [SerializeField, Min(0f)] private float _monsterProjectileSpeed = 6f;
        [SerializeField, Min(0.01f)] private float _monsterProjectileHitRadius = 0.4f;
        [SerializeField, Min(0.1f)] private float _monsterProjectileLifetime = 5f;

        [Header("Monster Hit Reaction")]
        [Tooltip("Seconds the monster stays frozen after being hit.")]
        [SerializeField, Min(0f)] private float _monsterHitStunSeconds = 1f;
        [SerializeField, Min(0f)] private float _monsterKnockbackDistance = 0.5f;
        [SerializeField, Min(0f)] private float _monsterKnockbackSeconds = 0.15f;

        [Header("Player")]
        [SerializeField, Min(1f)] private float _playerMaxHp = 100f;
        [Tooltip("Current HP can never be spent below this value.")]
        [SerializeField, Min(1f)] private float _minHpAfterSpend = 1f;
        [SerializeField, Min(0f)] private float _playerAttack = 10f;
        [SerializeField, Min(0f)] private float _playerThirdAttackDamage = 20f;
        [SerializeField, Min(0f)] private float _playerAttackSpeed = 1f;

        [Header("Dash Damage")]
        [Tooltip("A monster is hit by a dash when it is within this distance of the dash line.")]
        [SerializeField, Min(0f)] private float _dashDamageHitWidth = 1f;

        [Header("Chain Lightning")]
        [Tooltip("A monster is chained only when it is within this distance of the previous monster.")]
        [SerializeField, Min(0f)] private float _chainLightningRange = 3f;
        [Tooltip("Maximum number of monsters one chain can jump to.")]
        [SerializeField, Min(1)] private int _chainLightningMaxCount = 3;
        [SerializeField, Min(0.01f)] private float _chainLightningLineSeconds = 0.2f;
        [SerializeField, Min(0.01f)] private float _chainLightningLineWidth = 0.1f;
        [Tooltip("Glow color. The core line is always white.")]
        [SerializeField] private Color _chainLightningColor = new(0.4f, 0.5f, 1f, 1f);
        [Tooltip("Glow line width is the line width times this.")]
        [SerializeField, Min(1f)] private float _chainLightningGlowWidthScale = 3f;
        [Tooltip("Approximate distance between zigzag points.")]
        [SerializeField, Min(0.05f)] private float _chainLightningSegmentLength = 0.3f;
        [Tooltip("Max sideways offset of each zigzag point.")]
        [SerializeField, Min(0f)] private float _chainLightningJitter = 0.2f;
        [Tooltip("Seconds between re-randomizing the zigzag, for a flicker.")]
        [SerializeField, Min(0.01f)] private float _chainLightningFlickerSeconds = 0.04f;

        [Header("Slot Machine")]
        [Range(0f, 1f)] [SerializeField] private float _slotJackpotChance = 0.2f;
        [Tooltip("Current HP spent on each spin.")]
        [SerializeField, Min(0f)] private float _slotSpinHpCost = 10f;

        public float HpTrailHoldSeconds => _hpTrailHoldSeconds;
        public float HpTrailMinSpeed => _hpTrailMinSpeed;
        public float HpTrailSpeedPerGap => _hpTrailSpeedPerGap;
        public float SlotJackpotChance => _slotJackpotChance;
        public float MonsterRangedAttackRange => _monsterRangedAttackRange;
        public float MonsterRangedStopSeconds => _monsterRangedStopSeconds;
        public float MonsterProjectileSpeed => _monsterProjectileSpeed;
        public float MonsterProjectileHitRadius => _monsterProjectileHitRadius;
        public float MonsterProjectileLifetime => _monsterProjectileLifetime;

        public float MonsterHitStunSeconds => _monsterHitStunSeconds;
        public float MonsterKnockbackDistance => _monsterKnockbackDistance;
        public float MonsterKnockbackSeconds => _monsterKnockbackSeconds;

        public float DashDamageHitWidth => _dashDamageHitWidth;
        public float ChainLightningRange => _chainLightningRange;
        public int ChainLightningMaxCount => _chainLightningMaxCount;
        public float ChainLightningLineSeconds => _chainLightningLineSeconds;
        public float ChainLightningLineWidth => _chainLightningLineWidth;
        public Color ChainLightningColor => _chainLightningColor;
        public float ChainLightningGlowWidthScale => _chainLightningGlowWidthScale;
        public float ChainLightningSegmentLength => _chainLightningSegmentLength;
        public float ChainLightningJitter => _chainLightningJitter;
        public float ChainLightningFlickerSeconds => _chainLightningFlickerSeconds;

        public float PlayerMaxHp => _playerMaxHp;
        public float MinHpAfterSpend => _minHpAfterSpend;
        public float SlotSpinHpCost => _slotSpinHpCost;
        public float PlayerAttack => _playerAttack;
        public float PlayerThirdAttackDamage => _playerThirdAttackDamage;
        public float PlayerAttackSpeed => _playerAttackSpeed;

        public float MonsterSpawnRadius => _monsterSpawnRadius;
        public float MonsterMoveSpeed => _monsterMoveSpeed;
        public float MonsterAttackTriggerDistance => _monsterAttackTriggerDistance;
        public float MonsterAttackChargeSeconds => _monsterAttackChargeSeconds;
        public float MonsterAttackWaitSeconds => _monsterAttackWaitSeconds;
        public float MonsterAttackRadius =>_monsterAttackRadius;
        public float MonsterAttackAngle => _monsterAttackAngle;
    }
}
